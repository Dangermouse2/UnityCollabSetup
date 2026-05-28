using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] GameObject turretBase;
    [SerializeField] GameObject turretHead;
    [SerializeField] private float attackDistance = 20f;
    [SerializeField] ParticleSystem muzzleFlash;

    [SerializeField] private int points = 100; //How many points this enemy is worth Default 100
    [SerializeField] private float health = 5f; //enemies health


    [SerializeField] private Transform bulletStartPoint; //where enemy is firing from
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject player;  //whoever the enemy is shooting at
    [Range(0f, 1f)][SerializeField] private float hitAccuracy = 0.5f; //how close enemy is to hitting
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private float shootingDelay = 1f;

    private bool readyToShoot = true;  //these will be needed to delay shots
    private bool allowReset = true;
    private bool isDead;

    private bool attackMode = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        RaycastHit hit;
        Vector3 direction = player.transform.position - transform.position;

        if (Physics.Raycast(transform.position, direction, out hit, 100f))
        {
            if (hit.transform.tag == "Player")
            {
                if (Vector3.Distance(transform.position, player.transform.position) < attackDistance && !isDead)
                {
                    attackMode = true;
                    turretBase.transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z)); //look at the player
                    turretHead.transform.LookAt(player.transform.position);

                     if (readyToShoot)
                     {
                         Shooting();
                     }
                }
            }
        }
    }

    private void Shooting()
    {
        muzzleFlash.Play();
        readyToShoot = false; //prevent framerate rapid fire

        GameObject bullet = Instantiate(bulletPrefab, bulletStartPoint.transform.position, bulletStartPoint.transform.rotation);

        Vector3 shootingDirection = (player.transform.position - bulletStartPoint.transform.position).normalized;


        float x = UnityEngine.Random.Range(-hitAccuracy, hitAccuracy); //change spread x between intensity
        float y = UnityEngine.Random.Range(-hitAccuracy, hitAccuracy); //change spread x between intensity

        bullet.GetComponent<Rigidbody>().linearVelocity = Quaternion.AngleAxis(Random.Range(-4f, 4f) * hitAccuracy, Vector3.up) * shootingDirection * bulletSpeed;

        Destroy(bullet, 1.5f); //make sure bullet doesn't stay forever

        if (allowReset) //reset mode for autofire
        {
            Invoke("ResetShot", shootingDelay); //Invoke means call method later (at shooting delay time in seconds)
            allowReset = false;
        }
    }

    private void ResetShot() //called after shooting delay
    {
        if (health > 0) //only reset shot if enemy is alive
        {
            readyToShoot = true;
            allowReset = true;
        }
    }
}
