using System.Collections;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private static readonly int isBeingHit = Animator.StringToHash("isBeingHit");
    private static readonly int HitIndex = Animator.StringToHash("HitIndex");
    private static readonly int isDead = Animator.StringToHash("isDead");

    public Transform player;
    public int health = 20; 
    private Animator animator;
    
    public bool IsDead { get; private set; } = false;
    
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        LookAtPlayer();
    }

    private void LookAtPlayer()
    {
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); 
        }
    }

    public void PlayHitReaction(string type = "", int damage = 10)
    {
        if (health <= 0) return; 

        health -= damage; 
        if (health <= 0)
        {
            Die();
            return;
        }

        animator.SetBool(isBeingHit, true);
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
        animator.SetBool(isBeingHit, false);
    }

    private void Die()
    {
        animator.SetTrigger("isKO"); 
        var coll = GetComponent<Collider>();
        var rb = GetComponent<Rigidbody>();
        if (coll != null)
        {
            coll.enabled = false;
            rb.isKinematic = true; 
        }
        IsDead = true;
        enabled = false;

    }
}