using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int PunchIndex = Animator.StringToHash("PunchIndex");
    private static readonly int IsPunch = Animator.StringToHash("IsPunch");
    private static readonly int IsInRange = Animator.StringToHash("IsInRange");
    private static readonly int IsBeingHit = Animator.StringToHash("isBeingHit");
    private static readonly int HitIndex = Animator.StringToHash("HitIndex");
    private static readonly int IsKO = Animator.StringToHash("isKO");
    

    public FixedJoystick joystick;
    public Animator animator;
    public Transform player;
    public Transform cam;
    public Vector3 offset;
    public Rigidbody rb;
    public Slider hpSlider;
    public GameObject gameOver;

    private float smoothSpeed = 1f;
    private float normalMoveSpeed = 2f;
    private float slowMoveSpeed = 0f;
    public float moveSpeed;
    private float rotateSpeed = 10f;
    public int currentHealth = 0;
    public int maxHealth = 100;

    private bool isInRange = false;
    
    private Collider lastEnemyHit;
    private Coroutine punchRoutine;
    
    public bool IsDead { get; private set; } = false;


    private void Start(){
        moveSpeed = normalMoveSpeed;
        currentHealth = maxHealth;
        hpSlider.maxValue = maxHealth;
        hpSlider.value = currentHealth;
    }
    
    private void FixedUpdate()
    {
        var direction = new Vector3(joystick.Horizontal, 0f, joystick.Vertical);

        if (direction.magnitude >= 0.01f)
        {
            Vector3 move = direction.normalized * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion yRotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, yRotation, rotateSpeed * Time.fixedDeltaTime);
        }

        animator.SetFloat(Speed, direction.magnitude);
    }

    private void LateUpdate()
    {
        cam.position = Vector3.Lerp(cam.position, player.position + offset, smoothSpeed);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (IsDead) return;

        if (other.gameObject.CompareTag("Enemy") && punchRoutine == null)
        {
            lastEnemyHit = other.collider;
            isInRange = true;
            punchRoutine = StartCoroutine(PunchLoop());
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy"))
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
        while (isInRange && lastEnemyHit != null  && !IsDead)
        {
            AIController ai = lastEnemyHit.GetComponent<AIController>();
            if (ai == null || ai.IsDead)
            {
                animator.SetBool(IsPunch, false);
                punchRoutine = null;
                yield break; 
            }

            float index = Random.Range(0, 4);
            animator.SetBool(IsPunch, true);
            animator.SetTrigger(IsInRange);
            animator.SetFloat(PunchIndex, index);
            yield return new WaitForSeconds(2.2f); 
        }
    }
    
    public void HitEnemy(string type)
    {
        if (lastEnemyHit != null)
        {
            AIController ai = lastEnemyHit.GetComponent<AIController>();
            if (ai != null)
            {
                ai.PlayHitReaction(type);
            }
        }
    }

    public void PlayHitReaction(string type = "", int damage = 10)
    {
        if (IsDead) return;

        currentHealth -= damage;
        hpSlider.value = currentHealth;
        
        if (currentHealth <= 0)
        {
            IsDead = true;
            GameController.Instance.TriggerGameOver();
            return;
        }

        animator.SetBool(IsBeingHit, true);
        moveSpeed = slowMoveSpeed; 

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
        moveSpeed = normalMoveSpeed;
    }
    
}
