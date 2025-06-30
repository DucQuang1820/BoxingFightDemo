using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class AllyAIController : MonoBehaviour
{
    private static readonly int IsBeingHit = Animator.StringToHash("isBeingHit");
    private static readonly int HitIndex = Animator.StringToHash("HitIndex");
    private static readonly int IsKO = Animator.StringToHash("isKO");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int IsPunch = Animator.StringToHash("isPunch");
    private static readonly int IsInRange = Animator.StringToHash("isInRange");
    private static readonly int PunchIndex = Animator.StringToHash("PunchIndex");

    public Transform targetEnemy;
    public int health = 20;
    public TextMeshProUGUI hpText;

    public Animator animator;
    private NavMeshAgent agent;

    private bool isInRange = false;
    private Coroutine punchRoutine;
    public bool IsDead { get; private set; } = false;

    public float attackRange = 1.2f;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        ChooseRandomTarget();
    }

    private void Update()
    {
        if (IsDead || targetEnemy == null) {
            ChooseRandomTarget();
            return;
        }

        var enemyAI = targetEnemy.GetComponent<AIController>();
        if (enemyAI != null && enemyAI.IsDead)
        {
            agent.isStopped = true;
            animator.SetFloat(Speed, 0f);
            return;
        }

        agent.SetDestination(targetEnemy.position);
        animator.SetFloat(Speed, agent.velocity.magnitude);
        hpText.text = $"{health}";

        LookAtTarget();

        float distanceToEnemy = Vector3.Distance(transform.position, targetEnemy.position);
        if (distanceToEnemy <= attackRange)
        {
            if (!isInRange)
            {
                isInRange = true;
                if (punchRoutine == null)
                    punchRoutine = StartCoroutine(PunchLoop());
            }
        }
        else
        {
            isInRange = false;
            if (punchRoutine != null)
            {
                StopCoroutine(punchRoutine);
                punchRoutine = null;
                animator.SetBool(IsPunch, false);
            }
        }
    }

    private void ChooseRandomTarget()
    {
        var enemies = GameController.Instance.aiControllers;

        var aliveEnemies = enemies.FindAll(ai => ai != null && !ai.IsDead);

        if (aliveEnemies.Count > 0)
        {
            int randIndex = Random.Range(0, aliveEnemies.Count);
            targetEnemy = aliveEnemies[randIndex].transform;
        }
        else
        {
            targetEnemy = null;
        }
    }


    private void LookAtTarget()
    {
        if (targetEnemy == null) return;

        Vector3 direction = (targetEnemy.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private IEnumerator PunchLoop()
    {
        while (isInRange && !IsDead)
        {
            var enemyAI = targetEnemy.GetComponent<AIController>();
            if (enemyAI != null && enemyAI.IsDead)
            {
                animator.SetBool(IsPunch, false);
                punchRoutine = null;
                yield break;
            }

            float index = Random.Range(0, 4);
            animator.SetFloat(PunchIndex, index);
            animator.SetBool(IsPunch, true);
            animator.SetTrigger(IsInRange);

            yield return new WaitForSeconds(Random.Range(1.2f, 2.5f));
        }

        animator.SetBool(IsPunch, false);
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
            case "head": animator.SetFloat(HitIndex, 0); break;
            case "kidney": animator.SetFloat(HitIndex, 1); break;
            case "stomach": animator.SetFloat(HitIndex, 2); break;
            default: animator.SetFloat(HitIndex, 0); break;
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
        if (punchRoutine != null) StopCoroutine(punchRoutine);
        animator.SetBool(IsPunch, false);
        animator.SetBool(IsBeingHit, false);
        animator.SetTrigger(IsKO);
        agent.enabled = false;
        hpText.text = "";

        var coll = GetComponent<Collider>();
        var rb = GetComponent<Rigidbody>();
        if (coll != null) coll.enabled = false;
        if (rb != null) rb.isKinematic = true;
        enabled = false;
    }

    public void HitEnemy(string type)
    {
        if (targetEnemy != null)
        {
            AIController ai = targetEnemy.GetComponent<AIController>();
            if (ai != null)
            {
                ai.PlayHitReaction(type);
            }
        }
    }
}
