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

    private void Awake()
    {
        if (player == null)
        {
            player = GetComponent<PlayerController>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        HideAim();
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        // 드래그 중이 아닐 때만 CanAim 검사
        if (!isDragging && !player.CanAim())
        {
            return;
        }

        HandleInput();
    }

    // --------------------------------------------------
    // Input
    // --------------------------------------------------

    private void HandleInput()
    {
        if (Mouse.current == null)
        {
            return;
        }

        // 클릭 시작
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartDrag(
                Mouse.current.position.ReadValue()
            );
        }

        // 드래그 중
        if (Mouse.current.leftButton.isPressed &&
            isDragging)
        {
            UpdateDrag(
                Mouse.current.position.ReadValue()
            );
        }

        // 클릭 해제
        if (Mouse.current.leftButton.wasReleasedThisFrame &&
            isDragging)
        {
            EndDrag(
                Mouse.current.position.ReadValue()
            );
        }
    }

    // --------------------------------------------------
    // Start Drag
    // --------------------------------------------------

    private void StartDrag(Vector2 screenPosition)
    {
        Vector2 worldPosition =
            GetWorldPosition(screenPosition);

        Vector2 playerPosition =
            rb != null
                ? rb.position
                : (Vector2)transform.position;

        float distance =
            Vector2.Distance(
                worldPosition,
                playerPosition
            );

        if (distance > playerClickRadius)
        {
            return;
        }

        dragStartPosition =
            playerPosition;

        currentDragPosition =
            dragStartPosition;

        isDragging = true;

        player.StartAiming();

        ShowAim();

        DrawAim();

        Debug.Log(
            $"Start Drag | Position: {dragStartPosition}"
        );
    }

    // --------------------------------------------------
    // Update Drag
    // --------------------------------------------------

    private void UpdateDrag(Vector2 screenPosition)
    {
        Vector2 currentPosition =
            GetWorldPosition(screenPosition);

        currentDragPosition =
            ClampDragPosition(currentPosition);

        // 조준 중에는 플레이어를 실제로 이동
        if (rb != null)
        {
            rb.position =
                currentDragPosition;
        }
        else
        {
            transform.position =
                currentDragPosition;
        }

        DrawAim();
    }

    // --------------------------------------------------
    // End Drag
    // --------------------------------------------------

    private void EndDrag(Vector2 screenPosition)
    {
        isDragging = false;

        Vector2 currentPosition =
            GetWorldPosition(screenPosition);

        currentDragPosition =
            ClampDragPosition(currentPosition);

        Vector2 pullVector =
            dragStartPosition -
            currentDragPosition;

        // 너무 조금 당겼으면 발사하지 않음
        if (pullVector.magnitude < 0.15f)
        {
            ResetPlayerPosition();

            player.ResetReady();

            HideAim();

            Debug.Log("Drag cancelled - too short");

            return;
        }

        Vector2 velocity = pullVector * launchPower;
        Debug.Log(
            $"Pull: {pullVector} / Velocity: {velocity}"
        );

        // 플레이어를 원래 위치로 복귀
        ResetPlayerPosition();

        HideAim();

        Debug.Log(
            $"End Drag | Pull: {pullVector} | Velocity: {velocity}"
        );

        // 실제 발사
        player.Launch(velocity);
    }

    // --------------------------------------------------
    // Clamp
    // --------------------------------------------------

    private Vector2 ClampDragPosition(Vector2 position)
    {
        Vector2 offset =
            position -
            dragStartPosition;

        if (offset.magnitude >
            maxDragDistance)
        {
            offset =
                offset.normalized *
                maxDragDistance;
        }

        return dragStartPosition + offset;
    }

    // --------------------------------------------------
    // Aim Drawing
    // --------------------------------------------------

    private void DrawAim()
    {
        if (aimLine != null)
        {
            aimLine.positionCount = 2;

            aimLine.SetPosition(
                0,
                dragStartPosition
            );

            aimLine.SetPosition(
                1,
                currentDragPosition
            );
        }

        DrawTrajectory();
    }

    // --------------------------------------------------
    // Trajectory
    // --------------------------------------------------

    private void DrawTrajectory()
    {
        if (trajectoryLine == null ||
            rb == null)
        {
            return;
        }

        Vector2 pullVector =
            dragStartPosition -
            currentDragPosition;

        Vector2 velocity =
            pullVector *
            launchPower;

        // 실제 발사 속도와 동일하게 제한
        velocity =
            Vector2.ClampMagnitude(
                velocity,
                15f
            );

        trajectoryLine.positionCount =
            trajectoryPointCount;

        Vector2 gravity =
            Physics2D.gravity *
            rb.gravityScale;

        for (int i = 0;
             i < trajectoryPointCount;
             i++)
        {
            float time =
                i * trajectoryTimeStep;

            Vector2 position =
                dragStartPosition +
                velocity * time +
                0.5f *
                gravity *
                time *
                time;

            trajectoryLine.SetPosition(
                i,
                position
            );
        }
    }

    // --------------------------------------------------
    // Player Position
    // --------------------------------------------------

    private void ResetPlayerPosition()
    {
        if (rb != null)
        {
            rb.position =
                dragStartPosition;
        }
        else
        {
            transform.position =
                dragStartPosition;
        }
    }

    // --------------------------------------------------
    // World Position
    // --------------------------------------------------

    private Vector2 GetWorldPosition(
        Vector2 screenPosition)
    {
        if (mainCamera == null)
        {
            return transform.position;
        }

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    Mathf.Abs(
                        mainCamera.transform.position.z
                    )
                )
            );

        return worldPosition;
    }

    // --------------------------------------------------
    // Aim Visibility
    // --------------------------------------------------

    private void ShowAim()
    {
        if (aimLine != null)
        {
            aimLine.enabled = true;
        }

        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = true;
        }
    }

    private void HideAim()
    {
        if (aimLine != null)
        {
            aimLine.enabled = false;
        }

        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }
    }

    // --------------------------------------------------
    // Cancel
    // --------------------------------------------------

    public void CancelAim()
    {
        isDragging = false;

        HideAim();

        if (player != null &&
            player.State ==
            PlayerController.PlayerState.Aiming)
        {
            ResetPlayerPosition();

            player.ResetReady();
        }
    }
}
