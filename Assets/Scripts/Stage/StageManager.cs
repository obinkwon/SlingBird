using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Goal goalPrefab;

    [Header("Stage Settings")]
    [SerializeField] private float minGoalDistance = 5f;
    [SerializeField] private float maxGoalDistance = 8f;

    [Header("Score")]
    [SerializeField] private int scorePerGoal = 100;

    private int score;
    private int goalCount;

    private void Start()
    {
        score = 0;
        goalCount = 0;

        SpawnNextGoal();
    }

    public void OnGoalReached()
    {
        goalCount++;

        score += scorePerGoal;

        Debug.Log("Goal Reached!");
        Debug.Log("Score : " + score);

        SpawnNextGoal();

        player.ResetReady();
    }

    private void SpawnNextGoal()
    {
        if (goalPrefab == null)
            return;

        Vector2 spawnPosition =
            GetNextGoalPosition();

        Instantiate(
            goalPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }

    private Vector2 GetNextGoalPosition()
    {
        Vector2 playerPosition =
            player.transform.position;

        float distance =
            Random.Range(
                minGoalDistance,
                maxGoalDistance
            );

        float height =
            Random.Range(
                -2f,
                3f
            );

        return playerPosition +
               new Vector2(distance, height);
    }

    public int GetScore()
    {
        return score;
    }
}