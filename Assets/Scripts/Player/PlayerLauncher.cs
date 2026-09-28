using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어를 드래그해서 앵그리버드처럼 발사하는 스크립트.
/// Y자 새총 스틱(고정 이미지) + 양쪽 프롱(가지 끝)에서 뻗어나오는
/// 밴드(선 이미지) 2개로 새총처럼 당기는 모습을 표현한다.
/// (밴드는 직선으로만 늘어남 - 탄성/흔들림 없음)
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

    [Header("새총 스틱 (Y자 이미지, 고정 오브젝트)")]
    [SerializeField] private Transform slingshotStick;     // 화면에 항상 보이는 Y자 스틱 스프라이트
    [SerializeField] private Transform pouchAnchor;        // 새총 가운데(주머니) 기준점 - 힘 계산 및 대기 시 플레이어 위치
    [SerializeField] private Transform leftProngAnchor;    // 왼쪽 가지 끝 (밴드 시작점)
    [SerializeField] private Transform rightProngAnchor;   // 오른쪽 가지 끝 (밴드 시작점)

    [Header("새총 밴드 (선 이미지 2개, 각 프롱 -> 당기는 지점)")]
    [SerializeField] private Transform leftBandSprite;   // 세로로 긴 선 이미지 (Pivot: Bottom)
    [SerializeField] private Transform rightBandSprite;  // 세로로 긴 선 이미지 (Pivot: Bottom)
    [SerializeField] private float bandAngleOffset = -90f; // 스프라이트가 세로로 그려진 경우 -90, 가로면 0

    private Vector3 leftBandBaseScale;
    private Vector3 rightBandBaseScale;
    private bool isDragging;

    private void Awake()
    {
        if (player == null) player = GetComponent<PlayerController>();
        if (mainCamera == null) mainCamera = Camera.main;

        if (leftBandSprite != null) leftBandBaseScale = leftBandSprite.localScale;
        if (rightBandSprite != null) rightBandBaseScale = rightBandSprite.localScale;

        if (trajectoryLine != null)
            trajectoryLine.positionCount = 0;

        if (pouchAnchor == null)
            pouchAnchor = transform; // 별도 지정 안 하면 플레이어 자기 자신 위치 사용
    }

    private void Update()
    {
        // 조준/발사는 Ready 또는 Aiming 상태에서만 처리
        if (player.State != PlayerController.PlayerState.Ready &&
            player.State != PlayerController.PlayerState.Aiming)
        {
            HideBands();
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
            player.SetAiming(true);
        }
        else if (mouse.leftButton.isPressed && isDragging)
        {
            Vector2 clampedDragPos = GetClampedDragPosition(worldPos);
            UpdateBands(clampedDragPos);
            UpdateTrajectoryPreview(clampedDragPos);
        }
        else if (mouse.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            Vector2 clampedDragPos = GetClampedDragPosition(worldPos);
            LaunchPlayer(clampedDragPos);
            ClearTrajectoryPreview();
        }
        else if (!isDragging)
        {
            // 대기 상태: 밴드가 새총 가운데(주머니)에 걸려 있는 기본 모습
            UpdateBands(pouchAnchor.position);
        }
    }

    /// <summary>
    /// 주머니(pouchAnchor) 기준으로 maxDragDistance를 넘지 않도록 드래그 위치를 제한한다.
    /// </summary>
    private Vector2 GetClampedDragPosition(Vector2 rawWorldPos)
    {
        Vector2 anchor = pouchAnchor.position;
        Vector2 offset = rawWorldPos - anchor;

        if (offset.magnitude > maxDragDistance)
            offset = offset.normalized * maxDragDistance;

        return anchor + offset;
    }

    /// <summary>
    /// 좌우 밴드를 각각의 프롱(가지 끝) -> targetPos 로 직선으로 늘려서 갱신한다.
    /// </summary>
    private void UpdateBands(Vector2 targetPos)
    {
        UpdateSingleBand(leftBandSprite, leftBandBaseScale, leftProngAnchor, targetPos);
        UpdateSingleBand(rightBandSprite, rightBandBaseScale, rightProngAnchor, targetPos);
    }

    private void UpdateSingleBand(Transform band, Vector3 baseScale, Transform prong, Vector2 targetPos)
    {
        if (band == null || prong == null) return;

        band.gameObject.SetActive(true);

        Vector2 origin = prong.position;
        Vector2 dir = targetPos - origin;
        float distance = dir.magnitude;

        // 위치는 프롱 끝에 고정 (스프라이트 pivot이 하단이라고 가정)
        band.position = origin;

        // 방향에 맞춰 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + bandAngleOffset;
        band.rotation = Quaternion.Euler(0f, 0f, angle);

        // 거리만큼 y축으로 늘림 (스프라이트 원본 세로 길이 = 1 unit 기준)
        band.localScale = new Vector3(baseScale.x, distance, baseScale.z);
    }

    private void HideBands()
    {
        if (leftBandSprite != null) leftBandSprite.gameObject.SetActive(false);
        if (rightBandSprite != null) rightBandSprite.gameObject.SetActive(false);
    }

    /// <summary>
    /// 드래그 위치 기준으로 발사 속도를 계산해서 캐릭터를 발사한다.
    /// 당긴 반대 방향으로 날아가므로 velocity는 (pouchAnchor - dragPos) 방향.
    /// </summary>
    private void LaunchPlayer(Vector2 dragPos)
    {
        Vector2 anchor = pouchAnchor.position;
        Vector2 pullVector = anchor - dragPos; // 당긴 반대 방향
        Vector2 velocity = pullVector * launchPower;

        if (velocity.magnitude > maxLaunchSpeed)
            velocity = velocity.normalized * maxLaunchSpeed;

        player.Launch(velocity);
        HideBands(); // 발사 후에는 밴드 숨김 (날아가는 동안 새총과 분리된 모습)
    }

    private void UpdateTrajectoryPreview(Vector2 dragPos)
    {
        if (trajectoryLine == null) return;

        Vector2 anchor = pouchAnchor.position;
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
