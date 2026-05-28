using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //Static instance variables that will be shared across the game
    public static int lives = 1;
    public static int health = 100;
    public static int maxHealth = 100;
    public static string mainMessage = "Protect your coop!\nEliminate all foxes!";
    public static bool loseGame = false; //writing equal false is redundant because it is false by default
    public static bool winGame = false;

    private int enemies; //Added for a win condition (check if all enemies are gone)


    private void Update()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy").Length; //Check how many enemies are left
        if (enemies <= 0) //if no enemies, you win!
        {
            Debug.Log("You win!");
            if (winGame == false) //this is to prevent the game from calling winGame over and over
            {
                winGame = true;
                Invoke(nameof(Reset), 2f); //reset game after 2 seconds
            }
        }

        if (health <= 0) //if health reaches zero
        {
            Debug.Log("You lose!");
            health = 0; //this was to prevent health from going or staying negative
            if (loseGame == false)
            {
                loseGame = true;
                Invoke(nameof(Reset), 4f);
            }
        }
    }

    public void Reset()
    {
        lives = 1;
        health = 100;
        maxHealth = 100;        
        loseGame = false;
        winGame = false;
        ProjectileWeapon.ammo = 12; //change ammo
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //reload the current scene
    }
}
