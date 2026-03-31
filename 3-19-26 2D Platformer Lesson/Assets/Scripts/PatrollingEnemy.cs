using UnityEngine;

public class PatrollingEnemy : MonoBehaviour
{
    [Header("Movement")] //headers just give a title in the inspector window
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool turnAtLedges = false; //if false, our enemy will walk off the ledge
    private bool isMoving = false;
    private int facingDirection = -1; //we will use -1 to walk left, 1 will walk right

    [Header("Wall Detection")]
    [SerializeField] private float wallCheckDistance = 1f; //how far we check from the enemy to the wall
    [SerializeField] private LayerMask groundLayer; //used to avoid the Raycast2D from hitting the enemy collider

    [Header("Ledge Detection")]
    [SerializeField] private float ledgeCheckDistance = 1f;
    [SerializeField] private float ledgeCheckXOffset = 0.3f; //this will offset so our enemy looks for the edge before standing on the edge

    private Rigidbody2D rb; //we will use rigidbody2D movement

    private void Start()
    {
        isMoving = false; //make sure this boolean is false from the start
        rb = GetComponent<Rigidbody2D>(); //This will give an error if the RigidBody2D is not connected
    }

    private void OnBecameVisible() //as soon as any camera sees an image (Sprite renderer)
    {
        isMoving = true; //we won't start moving until a camera sees us
    }

    private void FixedUpdate() //use fixed update for movement so it triggers at exactly 50FPS
    {
        if (isMoving == false) 
        { 
            rb.linearVelocity = Vector3.zero; //force the enemy to freeze in place
            return; //break out of the fixed update (meaning we will ignore everything below this line
        }
        else
        {
            CheckForWalls(); //check if there is a wall in front of the enemy

            if(turnAtLedges == true) //only if turn at ledges in true
            {
                CheckForLedges(); //check for ledge and turn.
            }

            rb.linearVelocityX = facingDirection * moveSpeed; //make the enemy move in the right direction at the right move speed
        }
    }

    private void CheckForWalls()
    {
        //This line will return whatever the Raycast hits and ignore anything that is not a ground layer
        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(facingDirection, 0f), wallCheckDistance, groundLayer);

        if(hit.collider != null) //if the collider hit something
        {
            facingDirection = -1 * facingDirection; //flip the facing direction
            transform.localScale = new Vector2(-1 * transform.localScale.x, transform.localScale.y); //only multiply localScale x by -1 to flip image
        }
    }

    private void CheckForLedges()
    {
        //The startingPosition will be so we can check before reaching the ledge
        Vector2 startingRayPosition = new Vector2(transform.position.x + (facingDirection * ledgeCheckXOffset), transform.position.y);
        RaycastHit2D hit = Physics2D.Raycast(startingRayPosition, Vector2.down, ledgeCheckDistance, groundLayer);

        if (hit.collider == null) //if the ray isn't hitting anything, meaning no ground!
        {
            facingDirection = -1 * facingDirection; //flip the facing direction
            transform.localScale = new Vector2(-1 * transform.localScale.x, transform.localScale.y); //only multiply localScale x by -1 to flip image
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, new Vector2(facingDirection*wallCheckDistance, 0f)); //create a raycast to hit the wall
        if(turnAtLedges == true)
        {
            Vector2 startingRayPosition = new Vector2(transform.position.x + (facingDirection * ledgeCheckXOffset), transform.position.y);
            Gizmos.color = Color.blue; //a different color than the straight ahead ray
            Gizmos.DrawRay(startingRayPosition, Vector2.down * ledgeCheckDistance); //draw a ray going down
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) //Make sure you have your enemies tagged Enemy for this to not throw an error
        {
            facingDirection = -1 * facingDirection; //flip the facing direction
            transform.localScale = new Vector2(-1 * transform.localScale.x, transform.localScale.y); //only multiply localScale x by -1 to flip image
        }
    }
}
