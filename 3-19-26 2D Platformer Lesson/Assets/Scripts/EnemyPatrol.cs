using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement")] //This is not necessary at all, but it adds a title to the script above these variables
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool turnAtLedges = false;
    private bool isMoving;
    private float facingDirection = -1f; //used to help change direction

    [Header("Wall Detection")] //Again, not a necessary line of code, but it helps organize our variables
    [SerializeField] private float wallCheckDistance = 1f; //how far in front of the enemy we want to look
    [SerializeField] private LayerMask obstacles; //this will be all of the things that make the enemy turn direction

    [Header("Ledge Detection")]
    [SerializeField] private float ledgeCheckDistance = 1f; //how far down we are checking
    [SerializeField] private float ledgeCheckXOffset = 0.5f; //this is so our ray is not in the dead center of the enemy

    private Rigidbody2D rb;

    private void Start()
    {
        isMoving = false; //make sure the enemy does not move yet
        rb = GetComponent<Rigidbody2D>(); //get the rigidbody connected to the enemy
    }

    private void OnBecameVisible() //as soon as this object was seen by a camera
    {
        isMoving = true; //turn on the movement variable
    }

    private void FixedUpdate()
    {
        if (isMoving == false) //we are using this variable that only becomes true if the camera sees it
        {
            rb.linearVelocity = Vector2.zero; //prevent any movement
            return; //prevent the rest of this method from happening
        }
        else
        {
            CheckForWalls(); //This method turns the enemy around if it sees a wall

            if(turnAtLedges == true) //only do this if we turn on turnAtLedges
            {
                CheckForLedge();
            }

            rb.linearVelocityX = facingDirection * moveSpeed; //make enemy move in the facing direction at the intended speed
        }

    }

    private void CheckForWalls()
    {
        //The variable below sends a ray in the facing direction, checks for obstacles, and returns what it sees
        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(facingDirection, 0f), wallCheckDistance, obstacles);

        if(hit.collider != null) //if hit actually hit an obstacle
        {
            facingDirection = facingDirection * -1; //reverse the facing direction
            transform.localScale = new Vector2(-1 * transform.localScale.x, transform.localScale.y); //only flip the x local scale
        }
    }

    private void CheckForLedge()
    {
        //this line below gives us a starting point with the x-offset
        Vector2 startingPosition = new Vector2(transform.position.x + (facingDirection * ledgeCheckXOffset),  transform.position.y);
        //the line below creates a raycast down from the starting point, checking for ground obstcles
        RaycastHit2D hit = Physics2D.Raycast(startingPosition, Vector2.down, ledgeCheckDistance, obstacles);

        if (hit.collider == null) //if the raycast didn't hit anything
        {
            facingDirection = facingDirection * -1; //reverse the facing direction
            transform.localScale = new Vector2(-1 * transform.localScale.x, transform.localScale.y); //only flip the x local scale
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, new Vector2(facingDirection * wallCheckDistance, 0f));

        if (turnAtLedges == true)
        {
            Gizmos.color += Color.blue;
            Vector2 startingPosition = new Vector2(transform.position.x + (facingDirection * ledgeCheckXOffset), transform.position.y);
            Gizmos.DrawRay(startingPosition, Vector2.down*ledgeCheckDistance);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If the object we bumped into has the "Enemy" tag, turn around!
        if (collision.gameObject.CompareTag("Enemy"))
        {
            facingDirection *= -1;
            transform.localScale = new Vector2(-1 * transform.localScale.x, transform.localScale.y);
        }
    }
}