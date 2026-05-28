using UnityEngine;

public class StatueSimpler : Interactables
{
    protected override void Interact()
    {
        transform.Rotate(Vector3.up, 90); //rotate 90 degrees
    }
}
