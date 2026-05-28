using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 50f;
    private Transform cam; //This is our camera, Don't name it camera because that variable already exists

    private float xRotation; //this will rotate our player
    private Vector2 lookInput; //this will connect to the new input system's look input

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //lock our cursor, mainly to prevent clicking on other things while the game is playing
        cam = GetComponentInChildren<Camera>().transform;
    }

    private void Update()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime; //Time.deltaTime makes this variable update in seconds instead of frame rate
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY; //the up and down camera rotation corresponds to the up and down Y input
        xRotation = Mathf.Clamp(xRotation, -90, 90); //prevent Y look from going upside down too far up or down

        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f); //only rotate camera around the x-axis

        transform.Rotate(Vector3.up * mouseX); //player rotates around vertical axis when mouse goes left or right
    }

    private void OnLook(InputValue inputValue)
    {
        lookInput = inputValue.Get<Vector2>(); //make look input equal the Vector2 input from the new input system
    }
}
