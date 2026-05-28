using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Instance variables
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    //Ground check variables
    [SerializeField] private float groundDistance = 0.5f;
    [SerializeField] private float sphereSize = 0.5f;
    [SerializeField] private LayerMask groundLayer; //make sure we only check for ground, not the player

    private Rigidbody rb; //for movement
    private Vector2 moveInput; //this will be the new input system for movement
    private bool isGrounded;
    private bool isJumping;

    private void Start()
    {
        rb = GetComponent<Rigidbody>(); //we need to get the rigidbody connected to the player to move the player
    }

    private void Update()
    {     
        //The CheckSphere checks if a sphere at the bottom of our player is colliding with the ground
        isGrounded = Physics.CheckSphere(transform.position - Vector3.up * groundDistance, sphereSize, groundLayer);
    }

    private void FixedUpdate()
    {
        Vector3 direction = transform.forward * moveInput.y + transform.right * moveInput.x; //create a vector based on our heading
        direction.Normalize(); //prevent fast diagonal movement
        rb.linearVelocity = new Vector3(direction.x * moveSpeed, rb.linearVelocity.y, direction.z * moveSpeed);

        if (isJumping == true)
        {
            isJumping = false; //prevent us from flying away
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); //add a jump force to our player
        }
    }

    private void OnMovement(InputValue inputValue) //This is also an event from the new input system
    {
        moveInput = inputValue.Get<Vector2>(); //get whatever our input was and save it as the moveInput variable
    }

    private void OnJump() //this is an event that will trigger when the Jump button(s) are pressed from the new input system
    {
        if (isGrounded == true)
        {
            isJumping = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position - Vector3.up * groundDistance, sphereSize);
    }
}
