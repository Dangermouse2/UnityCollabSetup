using UnityEngine;

public class VictoryGoal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // The player made it to the top!
            GameManager.Instance.LevelComplete();
        }
    }
}