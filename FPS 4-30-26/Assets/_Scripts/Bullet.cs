using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject splatPrefab;
    [SerializeField] private LayerMask hitLayers; // Select "Default" and "Environment" in Inspector

    void OnTriggerEnter(Collider other)
    {
        // Ignore the player/gun so the bullet doesn't explode in your face
        if (other.CompareTag("Player")) return;

        // Shoot a ray BACKWARDS from the bullet's current position to find the exact entry point
        // This is more reliable than shooting forward if the bullet is already 'inside' the wall
        Ray ray = new Ray(transform.position + transform.forward * -1f, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5f, hitLayers))
        {
            Quaternion rot = Quaternion.LookRotation(hit.normal);
            Vector3 pos = hit.point + hit.normal * 0.01f;

            GameObject splat = Instantiate(splatPrefab, pos, rot);
            splat.transform.SetParent(hit.transform);
            Destroy(splat, 5f);
        }

        ray = new Ray(transform.position + transform.forward * 1f, transform.forward);
        

        if (Physics.Raycast(ray, out hit, 5f, hitLayers))
        {
            Quaternion rot = Quaternion.LookRotation(hit.normal);
            Vector3 pos = hit.point + hit.normal * 0.01f;

            GameObject splat = Instantiate(splatPrefab, pos, rot);
            splat.transform.SetParent(hit.transform);
            Destroy(splat, 5f);
        }

        Destroy(gameObject);
    }
}
