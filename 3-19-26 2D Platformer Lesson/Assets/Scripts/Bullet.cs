using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) 
        {
            GameManager.score += 100;
            Destroy(collision.gameObject); //Destroy the enemy
        }
        if (collision.CompareTag("Player"))
        {
            return; //exit the function
        }
        Destroy(this.gameObject); //Destroy self after a collision
    }

    private void OnBecameInvisible() //If no Camera sees this object
    {
        Destroy(this.gameObject); //destroy self
    }
}
