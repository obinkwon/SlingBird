using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어를 드래그해서 앵그리버드처럼 발사하는 스크립트.
/// 조준선은 LineRenderer 대신 늘어나는 스프라이트 이미지로 표현한다.
/// (직선으로만 늘어남 - 탄성/흔들림 없음)
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerLauncher : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Camera mainCamera;

    [Header("발사 설정")]
    [SerializeField] private float maxDragDistance = 3f;   // 이 이상 당기면 더 안 늘어남
    [SerializeField] private float launchPower = 6f;       // 당긴 거리 -> 속도 변환 배율
    [SerializeField] private float maxLaunchSpeed = 15f;    // 최종 발사 속도 제한

    [Header("궤적 예측선 (선택)")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPoints = 20;
    [SerializeField] private float trajectoryTimeStep = 0.1f;

    [Header("당기기 선 이미지 (Pull Line Sprite)")]
    [SerializeField] private Transform pullLineSprite;   // SpriteRenderer가 붙은 자식 오브젝트
    [SerializeField] private Transform anchorPoint;      // 새총 고정 지점 (보통 시작 위치)
    [SerializeField] private float spriteAngleOffset = -90f; // 스프라이트가 세로로 그려진 경우 -90

    private Vector3 pullLineBaseScale;
    private Vector2 dragStartWorldPos;
    private bool isDragging;

    private void Awake()
    {
        if (player == null) player = GetComponent<PlayerController>();
        if (mainCamera == null) mainCamera = Camera.main;

        if (pullLineSprite != null)
        {
            pullLineBaseScale = pullLineSprite.localScale;
            pullLineSprite.gameObject.SetActive(false);
        }

        if (trajectoryLine != null)
            trajectoryLine.positionCount = 0;

        if (anchorPoint == null)
            anchorPoint = transform; // 앵커 별도 지정 안 하면 자기 자신 위치 사용
    }

    private void Update()
    {
        // 조준/발사는 Ready 또는 Aiming 상태에서만 처리
        if (player.State != PlayerController.PlayerState.Ready &&
            player.State != PlayerController.PlayerState.Aiming)
        {
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

        if (mouse.leftButton.wasPressedThisFrame)
        {
            isDragging = true;
            dragStartWorldPos = anchorPoint.position;
            player.SetAiming(true);

            if (pullLineSprite != null)
                pullLineSprite.gameObject.SetActive(true);
        }
        else if (mouse.leftButton.isPressed && isDragging)
        {
            Vector2 clampedDragPos = GetClampedDragPosition(worldPos);
            UpdatePullLine(clampedDragPos);
            UpdateTrajectoryPreview(clampedDragPos);
        }
        else if (mouse.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            Vector2 clampedDragPos = GetClampedDragPosition(worldPos);
            LaunchPlayer(clampedDragPos);

            HidePullLine();
            ClearTrajectoryPreview();
        }
    }

    /// <summary>
    /// 앵커 기준으로 maxDragDistance를 넘지 않도록 드래그 위치를 제한한다.
    /// </summary>
    private Vector2 GetClampedDragPosition(Vector2 rawWorldPos)
    {
        Vector2 anchor = anchorPoint.position;
        Vector2 offset = rawWorldPos - anchor;

        if (offset.magnitude > maxDragDistance)
            offset = offset.normalized * maxDragDistance;

        return anchor + offset;
    }

    /// <summary>
    /// 당기기 선 이미지를 앵커 -> 드래그 위치 방향/거리에 맞춰 갱신한다. (직선 스케일링만 수행)
    /// </summary>
    private void UpdatePullLine(Vector2 dragPos)
    {
        if (pullLineSprite == null) return;

        Vector2 anchor = anchorPoint.position;
        Vector2 dir = dragPos - anchor;
        float distance = dir.magnitude;

        // 위치는 항상 앵커에 고정 (스프라이트 pivot이 하단이라고 가정)
        pullLineSprite.position = anchor;

        // 방향에 맞춰 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + spriteAngleOffset;
        pullLineSprite.rotation = Quaternion.Euler(0f, 0f, angle);

        // 거리만큼 y축으로 늘림 (스프라이트 원본 세로 길이 = 1 unit 기준)
        pullLineSprite.localScale = new Vector3(
            pullLineBaseScale.x,
            distance,
            pullLineBaseScale.z
        );
    }

    private void HidePullLine()
    {
        if (pullLineSprite != null)
            pullLineSprite.gameObject.SetActive(false);
    }

    /// <summary>
    /// 드래그 위치 기준으로 발사 속도를 계산해서 캐릭터를 발사한다.
    /// 당긴 반대 방향으로 날아가므로 velocity는 (anchor - dragPos) 방향.
    /// </summary>
    private void LaunchPlayer(Vector2 dragPos)
    {
        Vector2 anchor = anchorPoint.position;
        Vector2 pullVector = anchor - dragPos; // 당긴 반대 방향
        Vector2 velocity = pullVector * launchPower;

        if (velocity.magnitude > maxLaunchSpeed)
            velocity = velocity.normalized * maxLaunchSpeed;

        player.Launch(velocity);
    }

    private void UpdateTrajectoryPreview(Vector2 dragPos)
    {
        if (trajectoryLine == null) return;

        Vector2 anchor = anchorPoint.position;
        Vector2 pullVector = anchor - dragPos;
        Vector2 startVelocity = pullVector * launchPower;

        if (startVelocity.magnitude > maxLaunchSpeed)
            startVelocity = startVelocity.normalized * maxLaunchSpeed;

        trajectoryLine.positionCount = trajectoryPoints;
        Vector2 simPos = anchor;
        Vector2 simVel = startVelocity;
        float gravityScale = Physics2D.gravity.y; // Rigidbody2D 기본 gravityScale=1 가정

        for (int i = 0; i < trajectoryPoints; i++)
        {
            trajectoryLine.SetPosition(i, simPos);

            simVel += new Vector2(0f, gravityScale) * trajectoryTimeStep;
            simPos += simVel * trajectoryTimeStep;
        }
    }

    private void ClearTrajectoryPreview()
    {
        if (trajectoryLine != null)
            trajectoryLine.positionCount = 0;
    }
}
