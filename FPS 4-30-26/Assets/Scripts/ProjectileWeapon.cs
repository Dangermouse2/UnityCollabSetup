using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectileWeapon : MonoBehaviour
{
    //Instance variables
    //Sound FX
  //  [SerializeField] private AudioSource src;
  //  [SerializeField] private AudioClip sfx1;
  //  [SerializeField] private AudioClip sfx2; //NEW: Reload sound

    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem reloadFlash; //NEW: Used for reload

    [SerializeField] private Camera cam; //remember not to call it camera because that name is taken

    [SerializeField] private float shootingDelay = 0.25f;
    private bool readyToShoot = true; //make sure this is true to start
    private bool allowReset = true;

    //for auto firing
    [SerializeField] private float spreadIntensity; //how accurate our auto fire is

    [SerializeField] private GameObject bulletPrefab; //the thing we shoot
    [SerializeField] private Transform bulletPoint; //where the bullet will spawn
    [SerializeField] private float bulletSpeed = 50f; //Make this much slower than your actual bullet so we can see it
    [SerializeField] private shootingMode currentShootingMode; //used to switch between single and auto fire

    private bool isHoldingTrigger; //Tracks if the player is holding the button

    [SerializeField] private int maxAmmo = 12;
    public static int ammo = 12; //static so we can write our UI script easier
    

    private bool isReloading; //NEW: Used to prevent multiple reload presses

    private enum shootingMode
    {
        singleFire,
        autoFire
    }

    private void Update()
    {
        // If we are in auto mode and the trigger is held, try to shoot!
        if (currentShootingMode == shootingMode.autoFire && isHoldingTrigger)
        {
            Shoot();
        }
    }

    private void Shoot()
    {       
        //if we are waiting for the shoot to reset, do nothing OR if we are out of ammo
        if (readyToShoot == false || ammo <=0) 
        {
            return;
        }

        ammo--; //decrease ammo by 1

        readyToShoot = false; //gives us a shooting delay
        transform.Translate(0, 0, -0.05f); //recoil backward

        muzzleFlash.Play(); //This will show the particle flash
       // src.clip = sfx1; //load up our sound effect
       // src.Play(); //play the sound effect

        Vector3 shootingDirection = CalculateDirectionAndSpread(); //this method will give us aim and spread for auto fire

        GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, transform.rotation); //create the bullet
        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletSpeed, ForceMode.Impulse); //add the force to the bullet

        Destroy(bullet, 1.5f); //destroy the bullet after 1.5 seconds

        if(allowReset)
        {
            Invoke(nameof(Recoil), shootingDelay / 2.0f); //recoil at half the shooting delay
            Invoke(nameof(ResetShot), shootingDelay); //reset our shot after the delay is over
            allowReset = false;
        }
    }

    private void Recoil()
    {
        transform.Translate(0, 0, 0.05f); //recoil forward
    }

    private void ResetShot() 
    {
        
        allowReset = true;
        readyToShoot = true;
    }

    private Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); //set the ray to the middle of the visible screen
        RaycastHit hit; //used to save what the ray is seeing
        Vector3 targetPoint; //location of the ray

        if(Physics.Raycast(ray, out hit)) //if the ray is hitting anything
        {
            targetPoint = hit.point; //make the target the location of the ray hit
        }
        else //if we do not hit anything
        {
            targetPoint = ray.GetPoint(1000); //give a nonsense point 
        }

        Vector3 direction = targetPoint - bulletPoint.position; //create a vector going straight from the ray hit to the point the bullet spawns

        float x = Random.Range(-spreadIntensity, spreadIntensity); //add spread in the x axis
        float y = Random.Range(-spreadIntensity, spreadIntensity); //add spread in the y axis

        return (direction + new Vector3(x, y, 0)).normalized; //return where we are shooting plus the spread
    }

    private void OnShoot(InputValue value) //Only works if player input is set to broadcast message
    {      
        isHoldingTrigger = value.isPressed; //isPressed is true when down, false when released)

        // STUDENT TIP: If this only prints when you press, and NOT when you let go, 
        // your Input Action is missing a "Release" trigger!
        Debug.Log("Trigger State: " + isHoldingTrigger);

        if (currentShootingMode == shootingMode.singleFire) //Handle Single Fire
        {
            if (isHoldingTrigger) // This only runs once when the button is first tapped
            {
                Shoot();
            }
        }
    }

    private void OnReload(InputValue value) //only works if you have a button in new input named Reload
    {
        if (isReloading == true)
        {
            return;
        }
        isReloading = true;

       // src.clip = sfx2; //load up our reload sound effect
       // src.Play(); //play the sound effect
        reloadFlash.Play();
        
        transform.Rotate(15, 0, 0); //rotate gun down
        ammo = 0;
        Invoke(nameof(Reload), 2f);
    }

    private void Reload()
    {
        isReloading = false;
        transform.Rotate(-15, 0, 0); //rotate gun back into position
        ammo = maxAmmo; //reset our current ammo to the max
    }
}
