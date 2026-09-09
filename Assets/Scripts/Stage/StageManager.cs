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
        if (stageParent == null)
        {
            GameObject parentObject =
                new GameObject("GeneratedStages");

            stageParent = parentObject.transform;
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

        // 플랫폼의 윗면 계산
        float platformTop =
            startPlatformPosition.y +
            (platformSize.y * 0.5f);

        // 플레이어 Collider 가져오기
        Collider2D playerCollider =
            player.GetComponent<Collider2D>();

        float playerHalfHeight = 0.5f;

        if (playerCollider != null)
        {
            playerHalfHeight =
                playerCollider.bounds.extents.y;
        }

        // 플레이어가 플랫폼 위에 딱 올라오도록 배치
        Vector2 playerStartPosition =
            new Vector2(
                startPlatformPosition.x,
                platformTop +
                playerHalfHeight +
                playerStartOffsetY
            );

        Debug.Log(
            "Start Platform Position : " +
            startPlatformPosition
        );

        Debug.Log(
            "Platform Top : " +
            platformTop
        );

        Debug.Log(
            "Player Half Height : " +
            playerHalfHeight
        );

        Debug.Log(
            "Player Start Position : " +
            playerStartPosition
        );

        // Rigidbody2D 위치까지 설정
        player.SetStartPosition(
            playerStartPosition
        );

        lastPlatformPosition =
            startPlatformPosition;

        // 첫 스테이지 생성
        for (int i = 0;
             i < initialStageCount;
             i++)
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