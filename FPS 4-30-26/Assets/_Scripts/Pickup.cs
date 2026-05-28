using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private Material highlightMaterial;
    private Material[] originalMaterials;
    private MeshRenderer[] meshRenderers;

    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private float lookRange = 10f;

    private bool isLookedAt;
    private Camera playerCam;
    private GameObject player;

    private void Start()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        originalMaterials = new Material[meshRenderers.Length];
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            originalMaterials[i] = meshRenderers[i].material;
        }

        player = GameObject.FindGameObjectWithTag("Player");
        playerCam = player.GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, lookRange))
        {
            if (hit.collider.GetComponentInParent<Pickup>() == this)
            {
                if (!isLookedAt)
                {
                    SetLookedAt(true);
                }
                return;
            }
        }

        if (isLookedAt)
        {
            SetLookedAt(false);
        }
    }

    private void SetLookedAt(bool isLookedAt)
    {
        this.isLookedAt = isLookedAt;
        if (isLookedAt)
        {
            foreach(MeshRenderer renderer in meshRenderers)
            {
                renderer.material = highlightMaterial;
            }
        }
        else
        {
            for(int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material = originalMaterials[i];
            }
        }
    }

}
