using UnityEngine;
using UnityEngine.UI;
using static ProjectileWeapon;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float interactDistance = 3f;
    private bool isInteractable;
    private Camera cam;
    private RaycastHit hitInfo; 

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);//create forward ray from center of camera
        Debug.DrawRay(ray.origin, ray.direction * interactDistance);

        if (Physics.Raycast(ray, out hitInfo, interactDistance))
        {
            if (hitInfo.transform.CompareTag("Interactable"))
            {
                isInteractable = true;
            }
            else
            {
                isInteractable = false;
            }
        }
        else
        {
            isInteractable = false;
        }
    }

    private void Interact()
    {
        // Check if the component exists before calling to avoid NullReferenceErrors
        if (hitInfo.transform.TryGetComponent(out Interactables interactable))
        {
            interactable.BaseInteract();
        }
    }

    private void OnInteract()
    {
        if (isInteractable)
        {
            Interact();
        }
    }
}
