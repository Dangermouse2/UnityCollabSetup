using UnityEngine;

public class PlayerLauncher : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab; //We will drag the bullet prefab here
    [SerializeField] private Transform bulletPoint; //We will drag the bullet launch point here
    [SerializeField] private float bulletForce = 15f; //How much force we are applying to the bullet

    [SerializeField] private float fireDelay = 0.3f; //prevent fast fingers from creating a lot of bullets at once
    private float nextFireTime = 0f;

    private void Update()
    {
        if(GameManager.isDead) return;
        //This will only trigger when we click the mouse down AND we don't click during the delay
        if(Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireDelay; //Add the delay to the fire time
            Shoot(); //call the Shoot method
        }
    }

    private void Shoot()
    {
        //This creates (AKA Instanciates) a new bulletPrefab at the bulletpoint, same rotation as the bulletpoint
        GameObject bullet = Instantiate(bulletPrefab, bulletPoint.position, bulletPoint.rotation);
        //The below line grabs the Rigidbody2D off the bullet, then adds an Impulse force of bullet Force in the x direction
        bullet.GetComponent<Rigidbody2D>().AddForce(new Vector2(transform.localScale.x * bulletForce, 0f), ForceMode2D.Impulse);
        Destroy(bullet, 3f); //make sure bullet self destructs after 3 seconds
    }

}
