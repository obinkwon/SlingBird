using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLauncher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Camera mainCamera;

    [Header("Launch Settings")]
    [SerializeField] private float launchPower = 8f;
    [SerializeField] private float maxDragDistance = 2.5f;
    [SerializeField] private float minPullDistance = 0.15f;

    [Header("Aim Settings")]
    [SerializeField] private float playerClickRadius = 1f;

    [Header("Aim Line")]
    [SerializeField] private LineRenderer aimLine;

    [Header("Trajectory")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPointCount = 30;
    [SerializeField] private float trajectoryTimeStep = 0.08f;

    private bool isDragging;
    private Vector2 dragStartPosition;
    private Vector2 currentDragPosition;
    private Vector3[] trajectoryPoints;

    // --------------------------------------------------
    // Unity
    // --------------------------------------------------

    private void Awake()
    {
        if (player == null)
            player = GetComponent<PlayerController>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (aimLine != null)
            aimLine.sortingOrder = 1;

        if (trajectoryLine != null)
            trajectoryLine.sortingOrder = 1;

        HideAim();
    }

    private void Update()
    {
        if (player == null)
            return;

        // 드래그 중에 플레이어 상태가 외부에서 바뀌면(사망 등) 조준 취소
        if (isDragging && player.State != PlayerController.PlayerState.Aiming)
        {
            isDragging = false;
            HideAim();
            return;
        }

        // 드래그 중이 아닐 때만 CanAim 검사
        if (!isDragging && !player.CanAim())
            return;

        HandleInput();
    }

    // --------------------------------------------------
    // Input (마우스 + 터치 공용)
    // --------------------------------------------------

    private void HandleInput()
    {
        Pointer pointer = Pointer.current;

        if (pointer == null)
            return;

        Vector2 screenPosition = pointer.position.ReadValue();

        if (pointer.press.wasPressedThisFrame)
            StartDrag(screenPosition);

        if (pointer.press.isPressed && isDragging)
            UpdateDrag(screenPosition);

        if (pointer.press.wasReleasedThisFrame && isDragging)
            EndDrag(screenPosition);
    }

    // --------------------------------------------------
    // Drag
    // --------------------------------------------------

    private void StartDrag(Vector2 screenPosition)
    {
        Vector2 worldPosition = GetWorldPosition(screenPosition);
        Vector2 playerPosition = GetPlayerPosition();

        if (Vector2.Distance(worldPosition, playerPosition) > playerClickRadius)
            return;

        player.StartAiming();

        // StartAiming이 거부된 경우(상태 불일치) 드래그 시작 안 함
        if (player.State != PlayerController.PlayerState.Aiming)
            return;

        dragStartPosition = playerPosition;
        currentDragPosition = dragStartPosition;
        isDragging = true;

        ShowAim();
        DrawAim();
    }

    private void UpdateDrag(Vector2 screenPosition)
    {
        currentDragPosition = ClampDragPosition(GetWorldPosition(screenPosition));

        // 조준 중에는 플레이어를 실제로 이동
        SetPlayerPosition(currentDragPosition);

        DrawAim();
    }

    private void EndDrag(Vector2 screenPosition)
    {
        isDragging = false;

        currentDragPosition = ClampDragPosition(GetWorldPosition(screenPosition));

        Vector2 pullVector = dragStartPosition - currentDragPosition;

        // 너무 조금 당겼으면 발사하지 않음
        if (pullVector.magnitude < minPullDistance)
        {
            SetPlayerPosition(dragStartPosition);
            player.ResetReady();
            HideAim();
            return;
        }

        // 궤적과 실제 발사가 같은 계산을 쓰도록 먼저 속도를 구함
        Vector2 velocity = CalculateLaunchVelocity();

        // 플레이어를 원래 위치로 복귀 후 발사
        SetPlayerPosition(dragStartPosition);
        HideAim();

        player.Launch(velocity);
    }

    private Vector2 ClampDragPosition(Vector2 position)
    {
        Vector2 offset = position - dragStartPosition;

        if (offset.magnitude > maxDragDistance)
            offset = offset.normalized * maxDragDistance;

        return dragStartPosition + offset;
    }

    // --------------------------------------------------
    // Launch Velocity (발사 / 궤적 공용)
    // --------------------------------------------------

    private Vector2 CalculateLaunchVelocity()
    {
        Vector2 pullVector = dragStartPosition - currentDragPosition;

        return Vector2.ClampMagnitude(pullVector * launchPower, player.MaxSpeed);
    }

    // --------------------------------------------------
    // Aim Drawing
    // --------------------------------------------------

    private void DrawAim()
    {
        if (aimLine != null)
        {
            aimLine.positionCount = 2;
            aimLine.SetPosition(0, dragStartPosition);
            aimLine.SetPosition(1, currentDragPosition);
        }

        DrawTrajectory();
    }

    private void DrawTrajectory()
    {
        if (trajectoryLine == null || rb == null)
            return;

        Vector2 velocity = CalculateLaunchVelocity();
        Vector2 gravity = Physics2D.gravity * rb.gravityScale;

        if (trajectoryPoints == null || trajectoryPoints.Length != trajectoryPointCount)
            trajectoryPoints = new Vector3[trajectoryPointCount];

        for (int i = 0; i < trajectoryPointCount; i++)
        {
            float t = i * trajectoryTimeStep;

            trajectoryPoints[i] =
                dragStartPosition +
                velocity * t +
                0.5f * gravity * t * t;
        }

        trajectoryLine.positionCount = trajectoryPointCount;
        trajectoryLine.SetPositions(trajectoryPoints);
    }

    private void ShowAim()
    {
        if (aimLine != null)
            aimLine.enabled = true;

        if (trajectoryLine != null)
            trajectoryLine.enabled = true;
    }

    private void HideAim()
    {
        if (aimLine != null)
            aimLine.enabled = false;

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;
    }

    // --------------------------------------------------
    // Player Position
    // --------------------------------------------------

    private Vector2 GetPlayerPosition()
    {
        return rb != null ? rb.position : (Vector2)transform.position;
    }

    private void SetPlayerPosition(Vector2 position)
    {
        if (rb != null)
            rb.position = position;
        else
            transform.position = position;
    }

    // --------------------------------------------------
    // World Position
    // --------------------------------------------------

    private Vector2 GetWorldPosition(Vector2 screenPosition)
    {
        if (mainCamera == null)
            return transform.position;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                Mathf.Abs(mainCamera.transform.position.z)
            )
        );

        return worldPosition;
    }

    // --------------------------------------------------
    // Cancel
    // --------------------------------------------------

    public void CancelAim()
    {
        isDragging = false;

        HideAim();

        if (player != null &&
            player.State == PlayerController.PlayerState.Aiming)
        {
            SetPlayerPosition(dragStartPosition);
            player.ResetReady();
        }
    }
}
