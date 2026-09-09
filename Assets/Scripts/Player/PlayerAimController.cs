using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Camera mainCamera;

    [Header("Aim")]
    [SerializeField] private float maxPullDistance = 3f;
    [SerializeField] private float launchPower = 8f;

    [Header("Trajectory")]
    [SerializeField] private LineRenderer aimLine;
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
        if (mainCamera == null)
            return;

        if (isDragging)
        {
            UpdateDrag();
        }
        else
        {
            CheckInputStart();
        }
    }

    private void CheckInputStart()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPosition =
                GetMouseWorldPosition();

            TryStartAim(worldPosition);
        }

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 worldPosition =
                GetTouchWorldPosition();

            TryStartAim(worldPosition);
        }
    }

    private void TryStartAim(Vector2 pointerPosition)
    {
        if (!player.CanAim())
            return;

        Collider2D hit =
            Physics2D.OverlapPoint(pointerPosition);

        if (hit == null)
            return;

        if (!hit.transform.IsChildOf(transform) &&
            hit.transform != transform)
        {
            return;
        }

        player.StartAiming();

        isDragging = true;

        dragStartPosition =
            rb.position;

        currentDragPosition =
            dragStartPosition;

        ShowAim();
    }

    private void UpdateDrag()
    {
        Vector2 pointerPosition;
        bool released = false;

        // 마우스
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                pointerPosition =
                    GetMouseWorldPosition();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                pointerPosition =
                    GetMouseWorldPosition();

                released = true;
            }
            else
            {
                return;
            }
        }
        // 터치
        else if (Touchscreen.current != null)
        {
            var touch =
                Touchscreen.current.primaryTouch;

            if (touch.press.isPressed)
            {
                pointerPosition =
                    GetTouchWorldPosition();
            }
            else if (touch.press.wasReleasedThisFrame)
            {
                pointerPosition =
                    GetTouchWorldPosition();

                released = true;
            }
            else
            {
                return;
            }
        }
        else
        {
            return;
        }

        currentDragPosition =
            ClampDragPosition(pointerPosition);

        DrawAim();

        if (released)
        {
            Launch();
        }
    }

    private Vector2 ClampDragPosition(Vector2 position)
    {
        Vector2 offset =
            position -
            dragStartPosition;

        if (offset.magnitude >
            maxPullDistance)
        {
            offset =
                offset.normalized *
                maxPullDistance;
        }

        return dragStartPosition + offset;
    }

    private void Launch()
    {
        isDragging = false;

        Vector2 pullVector =
            dragStartPosition -
            currentDragPosition;

        if (pullVector.magnitude < 0.15f)
        {
            player.ResetReady();

            HideAim();

            return;
        }

        Vector2 launchVelocity =
            pullVector * launchPower;

        player.Launch(launchVelocity);

        HideAim();
    }

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

    private void DrawTrajectory()
    {
        if (trajectoryLine == null)
            return;

        Vector2 pullVector =
            dragStartPosition -
            currentDragPosition;

        Vector2 velocity =
            pullVector * launchPower;

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

    private Vector2 GetMouseWorldPosition()
    {
        Vector3 screenPosition =
            Mouse.current.position.ReadValue();

        screenPosition.z =
            Mathf.Abs(
                mainCamera.transform.position.z
            );

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                screenPosition
            );

        return worldPosition;
    }

    private Vector2 GetTouchWorldPosition()
    {
        Vector2 screenPosition =
            Touchscreen.current.primaryTouch
                .position.ReadValue();

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
}