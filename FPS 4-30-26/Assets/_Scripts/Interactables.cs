using UnityEngine;

public class Interactables : MonoBehaviour
{
    //This function will be called from our player
    public void BaseInteract()
    {
        Interact();
    }

    protected virtual void Interact()
    {
        //Nothing in this script. Interactables will override it.
    }
}
