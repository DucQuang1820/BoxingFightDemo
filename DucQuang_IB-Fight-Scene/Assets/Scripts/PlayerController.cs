using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    private static readonly int Speed = Animator.StringToHash("Speed");
    public FixedJoystick joystick; 
    public Animator animator;
    public Transform player;
    public Transform cam;
    public Vector3 offset; 
    public Rigidbody rb;
    
    private float smoothSpeed = 1f;
    private float moveSpeed = 2f;
    private float rotateSpeed = 10f;

    void Start()
    {
        
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
        var targetPosition = player.position + offset;

        cam.position = Vector3.Lerp(cam.position, targetPosition, smoothSpeed);
    }
}