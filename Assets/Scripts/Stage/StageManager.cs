using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject startPlatformPrefab;
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private GameObject goalPrefab;

    [Header("Start")]
    [SerializeField]
    private Vector2 startPlatformPosition = new Vector2(0f, -1f);
    [SerializeField] private float playerStartOffset = 0f;

    [Header("Stage")]
    [SerializeField] private float minPlatformDistance = 4f;
    [SerializeField] private float maxPlatformDistance = 6f;

    [SerializeField] private float minVerticalDistance = 1.5f;
    [SerializeField] private float maxVerticalDistance = 3.5f;

    [SerializeField] private float distanceIncreasePerStage = 0.15f;
    [SerializeField] private float maxDistanceIncrease = 3f;

    [Header("Goal")]
    [SerializeField] private float initialGoalRadius = 3.275f;
    [SerializeField] private float goalRadiusDecrease = 0.12f;
    [SerializeField] private float minimumGoalRadius = 1.2f;

    [Header("Score")]
    [SerializeField] private int scorePerGoal = 100;

    [Header("Goal Reachability")]
    [SerializeField] private float launchSimulationTimeStep = 0.05f;
    [SerializeField] private float launchSimulationMaxTime = 3f;
    [SerializeField] private float goalReachTolerance = 0.5f;

    [SerializeField] private int maxGoalGenerationAttempts = 30;

    private GameObject currentPlatform;
    private GameObject currentGoal;

    private Vector2 lastPlatformPosition;

    private int stageCount;
    private int score;

    public int StageCount => stageCount;
    public int Score => score;

    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        InitializeFirstStage();
    }

    // =========================================================
    // Initialize First Stage
    // =========================================================

    private void InitializeFirstStage()
    {
        stageCount = 0;
        score = 0;

        // =====================================================
        // 시작 플랫폼 생성
        // =====================================================

        currentPlatform =
            SpawnPlatform(startPlatformPosition);

        lastPlatformPosition =
            startPlatformPosition;

        // =====================================================
        // 시작 플랫폼을 PlayerController에 등록
        // =====================================================

        if (player != null &&
            currentPlatform != null)
        {
            Platform startPlatform =
                currentPlatform.GetComponent<Platform>();

            if (startPlatform != null)
            {
                player.SetCurrentPlatform(
                    startPlatform
                );
            }
            else
            {
                Debug.LogWarning(
                    "Start Platform에 Platform 컴포넌트가 없습니다."
                );
            }
        }

        // =====================================================
        // 플레이어를 시작 플랫폼 위에 배치
        // =====================================================

        if (player != null &&
            currentPlatform != null)
        {
            Collider2D platformCollider =
                currentPlatform.GetComponent<Collider2D>();

            Collider2D playerCollider =
                player.GetComponent<Collider2D>();

            if (platformCollider != null &&
                playerCollider != null)
            {
                float platformTop =
                    platformCollider.bounds.max.y;

                float playerHalfHeight =
                    playerCollider.bounds.extents.y;

                Vector2 playerPosition =
                    new Vector2(
                        startPlatformPosition.x,
                        platformTop +
                        playerHalfHeight +
                        playerStartOffset
                    );

                player.SetStartPosition(
                    playerPosition
                );
            }
            else
            {
                player.SetStartPosition(
                    startPlatformPosition +
                    Vector2.up * playerStartOffset
                );
            }
        }

        // =====================================================
        // 첫 Goal 생성
        // =====================================================

        SpawnNextGoal();
    }

    // =========================================================
    // Goal Reached
    // =========================================================

    public void OnGoalReached(Vector2 goalPosition)
    {
        if (player == null)
            return;

        if (!player.IsFlying())
            return;

        // =====================================================
        // Score / Stage
        // =====================================================

        stageCount++;
        score += scorePerGoal;

        // =====================================================
        // Player의 Flying 상태 종료
        // =====================================================

        player.ReachGoal();

        // =====================================================
        // 이전 플랫폼 삭제
        // =====================================================

        if (currentPlatform != null)
        {
            Destroy(currentPlatform);
            currentPlatform = null;
        }

        // =====================================================
        // 현재 Goal 삭제
        // =====================================================

        if (currentGoal != null)
        {
            Destroy(currentGoal);
            currentGoal = null;
        }

        // =====================================================
        // Goal 위치에 새 플랫폼 생성
        // =====================================================

        currentPlatform =
            SpawnPlatform(goalPosition);

        lastPlatformPosition =
            goalPosition;

        // =====================================================
        // 새 플랫폼을 Current Platform으로 등록
        // =====================================================

        if (player != null &&
            currentPlatform != null)
        {
            Platform newPlatform =
                currentPlatform.GetComponent<Platform>();

            if (newPlatform != null)
            {
                player.SetCurrentPlatform(
                    newPlatform
                );
            }
            else
            {
                Debug.LogWarning(
                    "New Platform에 Platform 컴포넌트가 없습니다."
                );
            }
        }

        // =====================================================
        // 플레이어를 새 플랫폼으로 이동
        // =====================================================

        MovePlayerToPlatform(goalPosition);

        // =====================================================
        // 다음 Goal 생성
        // =====================================================

        SpawnNextGoal();
    }

    // =========================================================
    // Spawn Next Goal
    // =========================================================

    private void SpawnNextGoal()
    {
        Vector2 nextGoalPosition =
            GetNextGoalPosition();

        SpawnGoal(nextGoalPosition);
    }

    // =========================================================
    // Get Next Goal Position
    // =========================================================

    private Vector2 GetNextGoalPosition()
    {
        float distanceIncrease =
            Mathf.Min(
                stageCount * distanceIncreasePerStage,
                maxDistanceIncrease
            );

        for (int attempt = 0;
             attempt < maxGoalGenerationAttempts;
             attempt++)
        {
            float horizontalDistance =
                Random.Range(
                    minPlatformDistance + distanceIncrease,
                    maxPlatformDistance + distanceIncrease
                );

            float verticalDistance =
                Random.Range(
                    minVerticalDistance,
                    maxVerticalDistance
                );

            if (Random.value < 0.5f)
            {
                verticalDistance *= -1f;
            }

            Vector2 candidate =
                lastPlatformPosition +
                new Vector2(
                    horizontalDistance,
                    verticalDistance
                );

            if (CanReachGoal(candidate))
            {
                return candidate;
            }
        }

        // 안전장치
        // 아무 위치도 못 찾았으면 기본 위치 반환
        return lastPlatformPosition +
               new Vector2(
                   minPlatformDistance,
                   0f
               );
    }

    // =========================================================
    // Spawn Platform
    // =========================================================

    private GameObject SpawnPlatform(
        Vector2 position)
    {
        GameObject prefab =
            platformPrefab;

        if (prefab == null)
        {
            prefab =
                startPlatformPrefab;
        }

        if (prefab == null)
        {
            Debug.LogError(
                "StageManager: Platform Prefab이 없습니다."
            );

            return null;
        }

        GameObject platform =
            Instantiate(
                prefab,
                position,
                Quaternion.identity
            );

        return platform;
    }

    // =========================================================
    // Spawn Goal
    // =========================================================

    private void SpawnGoal(
        Vector2 position)
    {
        if (goalPrefab == null)
        {
            Debug.LogError(
                "StageManager: Goal Prefab이 없습니다."
            );

            return;
        }

        currentGoal =
            Instantiate(
                goalPrefab,
                position,
                Quaternion.identity
            );

        // =====================================================
        // Goal 크기
        // =====================================================

        float goalRadius =
            Mathf.Max(
                minimumGoalRadius,
                initialGoalRadius -
                stageCount * goalRadiusDecrease
            );

        currentGoal.transform.localScale =
            Vector3.one * goalRadius;

        // =====================================================
        // Goal 초기화
        // =====================================================

        Goal goal =
            currentGoal.GetComponent<Goal>();

        if (goal != null)
        {
            goal.ResetGoal();
        }
    }

    // =========================================================
    // Move Player To Platform
    // =========================================================

    private void MovePlayerToPlatform(
        Vector2 position)
    {
        if (player == null)
            return;

        Collider2D platformCollider =
            currentPlatform != null
                ? currentPlatform.GetComponent<Collider2D>()
                : null;

        Collider2D playerCollider =
            player.GetComponent<Collider2D>();

        if (platformCollider != null &&
            playerCollider != null)
        {
            float platformTop =
                platformCollider.bounds.max.y;

            float playerHalfHeight =
                playerCollider.bounds.extents.y;

            Vector2 playerPosition =
                new Vector2(
                    position.x,
                    platformTop +
                    playerHalfHeight +
                    playerStartOffset
                );

            player.MoveToPlatform(
                playerPosition
            );
        }
        else
        {
            player.MoveToPlatform(
                position +
                Vector2.up * playerStartOffset
            );
        }
    }

    // =========================================================
    // Score
    // =========================================================

    public int GetScore()
    {
        return score;
    }

    private bool CanReachGoal(Vector2 goalPosition)
    {
        if (player == null)
            return true;

        Rigidbody2D playerRb =
            player.GetComponent<Rigidbody2D>();

        if (playerRb == null)
            return true;

        Vector2 startPosition =
            lastPlatformPosition;

        Vector2 direction =
            goalPosition - startPosition;

        float distance =
            direction.magnitude;

        if (distance <= 0.01f)
            return false;

        float gravity =
            Physics2D.gravity.y *
            playerRb.gravityScale;

        float maxSpeed = 15f;

        // 여러 발사 각도를 테스트
        for (float angle = 15f;
             angle <= 75f;
             angle += 5f)
        {
            float radians =
                angle * Mathf.Deg2Rad;

            Vector2 velocity =
                new Vector2(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians)
                ) * maxSpeed;

            // 목표가 플레이어보다 아래에 있다면
            // 아래쪽 각도도 검사
            if (goalPosition.y < startPosition.y)
            {
                velocity.y =
                    -Mathf.Abs(velocity.y);
            }

            if (CanReachWithVelocity(
                startPosition,
                goalPosition,
                velocity,
                gravity))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanReachWithVelocity(
    Vector2 startPosition,
    Vector2 goalPosition,
    Vector2 velocity,
    float gravity)
    {
        Vector2 position =
            startPosition;

        float time = 0f;

        while (time <= launchSimulationMaxTime)
        {
            position +=
                velocity *
                launchSimulationTimeStep;

            velocity.y +=
                gravity *
                launchSimulationTimeStep;

            float distance =
                Vector2.Distance(
                    position,
                    goalPosition
                );

            if (distance <= goalReachTolerance)
            {
                return true;
            }

            time +=
                launchSimulationTimeStep;
        }

        return false;
    }
}