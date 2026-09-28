using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어를 드래그해서 앵그리버드처럼 발사하는 스크립트.
/// - 당기는 표현: SlingshotVisual의 밴드 이미지(직선)
/// - 궤적 예측: 점 스프라이트를 포물선 위에 일정 간격으로 찍어 점선처럼 표현
/// 새총은 스테이지마다 새로 스폰되므로 StageManager가 AttachSlingshot()으로 연결해준다.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerLauncher : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Camera mainCamera;

    [Header("발사 설정")]
    [SerializeField] private float maxDragDistance = 3f;
    [SerializeField] private float launchPower = 6f;
    [Tooltip("이 거리보다 적게 당기고 놓으면 발사하지 않고 조준을 취소합니다.")]
    [SerializeField] private float minPullDistance = 0.3f;

    [Header("궤적 점선 (점 이미지)")]
    [Tooltip("점 하나로 쓸 스프라이트 (작은 원 이미지)")]
    [SerializeField] private Sprite dotSprite;
    [SerializeField] private int dotCount = 15;
    [Tooltip("점과 점 사이의 실제 거리(월드 단위). 클수록 성긴 점선")]
    [SerializeField] private float dotSpacing = 0.4f;
    [SerializeField] private float dotScale = 0.2f;
    [SerializeField] private Color dotColor = Color.white;
    [SerializeField] private int dotSortingOrder = 5;
    [Tooltip("켜면 멀어질수록 점이 투명해집니다.")]
    [SerializeField] private bool fadeWithDistance = true;
    [Range(0f, 1f)]
    [SerializeField] private float minDotAlpha = 0.2f;
    [SerializeField] private float previewSimStep = 0.01f;
    [SerializeField] private float previewMaxTime = 3f;

    // 현재 스테이지의 새총. StageManager가 스폰 후 AttachSlingshot으로 넘겨줌.
    private SlingshotVisual currentSlingshot;
    private Rigidbody2D playerRb;
    private bool isDragging;

    private GameObject dotContainer;
    private SpriteRenderer[] dots;

    private void Awake()
    {
        if (player == null) player = GetComponent<PlayerController>();
        if (mainCamera == null) mainCamera = Camera.main;

        playerRb = player.GetComponent<Rigidbody2D>();

        CreateDots();
    }

    private void OnDestroy()
    {
        if (dotContainer != null)
            Destroy(dotContainer);
    }

    // =========================================================
    // Slingshot
    // =========================================================

    /// <summary>
    /// 새 스테이지가 시작될 때 StageManager가 호출해서
    /// 새로 스폰된 새총을 발사 기준으로 연결한다.
    /// </summary>
    public void AttachSlingshot(SlingshotVisual slingshot)
    {
        if (currentSlingshot != null)
            currentSlingshot.HideBands();

        currentSlingshot = slingshot;
        isDragging = false;
        HideDots();
    }

    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        if (currentSlingshot == null)
            return; // 아직 새총이 연결되지 않음 (스폰 대기 등)

        // 조준 가능한 상태(Ready/Landed)이거나 조준 중일 때만 처리
        bool canInteract =
            player.State == PlayerController.PlayerState.Aiming ||
            player.CanAim();

        if (!canInteract)
        {
            // 비행 중/사망 등: 밴드·점선 숨기고 드래그 상태 초기화
            currentSlingshot.HideBands();
            isDragging = false;
            HideDots();
            return;
        }

        HandleDragInput();
    }

    private void HandleDragInput()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 screenPos = mouse.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        Vector2 pouch = currentSlingshot.PouchAnchor.position;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            player.StartAiming();

            // StartAiming이 실제로 성공했을 때만 드래그 시작
            isDragging = player.State == PlayerController.PlayerState.Aiming;
        }
        else if (mouse.leftButton.isPressed && isDragging)
        {
            Vector2 clampedDragPos = GetClampedDragPosition(worldPos, pouch);
            currentSlingshot.UpdateBands(clampedDragPos);
            UpdateTrajectoryDots(clampedDragPos, pouch);
        }
        else if (mouse.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            Vector2 clampedDragPos = GetClampedDragPosition(worldPos, pouch);
            HideDots();

            if (Vector2.Distance(clampedDragPos, pouch) < minPullDistance)
            {
                // 너무 조금 당김 -> 발사 취소하고 대기 상태로 복귀
                player.ResetReady();
                currentSlingshot.UpdateBands(pouch);
                return;
            }

            LaunchPlayer(clampedDragPos, pouch);
        }
        else if (!isDragging)
        {
            // 대기 상태: 밴드가 주머니 위치에 걸려 있는 기본 모습
            currentSlingshot.UpdateBands(pouch);
        }
    }

    // =========================================================
    // Launch
    // =========================================================

    private Vector2 GetClampedDragPosition(Vector2 rawWorldPos, Vector2 pouch)
    {
        Vector2 offset = rawWorldPos - pouch;

        if (offset.magnitude > maxDragDistance)
            offset = offset.normalized * maxDragDistance;

        return pouch + offset;
    }

    private Vector2 CalculateLaunchVelocity(Vector2 dragPos, Vector2 pouch)
    {
        Vector2 pullVector = pouch - dragPos; // 당긴 반대 방향
        Vector2 velocity = pullVector * launchPower;

        // PlayerController.Launch에서도 clamp되지만, 궤적 예측과 맞추기 위해 동일하게 제한
        return Vector2.ClampMagnitude(velocity, player.MaxSpeed);
    }

    private void LaunchPlayer(Vector2 dragPos, Vector2 pouch)
    {
        player.Launch(CalculateLaunchVelocity(dragPos, pouch));
        currentSlingshot.HideBands();
        // 새총 참조는 유지: 미스 후 같은 플랫폼에 다시 Landed 되면 재조준 가능.
        // 다음 스테이지에서는 StageManager가 AttachSlingshot으로 교체해준다.
    }

    // =========================================================
    // Trajectory Dots (점선)
    // =========================================================

    private void CreateDots()
    {
        if (dotSprite == null)
        {
            Debug.LogWarning("PlayerLauncher: dotSprite가 지정되지 않아 궤적 점선이 표시되지 않습니다.");
            dots = new SpriteRenderer[0];
            return;
        }

        // 플레이어가 움직이거나 회전해도 영향받지 않도록 부모 없이 생성
        dotContainer = new GameObject("TrajectoryDots");
        dots = new SpriteRenderer[dotCount];

        for (int i = 0; i < dotCount; i++)
        {
            GameObject dotObj = new GameObject($"Dot_{i}");
            dotObj.transform.SetParent(dotContainer.transform, false);
            dotObj.transform.localScale = Vector3.one * dotScale;

            SpriteRenderer sr = dotObj.AddComponent<SpriteRenderer>();
            sr.sprite = dotSprite;
            sr.color = dotColor;
            sr.sortingOrder = dotSortingOrder;

            dotObj.SetActive(false);
            dots[i] = sr;
        }
    }

    /// <summary>
    /// 포물선을 촘촘히 시뮬레이션하면서, 이동 거리가 dotSpacing만큼 쌓일 때마다
    /// 점을 하나씩 찍는다. (시간 간격이 아닌 거리 간격이라 점선이 고르게 보임)
    /// </summary>
    private void UpdateTrajectoryDots(Vector2 dragPos, Vector2 pouch)
    {
        if (dots == null || dots.Length == 0) return;

        Vector2 vel = CalculateLaunchVelocity(dragPos, pouch);
        Vector2 pos = pouch;

        float gravityScale = playerRb != null ? playerRb.gravityScale : 1f;
        Vector2 gravity = Physics2D.gravity * gravityScale;

        int used = 0;
        float travelled = 0f;
        float t = 0f;

        while (used < dots.Length && t < previewMaxTime)
        {
            Vector2 prev = pos;

            vel += gravity * previewSimStep;
            pos += vel * previewSimStep;
            t += previewSimStep;

            Vector2 step = pos - prev;
            travelled += step.magnitude;

            if (travelled >= dotSpacing)
            {
                // 넘친 만큼 뒤로 물려서 간격을 정확하게 맞춤
                float overshoot = travelled - dotSpacing;
                Vector2 dotPos = step.sqrMagnitude > 0f
                    ? pos - step.normalized * overshoot
                    : pos;

                PlaceDot(used, dotPos);
                used++;
                travelled = overshoot;
            }
        }

        // 남은 점은 숨김
        for (int i = used; i < dots.Length; i++)
            dots[i].gameObject.SetActive(false);
    }

    private void PlaceDot(int index, Vector2 position)
    {
        SpriteRenderer sr = dots[index];
        sr.transform.position = position;

        Color c = dotColor;
        if (fadeWithDistance && dots.Length > 1)
        {
            float t = index / (float)(dots.Length - 1);
            c.a = dotColor.a * Mathf.Lerp(1f, minDotAlpha, t);
        }
        sr.color = c;

        sr.gameObject.SetActive(true);
    }

    private void HideDots()
    {
        if (dots == null) return;

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] != null)
                dots[i].gameObject.SetActive(false);
        }
    }
}
