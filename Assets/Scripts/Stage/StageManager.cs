using UnityEngine;
 
public class StageManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject startPlatformPrefab;
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private GameObject goalPrefab;
 
    [Header("Start")]
    [SerializeField] private Vector2 startPlatformPosition = new Vector2(0f, -1f);
    [SerializeField] private float playerStartOffset = 0f;
 
    [Header("Stage Distance")]
    [SerializeField] private float minPlatformDistance = 4f;
    [SerializeField] private float maxPlatformDistance = 6f;
    [SerializeField] private float minVerticalDistance = 1.5f;
    [SerializeField] private float maxVerticalDistance = 3.5f;
    [SerializeField] private float distanceIncreasePerStage = 0.15f;
    [SerializeField] private float maxDistanceIncrease = 3f;
 
    [Header("Vertical Range")]
    [Tooltip("켜면 시작 높이 기준 ±verticalRange 밖으로 플랫폼이 계속 쏠리지 않도록 방향을 보정합니다.")]
    [SerializeField] private bool limitVerticalRange = true;
    [SerializeField] private float verticalRange = 6f;
 
    [Header("Goal")]
    [Tooltip("Goal 프리팹의 localScale로 적용되는 값입니다.")]
    [SerializeField] private float initialGoalRadius = 3.275f;
    [SerializeField] private float goalRadiusDecrease = 0.12f;
    [SerializeField] private float minimumGoalRadius = 1.2f;
 
    [Header("Score")]
    [SerializeField] private int scorePerGoal = 100;
 
    [Header("Goal Reachability")]
    [Tooltip("PlayerController의 maxSpeed와 같은 값으로 맞춰주세요.")]
    [SerializeField] private float launchSpeed = 15f;
    [Tooltip("최대 속도의 이 비율로만 닿아도 '도달 가능'으로 판정합니다. (여유 확보용)")]
    [Range(0.5f, 1f)]
    [SerializeField] private float reachSpeedMargin = 0.9f;
    [SerializeField] private float minLaunchAngle = 0f;
    [SerializeField] private float maxLaunchAngle = 80f;
    [SerializeField] private float maxFlightTime = 3f;
    [SerializeField] private int maxGoalGenerationAttempts = 30;
 
    private GameObject currentPlatform;
    private GameObject currentGoal;
 
    private Rigidbody2D playerRb;
    private Collider2D playerCollider;
 
    private Vector2 lastPlatformPosition;
 
    private int stageCount;
    private int score;
 
    public int StageCount => stageCount;
    public int Score => score;
 
    // GameUI 호환용
    public int GetScore() => score;
 
    // =========================================================
    // Unity Messages
    // =========================================================
 
    private void Awake()
    {
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
            playerCollider = player.GetComponent<Collider2D>();
        }
    }
 
    private void Start()
    {
        InitializeFirstStage();
    }
 
    // =========================================================
    // Stage Flow
    // =========================================================
 
    private void InitializeFirstStage()
    {
        stageCount = 0;
        score = 0;
 
        currentPlatform = SpawnPlatform(startPlatformPosition);
        lastPlatformPosition = startPlatformPosition;
 
        RegisterPlatformToPlayer(currentPlatform);
 
        if (player != null && currentPlatform != null)
        {
            player.SetStartPosition(
                GetPlayerPositionOn(currentPlatform, startPlatformPosition)
            );
        }
 
        SpawnNextGoal();
    }
 
    public void OnGoalReached(Vector2 goalPosition)
    {
        if (player == null)
            return;
 
        if (!player.IsFlying())
            return;
 
        stageCount++;
        score += scorePerGoal;
 
        // Flying 상태 종료
        player.ReachGoal();
 
        // 이전 플랫폼 / 현재 Goal 삭제
        DestroyAndClear(ref currentPlatform);
        DestroyAndClear(ref currentGoal);
 
        // Goal 위치에 새 플랫폼 생성 후 등록
        currentPlatform = SpawnPlatform(goalPosition);
        lastPlatformPosition = goalPosition;
 
        RegisterPlatformToPlayer(currentPlatform);
 
        // 플레이어를 새 플랫폼 위로 이동
        player.MoveToPlatform(
            GetPlayerPositionOn(currentPlatform, goalPosition)
        );
 
        SpawnNextGoal();
    }
 
    private void SpawnNextGoal()
    {
        SpawnGoal(GetNextGoalPosition());
    }
 
    // =========================================================
    // Goal Position
    // =========================================================
 
    private Vector2 GetNextGoalPosition()
    {
        float distanceIncrease = Mathf.Min(
            stageCount * distanceIncreasePerStage,
            maxDistanceIncrease
        );
 
        // 실제 발사가 시작될 위치 (플랫폼 윗면 + 플레이어 반높이)
        Vector2 launchOrigin = GetPlayerPositionOn(currentPlatform, lastPlatformPosition);
 
        for (int attempt = 0; attempt < maxGoalGenerationAttempts; attempt++)
        {
            float horizontal = Random.Range(
                minPlatformDistance + distanceIncrease,
                maxPlatformDistance + distanceIncrease
            );
 
            float vertical =
                Random.Range(minVerticalDistance, maxVerticalDistance) *
                PickVerticalSign();
 
            Vector2 candidate =
                lastPlatformPosition + new Vector2(horizontal, vertical);
 
            if (CanReachGoal(launchOrigin, candidate))
                return candidate;
        }
 
        // 안전장치: 가장 가까운 수평 위치
        Debug.LogWarning(
            "StageManager: 도달 가능한 Goal 위치를 찾지 못해 기본 위치를 사용합니다."
        );
 
        return lastPlatformPosition + new Vector2(minPlatformDistance, 0f);
    }
 
    // 위/아래 방향 선택 (범위를 벗어나지 않도록 보정)
    private float PickVerticalSign()
    {
        if (limitVerticalRange)
        {
            float minY = startPlatformPosition.y - verticalRange;
            float maxY = startPlatformPosition.y + verticalRange;
 
            bool canGoUp =
                lastPlatformPosition.y + minVerticalDistance <= maxY;
            bool canGoDown =
                lastPlatformPosition.y - minVerticalDistance >= minY;
 
            if (canGoUp && !canGoDown) return 1f;
            if (!canGoUp && canGoDown) return -1f;
        }
 
        return Random.value < 0.5f ? 1f : -1f;
    }
 
    // =========================================================
    // Reachability (포물선 해석해)
    // =========================================================
    //
    // y = x*t - g*x^2*(1 + t^2) / (2*v^2)   (t = tan(theta))
    // 를 t에 대한 이차방정식으로 풀어, 허용 각도/비행시간 안의
    // 해가 하나라도 있으면 도달 가능으로 판정합니다.
    // (시간 스텝 시뮬레이션처럼 Goal을 통과해 버리는 오차가 없음)
 
    private bool CanReachGoal(Vector2 origin, Vector2 goalPosition)
    {
        if (playerRb == null)
            return true;
 
        Vector2 d = goalPosition - origin;
 
        if (d.x <= 0.01f)
            return false;
 
        float g = -Physics2D.gravity.y * playerRb.gravityScale;
 
        // 중력이 없으면 직선 발사로 항상 도달 가능
        if (g <= 0.0001f)
            return true;
 
        float v = launchSpeed * reachSpeedMargin;
 
        float a = g * d.x * d.x / (2f * v * v);
        float c = d.y + a;
 
        float discriminant = d.x * d.x - 4f * a * c;
 
        if (discriminant < 0f)
            return false;
 
        float sqrt = Mathf.Sqrt(discriminant);
 
        float tan1 = (d.x + sqrt) / (2f * a);
        float tan2 = (d.x - sqrt) / (2f * a);
 
        return IsValidLaunch(tan1, d.x, v) ||
               IsValidLaunch(tan2, d.x, v);
    }
 
    private bool IsValidLaunch(float tan, float horizontalDistance, float speed)
    {
        float angle = Mathf.Atan(tan) * Mathf.Rad2Deg;
 
        if (angle < minLaunchAngle || angle > maxLaunchAngle)
            return false;
 
        float cos = Mathf.Cos(angle * Mathf.Deg2Rad);
        float flightTime = horizontalDistance / (speed * cos);
 
        return flightTime <= maxFlightTime;
    }
 
    // =========================================================
    // Spawn
    // =========================================================
 
    private GameObject SpawnPlatform(Vector2 position)
    {
        GameObject prefab = platformPrefab != null
            ? platformPrefab
            : startPlatformPrefab;
 
        if (prefab == null)
        {
            Debug.LogError("StageManager: Platform Prefab이 없습니다.");
            return null;
        }
 
        GameObject platform =
            Instantiate(prefab, position, Quaternion.identity);
 
        // Collider2D.bounds가 생성 직후 옛날 값이 되지 않도록 동기화
        Physics2D.SyncTransforms();
 
        return platform;
    }
 
    private void SpawnGoal(Vector2 position)
    {
        if (goalPrefab == null)
        {
            Debug.LogError("StageManager: Goal Prefab이 없습니다.");
            return;
        }
 
        currentGoal =
            Instantiate(goalPrefab, position, Quaternion.identity);
 
        currentGoal.transform.localScale =
            Vector3.one * GetCurrentGoalRadius();
 
        Goal goal = currentGoal.GetComponent<Goal>();
 
        if (goal != null)
            goal.ResetGoal();
    }
 
    private float GetCurrentGoalRadius()
    {
        return Mathf.Max(
            minimumGoalRadius,
            initialGoalRadius - stageCount * goalRadiusDecrease
        );
    }
 
    // =========================================================
    // Helpers
    // =========================================================
 
    private void RegisterPlatformToPlayer(GameObject platformObject)
    {
        if (player == null || platformObject == null)
            return;
 
        Platform platform = platformObject.GetComponent<Platform>();
 
        if (platform != null)
            player.SetCurrentPlatform(platform);
        else
            Debug.LogWarning("StageManager: Platform 컴포넌트가 없습니다.");
    }
 
    // 플랫폼 윗면 위에 서 있을 때의 플레이어 위치
    private Vector2 GetPlayerPositionOn(
        GameObject platform,
        Vector2 platformPosition)
    {
        Collider2D platformCollider =
            platform != null ? platform.GetComponent<Collider2D>() : null;
 
        if (platformCollider != null && playerCollider != null)
        {
            float platformTop = platformCollider.bounds.max.y;
            float playerHalfHeight = playerCollider.bounds.extents.y;
 
            return new Vector2(
                platformPosition.x,
                platformTop + playerHalfHeight + playerStartOffset
            );
        }
 
        return platformPosition + Vector2.up * playerStartOffset;
    }
 
    private static void DestroyAndClear(ref GameObject target)
    {
        if (target != null)
        {
            Destroy(target);
            target = null;
        }
    }
}
