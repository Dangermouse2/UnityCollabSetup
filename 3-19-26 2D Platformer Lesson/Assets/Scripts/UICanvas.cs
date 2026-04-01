using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UICanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gameOverScreen;

    private void Start()
    {
        gameOverScreen.text = "";
        StartCoroutine(IntroMessage());
    }

    private void Update()
    {
        livesText.text = "LIVES: " + GameManager.lives;
        scoreText.text = "SCORE: " + GameManager.score;
        if (GameManager.lives <= 0)
        {
            gameOverScreen.text = "GAME OVER";
        }
        if (GameManager.winGame == true)
        {
            StartCoroutine(WinMessage());
        }
    }

    IEnumerator IntroMessage()
    {
        GameManager.freezePlayer = true;
        gameOverScreen.text = "GET READY";
        yield return new WaitForSeconds(2f);
        gameOverScreen.text = "GO!";
        yield return new WaitForSeconds(1f);
        gameOverScreen.text = "";
        GameManager.freezePlayer = false;
        GameManager.isDead = false;
    }

    IEnumerator WinMessage()
    {
        GameManager.winGame = false;
        GameManager.freezePlayer = true;
        gameOverScreen.text = "YOU WIN!";
        yield return new WaitForSeconds(3f);
        GameManager.freezePlayer = false;
        GameManager.lives = 3; //If I don't reset the lives, they will stay at 0 because they are static
        GameManager.score = 0;
        SceneManager.LoadScene("MainMenu"); //Make sure the names match

    }
}
