using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어를 드래그해서 발사하는 스크립트.
/// 새총 스틱(Y자 이미지)은 스테이지마다 새로 스폰되므로,
/// 시각 요소는 SlingshotVisual 컴포넌트로 분리되어 있고
/// StageManager가 AttachSlingshot()으로 매 스테이지마다 새 새총을 연결해준다.
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

    [Header("궤적 예측선 (선택)")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPoints = 20;
    [SerializeField] private float trajectoryTimeStep = 0.1f;

    // 현재 스테이지의 새총. StageManager가 스폰 후 AttachSlingshot으로 넘겨줌.
    private SlingshotVisual currentSlingshot;
    private Rigidbody2D playerRb;
    private bool isDragging;

    private void Awake()
    {
        if (player == null) player = GetComponent<PlayerController>();
        if (mainCamera == null) mainCamera = Camera.main;

        playerRb = player.GetComponent<Rigidbody2D>();

        if (trajectoryLine != null)
            trajectoryLine.positionCount = 0;
    }

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
        ClearTrajectoryPreview();
    }

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
            // 비행 중/사망 등: 밴드 숨기고 드래그 상태 초기화
            currentSlingshot.HideBands();
            isDragging = false;
            ClearTrajectoryPreview();
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
            UpdateTrajectoryPreview(clampedDragPos, pouch);
        }
        else if (mouse.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            Vector2 clampedDragPos = GetClampedDragPosition(worldPos, pouch);
            ClearTrajectoryPreview();

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

    private void UpdateTrajectoryPreview(Vector2 dragPos, Vector2 pouch)
    {
        if (trajectoryLine == null) return;

        Vector2 simVel = CalculateLaunchVelocity(dragPos, pouch);
        Vector2 simPos = pouch;

        float gravityScale = playerRb != null ? playerRb.gravityScale : 1f;
        Vector2 gravity = Physics2D.gravity * gravityScale;

        trajectoryLine.positionCount = trajectoryPoints;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            trajectoryLine.SetPosition(i, simPos);
            simVel += gravity * trajectoryTimeStep;
            simPos += simVel * trajectoryTimeStep;
        }
    }

    private void ClearTrajectoryPreview()
    {
        if (trajectoryLine != null)
            trajectoryLine.positionCount = 0;
    }
}
