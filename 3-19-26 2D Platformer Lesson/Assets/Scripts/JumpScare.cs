using UnityEngine;
using UnityEngine.UI;

public class JumpScare : MonoBehaviour
{
    [SerializeField] private Image scareImage;

    private void Start()
    {
        scareImage.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            BossEntrance bossIntro = Object.FindAnyObjectByType<BossEntrance>();
            if (bossIntro != null)
            {
                bossIntro.TriggerBossIntro();
            }

            PlayerMovement.respawnPoint = transform.position;
         //   scareImage.enabled=true;
         //   InvokeRepeating(nameof(ScareImageToggle), 0f, 0.25f); //Invoke now, but repeat every quarter second
            Invoke(nameof(EndScare), 2f);
        }
    }

    private void ScareImageToggle()
    {
        scareImage.enabled = !scareImage.enabled; //toggleImage
    }

    private void EndScare()
    {
        CancelInvoke(nameof(ScareImageToggle));
        scareImage.enabled = false;
        InigoMontoya boss = Object.FindFirstObjectByType<InigoMontoya>();
        
        // 2. Make sure we actually found him (prevents crash errors)
        if (boss != null)
        {
            // 3. Press his public start button!
            boss.StartBossFight();
        }
    }
}
