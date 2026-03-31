using UnityEngine;

public class BearEnemy : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float stopDistance = 1f;
    private float walkDirection;
    private Transform player;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (transform.position.x - player.position.x > stopDistance)
        {
            walkDirection = -1f;
            if (transform.localScale.x > 0f)
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }
        else if (transform.position.x - player.position.x < -stopDistance)
        {
            walkDirection = 1f;
            if (transform.localScale.x < 0f)
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }
        else
        {
            walkDirection = 0f;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocityX = walkDirection * walkSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Ouch!");
            //colision object take damage function or line of code
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.gameObject.GetComponent<Collider2D>(), true);
            Invoke(nameof(TempCollision), 2f);
        }
    }

    private void TempCollision()
    {
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), player.gameObject.GetComponent<Collider2D>(), false);
    }
    
}
