using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab; //our explosion particle
    [SerializeField] private int explosionDamage = 20; //How much the explosion hurts our player
    [SerializeField] private float blastRadius = 5f; //How close do you need to be for explosion to hurt
    [SerializeField] private float lifeSpan = 3f; //needed so the particle system doesn't just disappear

    //Physics force
    [SerializeField] private float explosionForce = 700f; // How "strong" the push is
    [SerializeField] private float upwardsForce = 2f; // Makes objects fly UP, not just out

    private Rigidbody rb; //used for trajectory

    private void Start()
    {
        rb = GetComponent<Rigidbody>(); //Get the rigidbody off of the bullet
    }

    private void Update()
    {
        if(rb.linearVelocity.sqrMagnitude > 0.1f) //If the square root is not zero or negative
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity); //make bullet face direction it is heading
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (explosionPrefab != null) //this is protection if you forgot the prefab
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, transform.rotation);    
            Destroy(explosion, lifeSpan); //destroy the explosion object after the set time
        }

        // The "Blast Zone" logic
        // This creates an invisible sphere and returns everything inside it
        Collider[] objectsInBlast = Physics.OverlapSphere(transform.position, blastRadius); //create an array of objects

        foreach (Collider hit in objectsInBlast) //foreach goes through every object in the blast
        {
            // Check if we hit the Player
            if (hit.CompareTag("Player"))
            {                
                GameManager.health -= explosionDamage; //change player's health
                Debug.Log("Health: " + GameManager.health);
            }

            // Check if we hit a Turret
            if (hit.CompareTag("Enemy")) // Make sure your Turret is tagged "Enemy"
            {
                Turret turret = hit.GetComponent<Turret>();
                if (turret != null)
                {
                    turret.TakeDamage(1);
                }
            }

            // Handle Physics Push
            Rigidbody rb = hit.GetComponent<Rigidbody>(); //get the rigidbody
            if (rb != null) //if there was a rigidbody
            {
                // The Magic Function:
                // (Force Amount, Blast Center, Blast Radius, Upwards Lift)
                rb.AddExplosionForce(explosionForce, transform.position, blastRadius, upwardsForce); 
            }
        }

        Destroy(gameObject); //destroy bullet immediately, but explosion will stick around
    }
}