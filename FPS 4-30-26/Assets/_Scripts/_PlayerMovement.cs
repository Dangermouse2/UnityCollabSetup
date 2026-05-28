using UnityEngine;
using UnityEngine.InputSystem;

public class _PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private float sphereSize = 1f;
    [SerializeField] private LayerMask groundMask;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isJumping;
    private PlayerInput playerInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = new PlayerInput();
    }

    private void Update()
    {
        GroundCheck();
    }

    private void FixedUpdate()
    {
        if (isJumping)
        {
            isJumping = false;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        MovePlayer();
    }

    private void OnJump()
    {
        if (isGrounded)
        {
            isJumping = true;
        }
    }

    private void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void MovePlayer()
    {
        Vector3 direction = transform.right * moveInput.x + transform.forward * moveInput.y;
        direction.Normalize();
        rb.linearVelocity = new Vector3(direction.x*moveSpeed, rb.linearVelocity.y, direction.z*moveSpeed);
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.position - Vector3.up * groundDistance, sphereSize, groundMask);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position - Vector3.up * groundDistance, sphereSize);
    }
}
