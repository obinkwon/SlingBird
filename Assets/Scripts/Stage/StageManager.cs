using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerLauncher playerLauncher;
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private GameObject goalPrefab;
    [SerializeField] private GameObject slingshotPrefab; // SlingshotVisual이 붙은 Y자 새총 프리팹
    [SerializeField] private GameObject coinPrefab;      // Coin이 붙은 트리거 프리팹 (비워두면 코인 없음)

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

    [Header("Trajectory Fade (Difficulty)")]
    [Tooltip("이 스테이지(0부터 시작)부터 궤적이 투명해지기 시작합니다.")]
    [SerializeField] private int trajectoryFadeStartStage = 3;
    [Tooltip("시작 스테이지로부터 몇 스테이지 만에 최대 난이도(가장 짧은 궤적)에 도달하는지")]
    [SerializeField] private int trajectoryFadeStages = 15;
    [Tooltip("최대 난이도에서 궤적이 투명해지기 시작하는 위치 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float fadeStartAtMax = 0.1f;
    [Tooltip("최대 난이도에서 궤적이 완전히 사라지는 위치 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float fadeEndAtMax = 0.45f;

    [Header("Coins")]
    [Tooltip("스테이지마다 배치할 코인 개수 범위")]
    [SerializeField] private int minCoinsPerStage = 1;
    [SerializeField] private int maxCoinsPerStage = 3;

    [Header("Bonus Coins")]
    [Tooltip("이 스테이지 수마다 보너스 코인 지급 (0이면 끔)")]
    [SerializeField] private int bonusStageInterval = 5;
    [SerializeField] private int bonusCoins = 5;

    [Header("Trampoline")]
    [Tooltip("이 스테이지부터 발판과 목표 사이 아래쪽에 트램펄린이 나옵니다.")]
    [SerializeField] private int trampolineStartStage = 5;
    [Range(0f, 1f)]
    [SerializeField] private float trampolineChance = 1f;
    [Tooltip("발사 지점/목표 중 낮은 쪽보다 이만큼 아래에 놓는다 (정상 궤적을 막지 않도록)")]
    [SerializeField] private float trampolineDrop = 2.5f;
    [SerializeField] private float trampolineScale = 0.6f;

    [Header("Wind")]
    [Tooltip("이 스테이지부터 가로 바람 줄이 1줄 생긴다.")]
    [SerializeField] private int windStartStage = 10;
    [Tooltip("이 스테이지 수마다 바람 줄이 1줄씩 늘어난다.")]
    [SerializeField] private int windRowStageInterval = 5;
    [SerializeField] private int maxWindRows = 4;
    [Tooltip("바람 영역 안에서의 가속도 범위 (m/s²). 영역 안에 있는 동안만 받으므로 세게 잡는다")]
    [SerializeField] private float minWind = 20f;
    [SerializeField] private float maxWind = 35f;
    [SerializeField] private Sprite windSprite;
    [Tooltip("발사 지점에서 바람 줄 왼쪽 끝까지의 가로 거리")]
    [SerializeField] private float windAreaStart = 1f;
    [Tooltip("바람 줄의 가로 길이")]
    [SerializeField] private float windAreaWidth = 9f;
    [Tooltip("바람 줄 한 줄의 두께")]
    [SerializeField] private float windRowHeight = 1.2f;
    [Tooltip("발사 지점 기준 바람 줄이 놓일 수 있는 아래/위 범위")]
    [SerializeField] private float windAreaBelow = 3f;
    [SerializeField] private float windAreaAbove = 5f;
    [Tooltip("바람 줄 한 줄에 보이는 바람 줄기 개수")]
    [SerializeField] private int windStreakCount = 7;

    [Header("Score")]
    [SerializeField] private int scorePerGoal = 100;

    [Header("Game Over")]
    [Tooltip("현재 플랫폼/Goal 중 더 낮은 곳보다 이만큼 아래로 떨어지면 게임오버")]
    [SerializeField] private float fallDeathMargin = 6f;

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
    private GameObject currentSlingshot;
    private GameObject currentTrampoline;
    private readonly List<GameObject> currentCoins = new List<GameObject>();

    // CameraFollow가 목표를 화면에 함께 담을 때 사용
    public GameObject CurrentGoal => currentGoal;

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

            // 인스펙터에서 안 넣었으면 플레이어에서 찾아온다
            if (playerLauncher == null)
                playerLauncher = player.GetComponent<PlayerLauncher>();
        }
    }

    private void Start()
    {
        InitializeFirstStage();
    }

    private void Update()
    {
        CheckFallDeath();
    }

    // =========================================================
    // Game Over
    // =========================================================

    private void CheckFallDeath()
    {
        // 비행 중일 때만 검사 (조준/착지 상태는 플랫폼 위에 있음)
        if (player == null || !player.IsFlying())
            return;

        // Goal이 현재 플랫폼보다 아래에 있을 수 있으므로 둘 중 낮은 쪽 기준
        float lowestY = lastPlatformPosition.y;

        if (currentGoal != null)
            lowestY = Mathf.Min(lowestY, currentGoal.transform.position.y);

        if (player.transform.position.y < lowestY - fallDeathMargin)
            player.Die();
    }

    // =========================================================
    // Stage Flow
    // =========================================================

    private void InitializeFirstStage()
    {
        stageCount = 0;
        score = 0;

        ApplyTrajectoryDifficulty();

        currentPlatform = SpawnPlatform(startPlatformPosition);
        lastPlatformPosition = startPlatformPosition;

        RegisterPlatformToPlayer(currentPlatform);

        if (player != null && currentPlatform != null)
        {
            Vector2 playerPos =
                GetPlayerPositionOn(currentPlatform, startPlatformPosition);

            player.SetStartPosition(playerPos);
            SpawnSlingshot(playerPos);
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
        AudioManager.PlayGoal();

        // 일정 스테이지마다 보너스 코인
        if (bonusStageInterval > 0 && stageCount % bonusStageInterval == 0)
        {
            CoinWallet.Add(bonusCoins);
            AudioManager.PlayCoin();
        }

        // 새 스테이지 난이도에 맞춰 궤적 페이드 갱신
        ApplyTrajectoryDifficulty();

        // Flying 상태 종료
        player.ReachGoal();

        // 이전 플랫폼 / 현재 Goal / 이전 새총 삭제
        DestroyAndClear(ref currentPlatform);
        DestroyAndClear(ref currentGoal);
        DestroyAndClear(ref currentSlingshot);
        DestroyAndClear(ref currentTrampoline);
        WindArea.DestroyAll();
        ClearCoins();

        // Goal 위치에 새 플랫폼 생성 후 등록
        currentPlatform = SpawnPlatform(goalPosition);
        lastPlatformPosition = goalPosition;

        RegisterPlatformToPlayer(currentPlatform);

        // 플레이어를 새 플랫폼 위로 이동
        Vector2 playerPos = GetPlayerPositionOn(currentPlatform, goalPosition);
        player.MoveToPlatform(playerPos);

        // 새 플랫폼 위에 새 새총 생성 후 발사 스크립트에 연결
        SpawnSlingshot(playerPos);

        SpawnNextGoal();
    }

    private void SpawnNextGoal()
    {
        // 바람을 먼저 정해야 목표/코인 도달 검사에 반영된다
        ApplyWind();

        Vector2 goalPosition = GetNextGoalPosition();
        SpawnGoal(goalPosition);
        SpawnCoins(goalPosition);
        SpawnTrampoline(goalPosition);
    }

    // =========================================================
    // Difficulty - Trajectory Fade
    // =========================================================

    /// <summary>
    /// 0 = 궤적이 그대로 보임, 1 = 최대 난이도(궤적이 가장 짧게 사라짐)
    /// </summary>
    private float GetTrajectoryDifficulty()
    {
        if (stageCount < trajectoryFadeStartStage)
            return 0f;

        int stagesIn = stageCount - trajectoryFadeStartStage + 1;
        return Mathf.Clamp01(stagesIn / (float)Mathf.Max(1, trajectoryFadeStages));
    }

    private void ApplyTrajectoryDifficulty()
    {
        if (playerLauncher == null)
            return;

        float d = GetTrajectoryDifficulty();

        // d = 0 -> (1, 1): 페이드 없음
        // d = 1 -> (fadeStartAtMax, fadeEndAtMax): 발사 지점 근처부터 옅어져 일찍 사라짐
        float fadeStart = Mathf.Lerp(1f, fadeStartAtMax, d);
        float fadeEnd = Mathf.Lerp(1f, fadeEndAtMax, d);

        playerLauncher.SetTrajectoryFade(fadeStart, fadeEnd);
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

            // 실패할수록 최소 거리 쪽으로 당겨온다: 위쪽 목표는 멀리 못 가므로
            // 가까운 거리에서라도 위쪽 목표가 나올 수 있게 한다
            float shrink = attempt / (float)Mathf.Max(1, maxGoalGenerationAttempts - 1);
            horizontal = Mathf.Lerp(horizontal, minPlatformDistance, shrink);
            vertical = Mathf.Lerp(vertical, minVerticalDistance * Mathf.Sign(vertical), shrink);

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
    // Reachability
    // =========================================================
    //
    // 실제 발사는 주머니(pouch)가 아니라 당긴 위치에서 시작한다.
    // 위로 쏠수록 아래·뒤로 당기므로 발사 지점이 낮아져, 위쪽 목표가
    // 주머니 기준 계산보다 실제로는 닿기 어렵다. 그래서 각도마다
    // "그 각도로 쏠 때의 당긴 위치"를 출발점으로 잡고, 목표 x에서의 높이가
    // 목표 y를 위아래로 지나는(부호가 바뀌는) 각도가 있으면 도달 가능으로 본다.

    private bool CanReachGoal(Vector2 origin, Vector2 goalPosition)
    {
        if (playerRb == null)
            return true;

        // 중력이 없으면 직선 발사로 항상 도달 가능
        if (GetGravity() <= 0.0001f)
            return goalPosition.x - origin.x > 0.01f;

        return TrySolveLaunch(origin, goalPosition);
    }

    private float GetGravity() =>
        playerRb != null ? -Physics2D.gravity.y * playerRb.gravityScale : 0f;

    private bool TrySolveLaunch(Vector2 pouch, Vector2 target)
    {
        float g = GetGravity();

        if (g <= 0.0001f)
            return false;

        float v = launchSpeed * reachSpeedMargin;

        // 속도 v가 나오는 최소 당김 거리 (더 당겨도 속도는 maxSpeed로 막힌다)
        float pull = playerLauncher != null && playerLauncher.LaunchPower > 0f
            ? v / playerLauncher.LaunchPower
            : 0f;

        float prevDiff = float.NaN;

        for (float angle = minLaunchAngle; angle <= maxLaunchAngle; angle += 1f)
        {
            float rad = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);

            Vector2 launchPos = pouch - new Vector2(cos, Mathf.Sin(rad)) * pull;
            Vector2 d = target - launchPos;

            // 목표보다 뒤에서 출발하거나, 제한 시간 안에 목표 x까지 못 가면 무효
            float diff = d.x <= 0.01f
                ? float.NaN
                : HeightDiffAtTarget(launchPos, new Vector2(cos, Mathf.Sin(rad)) * v, target, g);

            if (float.IsNaN(diff))
            {
                prevDiff = float.NaN;
                continue;
            }

            if (diff == 0f || (!float.IsNaN(prevDiff) && (diff > 0f) != (prevDiff > 0f)))
                return true;

            prevDiff = diff;
        }

        return false;
    }

    // 바람 영역이 일부 구간에만 있어서 해석해 대신 짧게 시뮬레이션한다.
    // 목표 x에 도달한 순간의 높이 - 목표 높이 (제한 시간 안에 못 가면 NaN)
    private float HeightDiffAtTarget(Vector2 pos, Vector2 vel, Vector2 target, float g)
    {
        const float dt = 0.02f;   // 기본 물리 FixedUpdate 간격과 같게

        for (float t = 0f; t < maxFlightTime; t += dt)
        {
            Vector2 prev = pos;

            vel += (new Vector2(0f, -g) + WindArea.AccelAt(pos)) * dt;
            pos += vel * dt;

            if (pos.x >= target.x)
            {
                float f = (target.x - prev.x) / Mathf.Max(0.0001f, pos.x - prev.x);
                return Mathf.Lerp(prev.y, pos.y, f) - target.y;
            }
        }

        return float.NaN;
    }

    // =========================================================
    // Wind
    // =========================================================

    // 발사 지점 앞쪽에 가로로 긴 바람 줄을 만든다. 줄마다 방향/세기가 따로다.
    // 목표 위치보다 먼저 만들어야 도달 검사에 반영된다.
    private void ApplyWind()
    {
        WindArea.DestroyAll();

        if (stageCount < windStartStage)
            return;

        // 10스테이지 1줄, 15스테이지부터 2줄 ... (최대 maxWindRows)
        int rows = Mathf.Min(
            maxWindRows,
            1 + (stageCount - windStartStage) / Mathf.Max(1, windRowStageInterval));

        Vector2 origin = GetPlayerPositionOn(currentPlatform, lastPlatformPosition);

        // 세로 범위를 줄 두께 단위 칸으로 나눠, 겹치지 않게 무작위 칸을 고른다
        float bottom = origin.y - windAreaBelow;
        int slotCount = Mathf.Max(1, Mathf.FloorToInt((windAreaBelow + windAreaAbove) / windRowHeight));

        List<int> slots = new List<int>();
        for (int i = 0; i < slotCount; i++)
            slots.Add(i);

        for (int r = 0; r < rows && slots.Count > 0; r++)
        {
            int pick = Random.Range(0, slots.Count);
            int slot = slots[pick];
            slots.RemoveAt(pick);

            Rect rect = new Rect(
                origin.x + windAreaStart,
                bottom + slot * windRowHeight,
                windAreaWidth,
                windRowHeight);

            float wind = Random.Range(minWind, maxWind) * (Random.value < 0.5f ? -1f : 1f);

            GameObject row = new GameObject($"WindRow_{r}");
            row.AddComponent<WindArea>().Init(rect, wind, windSprite, player, windStreakCount);
        }
    }

    // =========================================================
    // Spawn
    // =========================================================

    private GameObject SpawnPlatform(Vector2 position)
    {
        if (platformPrefab == null)
        {
            Debug.LogError("StageManager: Platform Prefab이 없습니다.");
            return null;
        }

        GameObject platform =
            Instantiate(platformPrefab, position, Quaternion.identity);

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
        {
            // Goal.TryReach()가 매번 FindFirstObjectByType으로 StageManager를
            // 찾는 폴백에 의존하지 않도록, 정상 경로에서 직접 연결해준다.
            goal.Init(this);
            goal.ResetGoal();
        }
    }

    // 새총 프리팹을 생성하고, 주머니(pouch) 위치가 플레이어 대기 위치와
    // 일치하도록 맞춘 뒤 PlayerLauncher에 연결한다.
    private void SpawnSlingshot(Vector2 playerPosition)
    {
        if (slingshotPrefab == null || playerLauncher == null)
            return;

        currentSlingshot =
            Instantiate(slingshotPrefab, playerPosition, Quaternion.identity);

        SlingshotVisual slingshot =
            currentSlingshot.GetComponent<SlingshotVisual>();

        if (slingshot == null)
        {
            Debug.LogWarning("StageManager: 새총 프리팹에 SlingshotVisual이 없습니다.");
            return;
        }

        // 프리팹 피벗과 무관하게 pouchAnchor가 플레이어 위치에 오도록 보정
        Vector3 offset = (Vector3)playerPosition - slingshot.PouchAnchor.position;
        offset.z = 0f;
        currentSlingshot.transform.position += offset;

        playerLauncher.AttachSlingshot(slingshot);
    }

    // 목표 방향으로, 최대로 당겼을 때 닿을 수 있는 높이 범위 안에서 위아래 무작위로 코인을 놓는다.
    private void SpawnCoins(Vector2 goalPosition)
    {
        if (coinPrefab == null || currentPlatform == null)
            return;

        Vector2 origin = GetPlayerPositionOn(currentPlatform, lastPlatformPosition);

        float g = GetGravity();
        float v = launchSpeed * reachSpeedMargin;
        float dx = goalPosition.x - origin.x;

        if (g <= 0.0001f || dx <= 0.01f)
            return;

        float tanMin = Mathf.Tan(minLaunchAngle * Mathf.Deg2Rad);
        float tanMax = Mathf.Tan(maxLaunchAngle * Mathf.Deg2Rad);

        int count = Random.Range(minCoinsPerStage, maxCoinsPerStage + 1);

        for (int i = 0; i < count; i++)
        {
            // 발사 지점/목표에 붙지 않도록 가로 20%~80% 구간에 고르게
            float x = dx * (count == 1 ? 0.5f : Mathf.Lerp(0.2f, 0.8f, i / (float)(count - 1)));

            // 이 x에서 닿는 높이: 가장 낮은 각도 ~ 가장 높이 닿는 각도(v²/gx, 각도 제한 안으로)
            float yLow = HeightAt(x, tanMin, g, v);
            float yHigh = HeightAt(x, Mathf.Clamp(v * v / (g * x), tanMin, tanMax), g, v);

            // 비행 시간 제한까지 지키는 위치가 나올 때까지 몇 번 다시 뽑는다
            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector2 position = origin + new Vector2(x, Random.Range(yLow, yHigh));

                if (!TrySolveLaunch(origin, position))
                    continue;

                currentCoins.Add(Instantiate(coinPrefab, position, Quaternion.identity));
                break;
            }
        }
    }

    // 발사 기울기 tan, 속도 v로 쐈을 때 가로 x에서의 높이 (발사 지점 기준)
    private static float HeightAt(float x, float tan, float g, float v) =>
        x * tan - g * x * x * (1f + tan * tan) / (2f * v * v);

    private void ClearCoins()
    {
        foreach (GameObject coin in currentCoins)
        {
            if (coin != null)
                Destroy(coin);
        }

        currentCoins.Clear();
    }

    // 발사 지점과 목표 사이, 둘 중 낮은 쪽보다 아래에 트램펄린을 놓는다.
    // 포물선은 두 끝점을 잇는 선보다 위로 지나가므로 정상 궤적은 막지 않고, 짧게 떨어진 샷만 한 번 튕겨 올리고 사라진다.
    private void SpawnTrampoline(Vector2 goalPosition)
    {
        if (stageCount < trampolineStartStage || Random.value >= trampolineChance)
            return;

        Vector2 origin = GetPlayerPositionOn(currentPlatform, lastPlatformPosition);

        Vector2 position = new Vector2(
            Mathf.Lerp(origin.x, goalPosition.x, Random.Range(0.4f, 0.6f)),
            Mathf.Min(origin.y, goalPosition.y) - trampolineDrop);

        currentTrampoline = SpawnPlatform(position);

        if (currentTrampoline == null)
            return;

        // Platform.Awake가 스케일을 1로 맞추므로 생성 후에 줄인다
        currentTrampoline.transform.localScale = Vector3.one * trampolineScale;
        Physics2D.SyncTransforms();

        Platform platform = currentTrampoline.GetComponent<Platform>();

        if (platform != null)
            platform.MakeBouncy();
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
