using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Patrol Path")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float flySpeed = 2f;

    private Transform currentTarget;

    private void Start()
    {
        // Start by flying toward Point B
        currentTarget = pointB;
    }

    private void Update()
    {
        // 1. Move exactly one step closer to our target
        transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, flySpeed * Time.deltaTime);

        // 2. Are we basically touching the target? (Distance is almost zero)
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.1f)
        {
            // 3. We arrived! Time to swap targets and turn around.
            if (currentTarget == pointA)
            {
                currentTarget = pointB;
                FlipSprite(-1); // Face right
            }
            else
            {
                currentTarget = pointA;
                FlipSprite(1); // Face left
            }
        }
    }

    private void FlipSprite(int direction)
    {
        // We use Mathf.Abs to grab the raw scale number (ignoring any existing minus signs), 
        // then multiply it by our new direction (1 or -1) to guarantee they face the right way!
        Vector2 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * direction;
        transform.localScale = newScale;
    }
}