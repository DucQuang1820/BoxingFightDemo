using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    public int health = 20;
    public TextMeshProUGUI hpText;
    public Animator animator;

    private NavMeshAgent agent;
    private bool isInRange = false;
    private Coroutine punchRoutine;
    public bool IsDead { get; private set; } = false;
    public float attackRange = 1f;

    public Transform currentTarget;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        ChooseRandomTarget();
    }

    private void Update()
    {
        if (IsDead) return;

        if (currentTarget == null || IsTargetDead())
        {
            ChooseRandomTarget();
            if (currentTarget == null)
            {
                // No targets left
                agent.isStopped = true;
                animator.SetFloat(Speed, 0f);
                return;
            }
        }

        agent.isStopped = false;
        agent.SetDestination(currentTarget.position);
        animator.SetFloat(Speed, agent.velocity.magnitude);
        hpText.text = $"{health}";
        LookAtTarget();

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        if (distanceToTarget <= attackRange)
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


    void ChooseRandomTarget()
    {
        List<Transform> validTargets = new List<Transform>();

        var player = FindObjectOfType<PlayerController>();
        if (player != null && !player.IsDead)
            validTargets.Add(player.transform);

        var allies = FindObjectsOfType<AllyAIController>();
        foreach (var ally in allies)
        {
            if (ally != null && !ally.IsDead)
                validTargets.Add(ally.transform);
        }

        if (validTargets.Count > 0)
        {
            int rand = Random.Range(0, validTargets.Count);
            currentTarget = validTargets[rand];
        }
    }

    bool IsTargetDead()
    {
        if (currentTarget == null) return true;

        var pc = currentTarget.GetComponent<PlayerController>();
        if (pc != null) return pc.IsDead;

        var ally = currentTarget.GetComponent<AllyAIController>();
        if (ally != null) return ally.IsDead;

        return true;
    }

    void LookAtTarget()
    {
        if (currentTarget == null) return;

        Vector3 dir = (currentTarget.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
    }

    private IEnumerator PunchLoop()
    {
        while (isInRange && !IsDead)
        {
            if (IsTargetDead())
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
        animator.SetFloat(HitIndex, type switch
        {
            "head" => 0,
            "kidney" => 1,
            "stomach" => 2,
            _ => 0
        });

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
        if (currentTarget != null)
        {
            var pc = currentTarget.GetComponent<PlayerController>();
            if (pc != null) pc.PlayHitReaction(type);

            var ally = currentTarget.GetComponent<AllyAIController>();
            if (ally != null) ally.PlayHitReaction(type);
        }
    }
}
