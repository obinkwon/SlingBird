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
    [SerializeField] private float playerStartOffsetY = 0.05f;

    [Header("Goal")]
    [SerializeField] private float goalHeight = 1.5f;

    [SerializeField] private float startGoalRadius = 3.275f;
    [SerializeField] private float minimumGoalRadius = 0.8f;
    [SerializeField] private float goalRadiusDecrease = 0.12f;

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
        if (stageParent == null)
        {
            CreateStageParent();
        }

        if (startPlatformPrefab == null)
        {
            Debug.LogError("Start Platform Prefab이 연결되지 않았습니다.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player가 연결되지 않았습니다.");
            return;
        }

        // 시작 플랫폼 생성
        Platform startPlatform =
            Instantiate(
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

        // 플랫폼 윗면
        float platformTop =
            startPlatformPosition.y +
            platformSize.y * 0.5f;

        // 플레이어 Collider
        Collider2D playerCollider =
            player.GetComponent<Collider2D>();

        float playerHalfHeight = 0.5f;

        if (playerCollider != null)
        {
            playerHalfHeight =
                playerCollider.bounds.extents.y;
        }

        // 플레이어 시작 위치
        Vector2 playerStartPosition =
            new Vector2(
                startPlatformPosition.x,
                platformTop +
                playerHalfHeight +
                playerStartOffsetY
            );

        player.SetStartPosition(playerStartPosition);

        lastPlatformPosition =
            startPlatformPosition;

        Debug.Log(
            "Start Platform Position : " +
            startPlatformPosition
        );

        Debug.Log(
            "Player Start Position : " +
            playerStartPosition
        );

        // 초기 스테이지 생성
        for (int i = 0;
             i < initialStageCount;
             i++)
        {
            SpawnNextStage();
        }
    }

    // ==================================================
    // Goal 도착
    // ==================================================

    public void OnGoalReached(Vector2 goalPosition)
    {
        if (player == null)
            return;

        if (!player.IsFlying())
            return;

        stageCount++;

        score += scorePerGoal;

        Debug.Log("Stage : " + stageCount);
        Debug.Log("Score : " + score);

        // 현재 비행 종료
        player.ReachGoal();

        // Goal 아래의 새로운 플랫폼 생성
        Vector2 newPlatformPosition =
            new Vector2(
                goalPosition.x,
                goalPosition.y - goalHeight
            );

        SpawnPlatform(newPlatformPosition);

        // 새 플랫폼 기준으로 다음 Goal 생성
        lastPlatformPosition =
            newPlatformPosition;

        SpawnGoal(newPlatformPosition);

        // 플레이어를 새 플랫폼 위로 이동
        MovePlayerToPlatform(newPlatformPosition);
    }

    // ==================================================
    // 다음 스테이지 생성
    // ==================================================

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

        lastPlatformPosition =
            nextPosition;
    }

    // ==================================================
    // 플랫폼 위치 계산
    // ==================================================

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

    // ==================================================
    // 플랫폼 생성
    // ==================================================

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

    // ==================================================
    // Goal 생성
    // ==================================================

    private void SpawnGoal(Vector2 platformPosition)
    {
        Vector2 goalPosition =
            platformPosition +
            Vector2.up * goalHeight;

        Goal goal =
            Instantiate(
                goalPrefab,
                goalPosition,
                Quaternion.identity,
                stageParent
            );

        // 스테이지가 진행될수록 Goal 범위 감소
        float radius =
            Mathf.Max(
                minimumGoalRadius,
                startGoalRadius -
                (stageCount * goalRadiusDecrease)
            );

        // Goal의 CircleCollider2D 크기 변경
        CircleCollider2D goalCollider =
            goal.GetComponent<CircleCollider2D>();

        if (goalCollider != null)
        {
            goalCollider.radius = radius;

            Debug.Log(
                "Goal Radius : " +
                radius
            );
        }

        // Goal Transform 크기도 원형 범위에 맞춤
        goal.transform.localScale =
            Vector3.one;
    }

    // ==================================================
    // 플레이어를 새 플랫폼 위로 이동
    // ==================================================

    private void MovePlayerToPlatform(
        Vector2 platformPosition)
    {
        if (player == null)
            return;

        Collider2D playerCollider =
            player.GetComponent<Collider2D>();

        float playerHalfHeight = 0.5f;

        if (playerCollider != null)
        {
            playerHalfHeight =
                playerCollider.bounds.extents.y;
        }

        float platformTop =
            platformPosition.y +
            platformSize.y * 0.5f;

        Vector2 playerPosition =
            new Vector2(
                platformPosition.x,
                platformTop +
                playerHalfHeight +
                playerStartOffsetY
            );

        player.MoveToPlatform(playerPosition);

        Debug.Log(
            "Player moved to platform : " +
            playerPosition
        );
    }

    // ==================================================
    // Score
    // ==================================================

    public int GetScore()
    {
        return score;
    }

    public int GetStageCount()
    {
        return stageCount;
    }

    // ==================================================
    // Death
    // ==================================================

    private void Update()
    {
        if (player == null)
            return;

        if (player.State ==
            PlayerController.PlayerState.Dead)
        {
            return;
        }

        if (player.transform.position.y < deathY)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        if (player.State ==
            PlayerController.PlayerState.Dead)
        {
            return;
        }

        player.Die();

        Debug.Log("GAME OVER");
        Debug.Log("Final Score : " + score);
    }
}
