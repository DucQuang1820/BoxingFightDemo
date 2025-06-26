using UnityEngine;
using UnityEngine.InputSystem;
public class MovementController : MonoBehaviour
{
    public FixedJoystick joystick;  
    public Transform player;
    public Transform cam;
    public Vector3 offset; 
    public Rigidbody rb;
    
    private float smoothSpeed = 0.125f;
    private float moveSpeed = 2f;
    private float rotateSpeed = 5f;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        
    }

    private void Update()
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.leftStick.ReadValue();
        }
    }

    private void FixedUpdate()
    {
        var direction = new Vector3(joystick.Horizontal, 0f, joystick.Vertical);

        if (direction.magnitude >= 0.1f)
        {
            Vector3 move = direction.normalized * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion yRotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, yRotation, rotateSpeed * Time.fixedDeltaTime);
        }
    }

    private void LateUpdate()
    {
        var targetPosition = player.position + offset;

        cam.position = Vector3.SmoothDamp(cam.position, targetPosition, ref velocity, smoothSpeed);
    }
}