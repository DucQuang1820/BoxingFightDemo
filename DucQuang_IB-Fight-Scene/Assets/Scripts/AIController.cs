using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    private static readonly int IsBeingHit = Animator.StringToHash("isBeingHit");
    private static readonly int HitIndex = Animator.StringToHash("HitIndex");
    private static readonly int IsKO = Animator.StringToHash("isKO");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int IsPunch = Animator.StringToHash("isPunch");
    private static readonly int IsInRange = Animator.StringToHash("isInRange");
    private static readonly int PunchIndex = Animator.StringToHash("PunchIndex");

    public Transform player;
    public int health = 20;

    public Animator animator;
    private NavMeshAgent agent;

    private bool isInRange = false;
    private Coroutine punchRoutine;
    private Collider lastEnemyHit;

    public bool IsDead { get; private set; } = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (IsDead || player == null) return;

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null && playerController.IsDead)
        {
            agent.isStopped = true; 
            animator.SetFloat(Speed, 0f); 
            return;
        }
        
        agent.SetDestination(player.position);
        animator.SetFloat(Speed, agent.velocity.magnitude);

        LookAtPlayer();
    }

    private void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    public void PlayHitReaction(string type = "", int damage = 10)
    {
        if (IsDead) return;

        health -= damage;
        if (health <= 0)
        {
            Die();
            return;
        }

        animator.SetBool(IsBeingHit, true);

        switch (type)
        {
            case "head":
                animator.SetFloat(HitIndex, 0);
                break;
            case "kidney":
                animator.SetFloat(HitIndex, 1);
                break;
            case "stomach":
                animator.SetFloat(HitIndex, 2);
                break;
            default:
                animator.SetFloat(HitIndex, 0);
                break;
        }

        StartCoroutine(ResetHit());
    }

    private IEnumerator ResetHit()
    {
        yield return new WaitForSeconds(0.2f);
        animator.SetBool(IsBeingHit, false);
    }

    private void Die()
    {
        IsDead = true;

        if (punchRoutine != null)
        {
            StopCoroutine(punchRoutine);
            punchRoutine = null;
        }
        GameController.Instance.TriggerGameWin();
        animator.SetBool(IsPunch, false);
        animator.SetBool(IsBeingHit, false);
        animator.SetBool(IsInRange, false);
        animator.SetTrigger(IsKO);  

        agent.enabled = false;

        var coll = GetComponent<Collider>();
        var rb = GetComponent<Rigidbody>();

        if (coll != null) coll.enabled = false;
        if (rb != null) rb.isKinematic = true;

        enabled = false; 
    }

    private void OnCollisionEnter(Collision other)
    {
        if (IsDead) return;

        if (other.gameObject.CompareTag("Player") && punchRoutine == null)
        {
            lastEnemyHit = other.collider;
            isInRange = true;
            punchRoutine = StartCoroutine(PunchLoop());
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInRange = false;

            if (punchRoutine != null)
            {
                StopCoroutine(punchRoutine);
                punchRoutine = null;
                animator.SetBool(IsPunch, false);
                if (other.collider == lastEnemyHit)
                    lastEnemyHit = null;
            }
        }
    }

    private IEnumerator PunchLoop()
    {
        while (isInRange && lastEnemyHit != null && !IsDead)
        {
            PlayerController playerController = lastEnemyHit.GetComponent<PlayerController>();
            if (playerController != null && playerController.IsDead)
            {
                animator.SetBool(IsPunch, false);
                punchRoutine = null;
                yield break; 
            }

            float index = Random.Range(0, 4);
            animator.SetFloat(PunchIndex, index);
            animator.SetBool(IsPunch, true);
            animator.SetTrigger(IsInRange);

            yield return new WaitForSeconds(1.2f);
        }

        animator.SetBool(IsPunch, false);
    }
    public void HitEnemy(string type)
    {
        if (lastEnemyHit != null)
        {
            PlayerController pl = lastEnemyHit.GetComponent<PlayerController>();
            if (pl != null)
            {
                pl.PlayHitReaction(type);
            }
        }
    }
}
