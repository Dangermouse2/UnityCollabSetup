using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 2.5f, 0f);

    private Transform playerLocation; //Needed to follow the player

    private void Start()
    {
        //Important (FindGameObject) singular, not (FindGameObjects)
        playerLocation = GameObject.FindGameObjectWithTag("Player").transform; //find player by tag
    }

    private void LateUpdate() //this occurs immediately after Update (Better for tracking)
    {
        //Important, keep the transform.position.z so we don't end up with the camera on the player        
        if (playerLocation.position.y < -3f) //if we fell way off the edge (limit camera y position to -3)
        {
            transform.position = new Vector3(playerLocation.position.x, -3f, transform.position.z) + cameraOffset;
        }
        else
        {
            transform.position = new Vector3(playerLocation.position.x, playerLocation.position.y, transform.position.z) + cameraOffset;
        }
    }

}
