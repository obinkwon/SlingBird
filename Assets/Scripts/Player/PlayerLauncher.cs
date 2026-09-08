using UnityEngine;

public class PlayerLauncher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;

    [Header("Launch Settings")]
    [SerializeField] private float launchPower = 8f;
    [SerializeField] private float maxDragDistance = 2.5f;

    private Camera mainCamera;

    private bool isDragging;
    private Vector2 dragStartPosition;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (player == null)
            player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (player.State != PlayerController.PlayerState.Ready)
            return;

        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag(Input.mousePosition);
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateDrag(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag(Input.mousePosition);
        }
    }

    private void StartDrag(Vector2 screenPosition)
    {
        Vector2 worldPosition =
            mainCamera.ScreenToWorldPoint(screenPosition);

        float distance = Vector2.Distance(
            worldPosition,
            transform.position
        );

        if (distance > 1.0f)
            return;

        isDragging = true;
        dragStartPosition = worldPosition;
    }

    private void UpdateDrag(Vector2 screenPosition)
    {
        Vector2 currentPosition =
            mainCamera.ScreenToWorldPoint(screenPosition);

        Vector2 dragVector =
            currentPosition - dragStartPosition;

        dragVector = Vector2.ClampMagnitude(
            dragVector,
            maxDragDistance
        );

        transform.position =
            (Vector2)transform.position + dragVector;
    }

    private void EndDrag(Vector2 screenPosition)
    {
        isDragging = false;

        Vector2 currentPosition =
            mainCamera.ScreenToWorldPoint(screenPosition);

        Vector2 dragVector =
            currentPosition - dragStartPosition;

        dragVector = Vector2.ClampMagnitude(
            dragVector,
            maxDragDistance
        );

        Vector2 launchDirection = -dragVector;

        Vector2 velocity =
            launchDirection * launchPower;

        player.Launch(velocity);
    }
}