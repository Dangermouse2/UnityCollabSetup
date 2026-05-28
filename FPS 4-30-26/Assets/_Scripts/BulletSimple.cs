using UnityEngine;

public class BulletSimple : MonoBehaviour
{
    [SerializeField] private int bulletDamage = 10;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.health -= bulletDamage;
        }
        if (other.CompareTag("Enemy"))
        {
            if (other.GetComponent<Turret>() != null)
            {
                other.GetComponent<Turret>().TakeDamage(1);
            }
            else if (other.GetComponent<PatrolingEnemy>() != null)
            {
                other.GetComponent<PatrolingEnemy>().TakeDamage(1);
            }
        }
    }
}
