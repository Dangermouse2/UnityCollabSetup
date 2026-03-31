using UnityEngine;
using System.Collections;

public class BossEntrance : MonoBehaviour
{
    public RectTransform bossImage; // Drag your UI Image here
    public float screenWidth = 2000f; // Adjust based on your resolution

    public void TriggerBossIntro()
    {
        StartCoroutine(FlyAcrossRoutine());
    }

    IEnumerator FlyAcrossRoutine()
    {
        float duration = 0.5f; // Time for each move
        float elapsed = 0;

        // 1. FLY IN (Left to Center)
        Vector2 startPos = new Vector2(-screenWidth, 0);
        Vector2 centerPos = new Vector2(0, 0);

        while (elapsed < duration)
        {
            // Using SmoothStep makes it start fast and "settle" into the center
            bossImage.anchoredPosition = Vector2.Lerp(startPos, centerPos, Mathf.SmoothStep(0, 1, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. STALL (The "Oh No" Moment)
        yield return new WaitForSeconds(3f);

        // 3. FLY OUT (Center to Right)
        elapsed = 0;
        Vector2 endPos = new Vector2(screenWidth, 0);
        while (elapsed < duration)
        {
            // No SmoothStep here makes it look like it's accelerating away
            bossImage.anchoredPosition = Vector2.Lerp(centerPos, endPos, (elapsed / duration) * (elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}