using UnityEngine;

public class Goal : MonoBehaviour
{
    private bool reached;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (reached)
            return;

        PlayerController player =
            collision.GetComponent<PlayerController>();

        if (player == null)
            return;

        reached = true;

        player.Arrive();

        StageManager stageManager =
            FindFirstObjectByType<StageManager>();

        if (stageManager != null)
        {
            stageManager.OnGoalReached();
        }
    }
}