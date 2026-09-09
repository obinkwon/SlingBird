using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Platform platformPrefab;
    [SerializeField] private Goal goalPrefab;

    [Header("Stage Generation")]
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 8f;

    [SerializeField] private float minHeight = -2f;
    [SerializeField] private float maxHeight = 2f;
    [SerializeField] private int initialStageCount = 4;

    [Header("Platform")]
    [SerializeField] private Vector2 platformSize = new Vector2(3f, 0.5f);

    [Header("Start Platform")]
    [SerializeField] private Platform startPlatformPrefab;
    [SerializeField] private Vector2 startPlatformPosition = new Vector2(0f, -2f);
    [SerializeField] private float playerStartOffsetY = 1f;

    [Header("Goal")]
    [SerializeField] private float goalHeight = 1.5f;

    [Header("Score")]
    [SerializeField] private int scorePerGoal = 100;

    [Header("Death")]
    [SerializeField] private float deathY = -8f;

    [Header("Platform Types")]
    [SerializeField] private Platform movingPlatformPrefab;

    [SerializeField, Range(0f, 1f)]
    private float movingPlatformChance = 0.3f;

    [SerializeField] private BreakPlatform breakPlatformPrefab;

    [SerializeField, Range(0f, 1f)]
    private float breakPlatformChance = 0.2f;

    private int score;
    private int stageCount;

    private Transform stageParent;

    // 마지막으로 생성된 플랫폼 위치
    private Vector2 lastPlatformPosition;

    private void Start()
    {
        score = 0;
        stageCount = 0;

        CreateStageParent();

        InitializeFirstStage();
    }

    private void CreateStageParent()
    {
        GameObject parentObject =
            new GameObject("GeneratedStages");

        stageParent = parentObject.transform;
    }

    private void InitializeFirstStage()
    {
        // 시작 플랫폼 생성
        Platform startPlatform = null;

        if (startPlatformPrefab != null)
        {
            startPlatform = Instantiate(
                startPlatformPrefab,
                startPlatformPosition,
                Quaternion.identity,
                stageParent
            );

            startPlatform.transform.localScale =
                new Vector3(
                    platformSize.x,
                    platformSize.y,
                    1f
                );
        }
        else
        {
            Debug.LogError(
                "Start Platform Prefab이 연결되지 않았습니다."
            );
        }

        // 플레이어 위치 설정
        if (player != null)
        {
            Collider2D playerCollider =
                player.GetComponent<Collider2D>();

            float platformTop =
                startPlatformPosition.y +
                platformSize.y * 0.5f;

            float playerHalfHeight = 0.5f;

            if (playerCollider != null)
            {
                playerHalfHeight =
                    playerCollider.bounds.extents.y;
            }

            Vector2 playerStartPosition =
                new Vector2(
                    startPlatformPosition.x,
                    platformTop +
                    playerHalfHeight +
                    0.05f
                );

            player.SetStartPosition(
                playerStartPosition
            );
        }
        else
        {
            Debug.LogError(
                "StageManager의 Player 참조가 없습니다."
            );
        }

        // 다음 플랫폼 생성 기준점
        lastPlatformPosition =
            startPlatformPosition;

        // 이후 플랫폼 생성
        for (int i = 0; i < initialStageCount; i++)
        {
            SpawnNextStage();
        }
    }

    public void OnGoalReached()
    {
        stageCount++;

        score += scorePerGoal;

        Debug.Log("Stage : " + stageCount);
        Debug.Log("Score : " + score);

        SpawnNextStage();

        player.ResetReady();
    }

    private void SpawnNextStage()
    {
        if (platformPrefab == null)
        {
            Debug.LogWarning(
                "Platform Prefab이 연결되지 않았습니다."
            );

            return;
        }

        if (goalPrefab == null)
        {
            Debug.LogWarning(
                "Goal Prefab이 연결되지 않았습니다."
            );

            return;
        }

        Vector2 nextPosition =
            GetNextPlatformPosition();

        SpawnPlatform(nextPosition);

        SpawnGoal(nextPosition);

        // 다음 생성의 기준점
        lastPlatformPosition =
            nextPosition;
    }

    private Vector2 GetNextPlatformPosition()
    {
        float distance =
            Random.Range(
                minDistance,
                maxDistance
            );

        float height =
            Random.Range(
                minHeight,
                maxHeight
            );

        return lastPlatformPosition +
               new Vector2(
                   distance,
                   height
               );
    }

    private void SpawnPlatform(Vector2 position)
    {
        Platform prefab =
            platformPrefab;

        float randomValue =
            Random.value;

        if (breakPlatformPrefab != null &&
            randomValue < breakPlatformChance)
        {
            prefab =
                breakPlatformPrefab;
        }
        else if (movingPlatformPrefab != null &&
                 randomValue <
                 breakPlatformChance +
                 movingPlatformChance)
        {
            prefab =
                movingPlatformPrefab;
        }

        Platform platform =
            Instantiate(
                prefab,
                position,
                Quaternion.identity,
                stageParent
            );

        platform.transform.localScale =
            new Vector3(
                platformSize.x,
                platformSize.y,
                1f
            );
    }

    private void SpawnGoal(Vector2 platformPosition)
    {
        Vector2 goalPosition =
            platformPosition +
            Vector2.up * goalHeight;

        Instantiate(
            goalPrefab,
            goalPosition,
            Quaternion.identity,
            stageParent
        );
    }

    public int GetScore()
    {
        return score;
    }

    public int GetStageCount()
    {
        return stageCount;
    }

    private void Update()
    {
        if (player == null)
            return;

        if (player.transform.position.y < deathY)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        if (player.State ==
            PlayerController.PlayerState.Dead)
            return;

        player.Die();

        Debug.Log("GAME OVER");
        Debug.Log("Final Score : " + score);
    }
}