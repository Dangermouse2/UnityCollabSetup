using TMPro;
using UnityEngine;
using UnityEngine.UI; //needed for images

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoText; //this will give a null reference error if we don't put it in our inspector
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text mainMessage;

    [SerializeField] private Image healthBar; //Need using UnityEngine.UI at top for Image to work
    [SerializeField] private Image bloodOverlay;

    //Needed for slow fade with overlay
    [SerializeField] private float damageDuration = 1.5f; //how long it takes for blood to disappear
    private float fadeTime; //this will be our float timer (Used to check when damage happened and how long since it happened)
    private int previousHealth; //check if my health changed

    private void Start()
    {
        //the line of code below makes our bloodOverlay totally transparent
        bloodOverlay.color = new Color(bloodOverlay.color.r, bloodOverlay.color.g, bloodOverlay.color.b, 0);
        previousHealth = GameManager.health; //make the previous health our starting health
        mainMessage.text = GameManager.mainMessage; //make the main message our starting message from the GameManager
        Invoke(nameof(ClearMessage), 2f); //turn off message after 2 seconds
    }

    private void Update()
    {
        if (GameManager.loseGame) //if we lose
        {
            //the line of code below makes our bloodOverlay totally opaque (because last number is 1)
            bloodOverlay.color = new Color(bloodOverlay.color.r, bloodOverlay.color.g, bloodOverlay.color.b, 1);
            mainMessage.text = "You were defeated!"; //whatever you want your game over message to be
        }
        if (GameManager.winGame) 
        {
            mainMessage.text = "You were victorious!\nYou are the new king!";
        }

        if(GameManager.health < previousHealth && GameManager.loseGame == false) //if we took damage and lose game is not active
        {
            //the line of code below makes our bloodOverlay totally opaque (because last number is 1)
            bloodOverlay.color = new Color(bloodOverlay.color.r, bloodOverlay.color.g, bloodOverlay.color.b, 1);
            previousHealth = GameManager.health; //reset our health
            fadeTime = damageDuration; //start the fade timer
        }
        if(fadeTime >= 0)
        {
            fadeTime = fadeTime - Time.deltaTime; //this will subtract from the current fade time in seconds
            float alphaPercentage = fadeTime / damageDuration; //how opaque transparent our image is (negatives default to zero)
            //the line of code below makes our bloodOverlay the opacity of the alphaPercentage (remember negatives will default to zero)
            bloodOverlay.color = new Color(bloodOverlay.color.r, bloodOverlay.color.g, bloodOverlay.color.b, alphaPercentage);
        }

        healthText.text = GameManager.health + " / " + GameManager.maxHealth; //this updates the health text "currentHealth / maxHealth"
        healthBar.fillAmount = (float) GameManager.health / GameManager.maxHealth; //update how full the health bar is
        ammoText.text = "Ammo: " + ProjectileWeapon.ammo; //update our ammo text
    }

    private void ClearMessage()
    {
        mainMessage.text = ""; //blank out the main message
    }
}
