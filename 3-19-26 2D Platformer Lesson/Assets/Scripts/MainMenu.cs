using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame() //This is the method we will call from our button
    {
        SceneManager.LoadScene("level1"); //Make sure that the spelling matches the scene name perfectly!
    }
}
