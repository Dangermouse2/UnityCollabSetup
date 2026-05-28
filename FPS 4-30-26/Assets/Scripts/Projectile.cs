using UnityEngine;

public class Projectile : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) //When the projectile hits something
    {


        Destroy(gameObject); //destroy game object
    }
}
