using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Goal : MonoBehaviour
{
    private StageManager stageManager;
    private bool reached;

    // StageManager가 프리팹을 생성한 직후 호출
    public void Init(StageManager manager)
    {
        stageManager = manager;
        reached = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryReach(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryReach(collision); // 진입 후에 비행 상태가 되는 경우 대비
    }

    private void TryReach(Collider2D collision)
    {
        if (reached)
            return;

        PlayerController player = collision.GetComponentInParent<PlayerController>();

        if (player == null || !player.IsFlying())
            return;

        reached = true;

        if (stageManager == null)
            stageManager = FindFirstObjectByType<StageManager>(); // Init을 못 받았을 때의 대비책

        if (stageManager != null)
            stageManager.OnGoalReached(transform.position);
    }

    public void ResetGoal()
    {
        reached = false;
    }
}
