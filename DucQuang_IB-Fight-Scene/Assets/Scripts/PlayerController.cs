using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int PunchIndex = Animator.StringToHash("PunchIndex");
    private static readonly int IsPunch = Animator.StringToHash("IsPunch");
    private static readonly int IsInRange = Animator.StringToHash("IsInRange");

    public FixedJoystick joystick;
    public Animator animator;
    public Transform player;
    public Transform cam;
    public Vector3 offset;
    public Rigidbody rb;

    private float smoothSpeed = 1f;
    private float moveSpeed = 2f;
    private float rotateSpeed = 10f;

    private bool isInRange = false;
    private Coroutine punchRoutine;

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
        if (other.gameObject.CompareTag("Enemy") && punchRoutine == null)
        {
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
            }
        }
    }

    private IEnumerator PunchLoop()
    {
        while (isInRange)
        {
            float index = Random.Range(0, 4);
            animator.SetBool(IsPunch, true);
            animator.SetTrigger(IsInRange);
            animator.SetFloat(PunchIndex, index);
            
            yield return new WaitForSeconds(2.2f); 
        }
    }
    
    public void HitEnemy()
    {
        // if (lastEnemyHit != null)
        // {
        //     AIController ai = lastEnemyHit.GetComponent<AIController>();
        //     if (ai != null)
        //     {
        //         ai.PlayHitReaction();
        //     }
        // }
    }
}
