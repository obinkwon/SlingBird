using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLauncher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;

    [Header("Launch Settings")]
    [SerializeField] private float launchPower = 8f;
    [SerializeField] private float maxDragDistance = 2.5f;

    [Header("Aim Settings")]
    [SerializeField] private float playerClickRadius = 1f;

    private Camera mainCamera;

    private bool isDragging;
    private Vector2 dragStartPosition;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (player == null)
        {
            player = GetComponent<PlayerController>();
        }
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        if (!player.CanAim())
        {
            return;
        }

        HandleInput();
    }

    private void HandleInput()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartDrag(
                Mouse.current.position.ReadValue()
            );
        }

        if (Mouse.current.leftButton.isPressed &&
            isDragging)
        {
            UpdateDrag(
                Mouse.current.position.ReadValue()
            );
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame &&
            isDragging)
        {
            EndDrag(
                Mouse.current.position.ReadValue()
            );
        }
    }

    private void StartDrag(Vector2 screenPosition)
    {
        Vector2 worldPosition =
            mainCamera.ScreenToWorldPoint(
                screenPosition
            );

        float distance =
            Vector2.Distance(
                worldPosition,
                transform.position
            );

        if (distance > playerClickRadius)
        {
            return;
        }

        isDragging = true;

        dragStartPosition =
            transform.position;

        player.StartAiming();
    }

    private void UpdateDrag(Vector2 screenPosition)
    {
        Vector2 currentPosition =
            mainCamera.ScreenToWorldPoint(
                screenPosition
            );

        Vector2 dragVector =
            currentPosition -
            dragStartPosition;

        dragVector =
            Vector2.ClampMagnitude(
                dragVector,
                maxDragDistance
            );

        transform.position =
            dragStartPosition +
            dragVector;
    }

    private void EndDrag(Vector2 screenPosition)
    {
        isDragging = false;

        Vector2 currentPosition =
            mainCamera.ScreenToWorldPoint(
                screenPosition
            );

        Vector2 dragVector =
            currentPosition -
            dragStartPosition;

        dragVector =
            Vector2.ClampMagnitude(
                dragVector,
                maxDragDistance
            );

        Vector2 launchDirection =
            -dragVector;

        Vector2 velocity =
            launchDirection *
            launchPower;

        // 플레이어를 원래 발사 위치로 복귀
        transform.position =
            dragStartPosition;

        player.Launch(velocity);
    }
}