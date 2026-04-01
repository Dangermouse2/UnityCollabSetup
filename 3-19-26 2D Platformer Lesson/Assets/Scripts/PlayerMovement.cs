using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpForce = 9f;
    [SerializeField] private Vector2 groundCheckBox = Vector2.one; //We will change this size to match our ground check
    [SerializeField] private float groundDistance = .5f; //We will change this as needed in the inspector
    [SerializeField] private LayerMask groundLayer; //We need this so the box cast only checks for ground collision (not player collision)
    
    private bool isJumping; //Needed to check jumping  
    private bool isGrounded; //Needed to check if we are on the ground

    private Vector2 moveDirection;

    private Rigidbody2D rb; //Note that this is a 2D game, similar to 3D but with the extra ending

    private Animator animator; //needed for animation changes


    //make this public and static
    public static Vector3 respawnPoint;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>(); //Get the component connected to player
        respawnPoint = transform.position; //Set the respawn point to wherever the scene starts
        animator = GetComponentInChildren<Animator>(); //Make sure we are getting it in the child element        
    }

    private void Update() //Update at frame rate
    {
        if (GameManager.isDead || GameManager.freezePlayer) //if we are dead
        {
            return; //leave the Update function before doing anything else
        }

        if (transform.position.y < -8f) //if we fell off the edge of the game
        {
            PlayerDeath();
        }

        float xDirection = Input.GetAxis("Horizontal"); //Make sure you spell Horizontal correctly!
        animator.SetFloat("xMove", Mathf.Abs(xDirection)); //make sure you spell xMove the same way Abs gets absolute value
        // float yDirection = Input.GetAxis("Vertical");

        //If we are going left, and our character's x direction is right
        if(xDirection < 0 && transform.localScale.x > 0)
        {
            //Make sure you only make the x direction negative
            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        }
        else if (xDirection > 0 && transform.localScale.x < 0) //if we are going right and facing left
        {
            //Make sure you only make the x direction negative
            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        }

        isGrounded = GroundCheck(); //Ground check every Update

        if (Input.GetButton("Jump")) //Similar to GetKey(KeyCode.Space) this checks if we pressed Space Bar
        {
            if (isGrounded == true) //if the ground check returns true
            {
                isJumping = true; //turn on the jump that will be handled in the fixed update
            }
        }

        moveDirection = new Vector2(xDirection * speed, rb.linearVelocity.y); //keep the same linear velocity
    }

    private void FixedUpdate() //Update at 50 FPS (Better for movement and Physics)
    {
        if (GameManager.isDead) //used to prevent adding any more physics
        {
            return; 
        }

        rb.linearVelocity = moveDirection; //this will make us stop immediately after we stop
        animator.SetFloat("yMove", rb.linearVelocityY); //set the yMove Animation elements
        
        if(isJumping == true)
        {
            isJumping = false; //prevent us from flying or jumping forever
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); //Add jump force to the player
        }
        
    }

    private bool GroundCheck()
    {
        //The line below checks if we are on the ground layer
        if (Physics2D.BoxCast(transform.position, groundCheckBox, 0f, -transform.up, groundDistance, groundLayer))
        {
            animator.SetBool("isGrounded", true);       
            return true;
        }
        else
        {
            animator.SetBool("isGrounded", false);
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position - transform.up * groundDistance, groundCheckBox); //draw ground check box
    }

    public void PlayerDeath()
    {
        if (GameManager.isDead == false) //this is to make sure we only die once
        {
            GameManager.isDead = true; //prevent a second deaths
            GameManager.lives = GameManager.lives - 1; //make lives go down by 1
            Debug.Log("Player lives: " + GameManager.lives); //chow lives remaining in console window
            animator.SetBool("death", true); //make sure that "death" is spelled the same way in your animator controller
            Invoke(nameof(Respawn), 3f);
            rb.linearVelocity = Vector2.zero; //stop player movement
            rb.AddForce(Vector2.up * 10, ForceMode2D.Impulse); //toss player in the air
            GetComponent<Collider2D>().enabled = false; //turn off collider so I can fall through the ground
        }
    }

    private void Respawn()
    {
        
        //Reset velocity so we don't spawn falling at 100mph at clip through ground!
        rb.linearVelocity = Vector2.zero;

        GetComponent<Collider2D>().enabled = true; //turn collider back on so I don't fall through things anymore
        animator.SetBool("death", false); //turn off death animation and go to idle
        transform.position = respawnPoint; //move to the respawn point
        GameManager.isDead = false; //allow our player to die again
        if (GameManager.lives <= 0) 
        {
            GameManager.lives = 3; //If I don't reset the lives, they will stay at 0 because they are static
            GameManager.score = 0;
            SceneManager.LoadScene("MainMenu"); //Make sure the names match
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss")) //if we collide with an enemy
        {
            PlayerDeath(); 
        }
    }
}
