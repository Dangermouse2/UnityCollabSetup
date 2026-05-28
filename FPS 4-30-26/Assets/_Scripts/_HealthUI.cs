using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class _HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text mainMessage;
    [SerializeField] private Image healthBar;
    [SerializeField] private Image bloodOverlay;

    [SerializeField] private float damageDuration = 2;
    private float fadeTime;
    private int previousHealth;


    private void Start()
    {
        bloodOverlay.color = new Color(bloodOverlay.color.r, bloodOverlay.color.g, bloodOverlay.color.b, 0);
        previousHealth = GameManager.health;
        mainMessage.text = GameManager.mainMessage;
        Invoke(nameof(ClearMessage), 2f);
    }

    private void Update()
    {
        if (GameManager.health < previousHealth && GameManager.loseGame == false)
        {
            bloodOverlay.color = new Color(bloodOverlay.color.r, bloodOverlay.color.g, bloodOverlay.color.b, 1);
            previousHealth = GameManager.health;
            fadeTime = damageDuration;
        }

        if (GameManager.loseGame)
        {
            bloodOverlay.color = new Color(bloodOverlay.color.r, bloodOverlay.color.g, bloodOverlay.color.b, 1);
            mainMessage.text = "Failure!";
        }

        if (GameManager.winGame)
        {
            mainMessage.text = "The Flock is Safe!";
        }

        if(fadeTime >= 0)
        {
            fadeTime -= Time.deltaTime;
            float alphaPercentage = fadeTime / damageDuration; //How opaque/transparent the image will be
            bloodOverlay.color = new Color(bloodOverlay.color.r, bloodOverlay.color.g, bloodOverlay.color.b, alphaPercentage);
        }

        healthText.text = "HEALTH: " + GameManager.health;
        healthBar.fillAmount = (float)GameManager.health / GameManager.maxHealth;

    }

    private void ClearMessage()
    {
        mainMessage.text = "";
    }
}
