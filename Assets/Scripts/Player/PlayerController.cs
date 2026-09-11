using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Ready,
        Aiming,
        Flying,
        Landed,
        Dead
    }

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 15f;

    [Header("Launch Collision")]
    [SerializeField] private float launchIgnoreCollisionTime = 0.15f;

    public PlayerState State { get; private set; }

    private RigidbodyConstraints2D originalConstraints;

    private Platform currentPlatform;

    private Collider2D playerCollider;

    private Collider2D ignoredPlatformCollider;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        playerCollider =
            GetComponent<Collider2D>();

        if (rb == null)
        {
            Debug.LogError(
                "PlayerController: Rigidbody2D가 없습니다."
            );

            return;
        }

        if (playerCollider == null)
        {
            Debug.LogError(
                "PlayerController: Collider2D가 없습니다."
            );
        }

        originalConstraints =
            rb.constraints;

        State = PlayerState.Ready;
    }

    // =========================================================
    // Platform
    // =========================================================

    public void SetCurrentPlatform(
        Platform platform)
    {
        currentPlatform = platform;

        if (platform != null)
        {
            Debug.Log(
                $"Current Platform Set: {platform.name}"
            );
        }
        else
        {
            Debug.Log(
                "Current Platform Cleared"
            );
        }
    }

    public bool IsCurrentPlatform(
        Platform platform)
    {
        return currentPlatform == platform;
    }

    // =========================================================
    // Position
    // =========================================================

    public void SetStartPosition(
        Vector2 position)
    {
        if (rb == null)
            return;

        rb.linearVelocity =
            Vector2.zero;

        rb.angularVelocity = 0f;

        rb.constraints =
            originalConstraints;

        rb.position = position;

        transform.position =
            position;

        Physics2D.SyncTransforms();

        State =
            PlayerState.Ready;
    }

    // =========================================================
    // Aiming
    // =========================================================

    public void StartAiming()
    {
        if (State != PlayerState.Ready &&
            State != PlayerState.Landed)
        {
            return;
        }

        if (rb == null)
            return;

        rb.linearVelocity =
            Vector2.zero;

        rb.angularVelocity = 0f;

        rb.constraints =
            originalConstraints |
            RigidbodyConstraints2D.FreezePosition |
            RigidbodyConstraints2D.FreezeRotation;

        State =
            PlayerState.Aiming;

        Debug.Log(
            "Player Aiming"
        );
    }

    // =========================================================
    // Launch
    // =========================================================

    public void Launch(
        Vector2 velocity)
    {
        if (State != PlayerState.Aiming)
        {
            Debug.LogWarning(
                $"Launch rejected. Current State: {State}"
            );

            return;
        }

        if (rb == null)
            return;

        // -----------------------------------------------------
        // 조준 중 Freeze 해제
        // -----------------------------------------------------

        rb.constraints =
            originalConstraints;

        rb.angularVelocity = 0f;

        // -----------------------------------------------------
        // 현재 플랫폼과의 충돌 무시
        // -----------------------------------------------------

        IgnoreCurrentPlatformCollision();

        // -----------------------------------------------------
        // 발사 속도
        // -----------------------------------------------------

        velocity =
            Vector2.ClampMagnitude(
                velocity,
                maxSpeed
            );

        rb.linearVelocity =
            velocity;

        State =
            PlayerState.Flying;

        Debug.Log(
            $"Player Launched | Velocity: {rb.linearVelocity}"
        );

        // -----------------------------------------------------
        // 디버그
        // -----------------------------------------------------

        Invoke(
            nameof(DebugVelocity),
            0.1f
        );
    }

    // =========================================================
    // Ignore Current Platform
    // =========================================================

    private void IgnoreCurrentPlatformCollision()
    {
        if (playerCollider == null)
            return;

        if (currentPlatform == null)
            return;

        Collider2D platformCollider =
            currentPlatform.GetComponent<Collider2D>();

        if (platformCollider == null)
            return;

        Physics2D.IgnoreCollision(
            playerCollider,
            platformCollider,
            true
        );

        ignoredPlatformCollider =
            platformCollider;

        Debug.Log(
            $"Ignore Platform Collision: {currentPlatform.name}"
        );

        CancelInvoke(
            nameof(RestorePlatformCollision)
        );

        Invoke(
            nameof(RestorePlatformCollision),
            launchIgnoreCollisionTime
        );
    }

    // =========================================================
    // Restore Platform Collision
    // =========================================================

    private void RestorePlatformCollision()
    {
        if (playerCollider == null)
            return;

        if (ignoredPlatformCollider == null)
            return;

        Physics2D.IgnoreCollision(
            playerCollider,
            ignoredPlatformCollider,
            false
        );

        Debug.Log(
            "Platform Collision Restored"
        );

        ignoredPlatformCollider =
            null;
    }

    // =========================================================
    // Land
    // =========================================================

    public void Land()
    {
        if (State != PlayerState.Flying)
        {
            return;
        }

        if (rb == null)
            return;

        rb.linearVelocity =
            Vector2.zero;

        rb.angularVelocity = 0f;

        rb.constraints =
            originalConstraints;

        State =
            PlayerState.Landed;

        Debug.Log(
            "Player Landed"
        );
    }

    // =========================================================
    // Goal
    // =========================================================

    public void ReachGoal()
    {
        if (State != PlayerState.Flying)
        {
            return;
        }

        if (rb != null)
        {
            Debug.Log(
                $"Goal Reached | Current Velocity: {rb.linearVelocity}"
            );
        }
    }

    // =========================================================
    // Move To Platform
    // =========================================================

    public void MoveToPlatform(
        Vector2 position)
    {
        if (rb == null)
            return;

        // 혹시 이전 플랫폼 충돌 무시가 남아 있다면 복구
        RestorePlatformCollision();

        rb.linearVelocity =
            Vector2.zero;

        rb.angularVelocity = 0f;

        rb.constraints =
            originalConstraints;

        rb.position =
            position;

        transform.position =
            position;

        Physics2D.SyncTransforms();

        State =
            PlayerState.Landed;

        Debug.Log(
            $"Player moved to new platform: {position}"
        );
    }

    // =========================================================
    // Reset
    // =========================================================

    public void ResetReady()
    {
        RestorePlatformCollision();

        if (rb == null)
            return;

        rb.linearVelocity =
            Vector2.zero;

        rb.angularVelocity = 0f;

        rb.constraints =
            originalConstraints;

        State =
            PlayerState.Ready;

        Debug.Log(
            "Player Reset Ready"
        );
    }

    // =========================================================
    // Death
    // =========================================================

    public void Die()
    {
        if (State == PlayerState.Dead)
        {
            return;
        }

        State =
            PlayerState.Dead;

        RestorePlatformCollision();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity = 0f;

            rb.constraints =
                originalConstraints;
        }

        Debug.Log(
            "Player Dead"
        );
    }

    // =========================================================
    // State
    // =========================================================

    public bool CanAim()
    {
        return State == PlayerState.Ready ||
               State == PlayerState.Landed;
    }

    public bool IsFlying()
    {
        return State ==
               PlayerState.Flying;
    }

    public bool IsLanded()
    {
        return State ==
               PlayerState.Landed;
    }

    // =========================================================
    // Debug
    // =========================================================

    private void DebugVelocity()
    {
        if (rb == null)
            return;

        Debug.Log(
            $"[0.1s AFTER LAUNCH] " +
            $"Velocity: {rb.linearVelocity} | " +
            $"Position: {rb.position} | " +
            $"Constraints: {rb.constraints}"
        );
    }
}