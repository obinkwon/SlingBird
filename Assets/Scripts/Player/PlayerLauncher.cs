using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어를 드래그해서 앵그리버드처럼 발사하는 스크립트.
/// 새총 스틱(Y자 이미지)은 스테이지마다 새로 스폰되므로,
/// 시각 요소는 SlingshotVisual 컴포넌트로 분리되어 있고
/// StageManager/Goal이 AttachSlingshot()으로 매 스테이지마다 새 새총을 연결해준다.
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
    [SerializeField] private float maxLaunchSpeed = 15f;

    [Header("궤적 예측선 (선택)")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPoints = 20;
    [SerializeField] private float trajectoryTimeStep = 0.1f;

    // 현재 스테이지의 새총. StageManager/Goal이 스폰 후 AttachSlingshot으로 넘겨줌.
    private SlingshotVisual currentSlingshot;
    private bool isDragging;

    private void Awake()
    {
        if (player == null) player = GetComponent<PlayerController>();
        if (mainCamera == null) mainCamera = Camera.main;

        if (trajectoryLine != null)
            trajectoryLine.positionCount = 0;
    }

    /// <summary>
    /// 새 스테이지가 시작될 때 StageManager(또는 Goal)가 호출해서
    /// 새로 스폰된 새총을 이 플레이어의 발사 기준으로 연결한다.
    /// </summary>
    public void AttachSlingshot(SlingshotVisual slingshot)
    {
        // 이전 새총이 남아있다면 밴드부터 정리
        currentSlingshot?.HideBands();

        currentSlingshot = slingshot;
        isDragging = false;
        ClearTrajectoryPreview();
    }

    private void Update()
    {
        if (currentSlingshot == null)
            return; // 아직 새총이 연결되지 않음 (스폰 대기 등)

        if (player.State != PlayerController.PlayerState.Ready &&
            player.State != PlayerController.PlayerState.Aiming)
        {
            currentSlingshot.HideBands();
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
            isDragging = true;
            player.SetAiming(true);
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
            LaunchPlayer(clampedDragPos, pouch);
            ClearTrajectoryPreview();
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

    private void LaunchPlayer(Vector2 dragPos, Vector2 pouch)
    {
        Vector2 pullVector = pouch - dragPos;
        Vector2 velocity = pullVector * launchPower;

        if (velocity.magnitude > maxLaunchSpeed)
            velocity = velocity.normalized * maxLaunchSpeed;

        player.Launch(velocity);
        currentSlingshot.HideBands();

        // 발사된 새총은 더 이상 쓰지 않음 -> 다음 스테이지에서 AttachSlingshot으로 새로 연결됨
        currentSlingshot = null;
    }

    private void UpdateTrajectoryPreview(Vector2 dragPos, Vector2 pouch)
    {
        if (trajectoryLine == null) return;

        Vector2 pullVector = pouch - dragPos;
        Vector2 startVelocity = pullVector * launchPower;

        if (startVelocity.magnitude > maxLaunchSpeed)
            startVelocity = startVelocity.normalized * maxLaunchSpeed;

        trajectoryLine.positionCount = trajectoryPoints;
        Vector2 simPos = pouch;
        Vector2 simVel = startVelocity;
        float gravityScale = Physics2D.gravity.y;

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
