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

    // ==================================================
    // 입력 시작
    // ==================================================

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

    // ==================================================
    // 조준 시작
    // ==================================================

    private void TryStartAim(Vector2 pointerPosition)
    {
        if (player == null)
            return;

        // Ready 또는 Landed 상태에서만 가능
        if (!player.CanAim())
            return;

        Collider2D hit =
            Physics2D.OverlapPoint(pointerPosition);

        if (hit == null)
            return;

        // 플레이어를 클릭했는지 확인
        if (!hit.transform.IsChildOf(transform) &&
            hit.transform != transform)
        {
            return;
        }

        player.StartAiming();

        isDragging = true;

        // 현재 플레이어 위치를 발사 기준점으로 사용
        dragStartPosition =
            rb.position;

        currentDragPosition =
            dragStartPosition;

        ShowAim();
    }

    // ==================================================
    // 드래그
    // ==================================================

    private void UpdateDrag()
    {
        Vector2 pointerPosition;
        bool released = false;

        // ----------------------------------------------
        // 마우스
        // ----------------------------------------------

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

        // ----------------------------------------------
        // 터치
        // ----------------------------------------------

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

    // ==================================================
    // 당기는 거리 제한
    // ==================================================

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

    // ==================================================
    // 발사
    // ==================================================

    private void Launch()
    {
        isDragging = false;

        Vector2 pullVector =
            dragStartPosition -
            currentDragPosition;

        // 거의 당기지 않았다면 발사하지 않음
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

    // ==================================================
    // 조준 표시
    // ==================================================

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

    // ==================================================
    // 예상 궤적
    // ==================================================

    private void DrawTrajectory()
    {
        if (trajectoryLine == null)
            return;

        Vector2 pullVector =
            dragStartPosition -
            currentDragPosition;

        Vector2 velocity =
            pullVector * launchPower;

        // PlayerController의 최대 속도와 동일하게 제한
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

    // ==================================================
    // 마우스 위치
    // ==================================================

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

    // ==================================================
    // 터치 위치
    // ==================================================

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

    // ==================================================
    // 조준 표시 ON
    // ==================================================

    private void ShowAim()
    {
        if (aimLine != null)
            aimLine.enabled = true;

        if (trajectoryLine != null)
            trajectoryLine.enabled = true;
    }

    // ==================================================
    // 조준 표시 OFF
    // ==================================================

    private void HideAim()
    {
        if (aimLine != null)
            aimLine.enabled = false;

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;
    }

    // ==================================================
    // 외부에서 조준 강제 종료
    // ==================================================

    public void CancelAim()
    {
        isDragging = false;

        HideAim();

        if (player != null &&
            player.State == PlayerController.PlayerState.Aiming)
        {
            player.ResetReady();
        }
    }
}