using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab; //Drag your bullet prefab here
    [SerializeField] private Transform launchPoint; //An empty transform child of the player gameobject, we will drag this in as well
    [SerializeField] private float launchForce = 15f; //how much force we are applying to the projectile

    [SerializeField] private float fireRate = 0.2f; // How fast we fire our weapon
    private float nextFireTime = 0f; //this will be used to help create delay for the bullet fire by keeping track of time between shots

    private void Update() //this happens at the framerate of the computer
    {
        if(GameManager.isDead || GameManager.freezePlayer) return;

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime) //if you are holding the fire button down (mouse left) AND the delay has been met or passed
        {
            nextFireTime = Time.time + fireRate; //set the delay between shots
            Shoot(); //call the shoot method
        }
    }

    private void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, launchPoint.position, launchPoint.rotation); //create a new projectile at the launch point
        //The line below accesses the projectile's Rigidbody2D and applies force in the correct facing x direction
        projectile.GetComponent<Rigidbody2D>().AddForce(new Vector2(transform.localScale.x * launchForce, 0f), ForceMode2D.Impulse);
        Destroy(projectile, 3f); //Destroy the projectile after 3 seconds.
    }
}
