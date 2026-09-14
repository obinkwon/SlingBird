using UnityEngine;

public class Goal : MonoBehaviour
{
    private bool reached;

    private void Awake()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 1;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (reached)
            return;

        PlayerController player =
            collision.GetComponent<PlayerController>();

        if (player == null)
            return;

        if (!player.IsFlying())
            return;

        reached = true;

        StageManager stageManager =
            FindFirstObjectByType<StageManager>();

        if (stageManager != null)
        {
            stageManager.OnGoalReached(
                transform.position
            );
        }
    }

    public void ResetGoal()
    {
        reached = false;
    }
}