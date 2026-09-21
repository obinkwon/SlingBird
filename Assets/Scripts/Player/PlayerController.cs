using System;
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

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    public PlayerState State { get; private set; } = PlayerState.Ready;

    /// <summary>최대 발사 속도 (PlayerLauncher가 궤적 계산에 사용)</summary>
    public float MaxSpeed => maxSpeed;

    /// <summary>상태가 바뀔 때 호출됨</summary>
    public event Action<PlayerState> OnStateChanged;

    private RigidbodyConstraints2D originalConstraints;
    private Platform currentPlatform;
    private Collider2D playerCollider;
    private Collider2D ignoredPlatformCollider;
    private float ignoreEndTime;

    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        playerCollider = GetComponent<Collider2D>();

        if (rb == null)
        {
            Debug.LogError("PlayerController: Rigidbody2D가 없습니다.");
            return;
        }

        if (playerCollider == null)
            Debug.LogError("PlayerController: Collider2D가 없습니다.");

        // 빠르게 움직여도 얇은 플랫폼을 통과하지 않도록
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        originalConstraints = rb.constraints;
    }

    private void FixedUpdate()
    {
        // 발사 직후 무시했던 플랫폼 충돌은
        // 최소 시간이 지나고, 더 이상 겹치지 않을 때 복구
        if (ignoredPlatformCollider == null || Time.time < ignoreEndTime)
            return;

        if (playerCollider == null)
            return;

        ColliderDistance2D distance =
            playerCollider.Distance(ignoredPlatformCollider);

        if (!distance.isOverlapped)
            RestorePlatformCollision();
    }

    // =========================================================
    // Platform
    // =========================================================

    public void SetCurrentPlatform(Platform platform)
    {
        currentPlatform = platform;

        Log(platform != null
            ? $"Current Platform Set: {platform.name}"
            : "Current Platform Cleared");
    }

    public bool IsCurrentPlatform(Platform platform)
    {
        return currentPlatform == platform;
    }

    // =========================================================
    // Position
    // =========================================================

    public void SetStartPosition(Vector2 position)
    {
        if (rb == null)
            return;

        RestorePlatformCollision();
        StopMotion();
        TeleportTo(position);

        SetState(PlayerState.Ready);
    }

    public void MoveToPlatform(Vector2 position)
    {
        if (rb == null)
            return;

        // 혹시 이전 플랫폼 충돌 무시가 남아 있다면 복구
        RestorePlatformCollision();
        StopMotion();
        TeleportTo(position);

        SetState(PlayerState.Landed);

        Log($"Player moved to new platform: {position}");
    }

    // =========================================================
    // Aiming
    // =========================================================

    public void StartAiming()
    {
        if (!CanAim() || rb == null)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 조준 중에는 물리로 움직이지 않도록 고정
        rb.constraints =
            originalConstraints |
            RigidbodyConstraints2D.FreezePosition |
            RigidbodyConstraints2D.FreezeRotation;

        SetState(PlayerState.Aiming);
    }

    // =========================================================
    // Launch
    // =========================================================

    public void Launch(Vector2 velocity)
    {
        if (State != PlayerState.Aiming)
        {
            Debug.LogWarning($"Launch rejected. Current State: {State}");
            return;
        }

        if (rb == null)
            return;

        // 조준 중 Freeze 해제
        rb.constraints = originalConstraints;
        rb.angularVelocity = 0f;

        IgnoreCurrentPlatformCollision();

        rb.linearVelocity = Vector2.ClampMagnitude(velocity, maxSpeed);

        SetState(PlayerState.Flying);

        Log($"Player Launched | Velocity: {rb.linearVelocity}");
    }

    // =========================================================
    // Ignore / Restore Platform Collision
    // =========================================================

    private void IgnoreCurrentPlatformCollision()
    {
        if (playerCollider == null || currentPlatform == null)
            return;

        Collider2D platformCollider =
            currentPlatform.GetComponent<Collider2D>();

        if (platformCollider == null)
            return;

        // 이전에 무시 중이던 게 있으면 먼저 복구
        RestorePlatformCollision();

        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);

        ignoredPlatformCollider = platformCollider;
        ignoreEndTime = Time.time + launchIgnoreCollisionTime;

        Log($"Ignore Platform Collision: {currentPlatform.name}");
    }

    private void RestorePlatformCollision()
    {
        if (playerCollider == null || ignoredPlatformCollider == null)
            return;

        Physics2D.IgnoreCollision(playerCollider, ignoredPlatformCollider, false);

        ignoredPlatformCollider = null;

        Log("Platform Collision Restored");
    }

    // =========================================================
    // Land / Goal / Reset / Death
    // =========================================================

    public void Land()
    {
        if (State != PlayerState.Flying || rb == null)
            return;

        StopMotion();

        SetState(PlayerState.Landed);
    }

    public void ReachGoal()
    {
        if (State != PlayerState.Flying)
            return;

        if (rb != null)
            Log($"Goal Reached | Current Velocity: {rb.linearVelocity}");
    }

    public void ResetReady()
    {
        RestorePlatformCollision();

        if (rb == null)
            return;

        StopMotion();

        SetState(PlayerState.Ready);
    }

    public void Die()
    {
        if (State == PlayerState.Dead)
            return;

        SetState(PlayerState.Dead);

        RestorePlatformCollision();

        if (rb != null)
            StopMotion();
    }

    // =========================================================
    // State Queries
    // =========================================================

    public bool CanAim()
    {
        return State == PlayerState.Ready ||
               State == PlayerState.Landed;
    }

    public bool IsFlying()
    {
        return State == PlayerState.Flying;
    }

    public bool IsLanded()
    {
        return State == PlayerState.Landed;
    }

    // =========================================================
    // Helpers
    // =========================================================

    private void SetState(PlayerState newState)
    {
        if (State == newState)
            return;

        State = newState;
        OnStateChanged?.Invoke(newState);

        Log($"State -> {newState}");
    }

    private void StopMotion()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = originalConstraints;
    }

    private void TeleportTo(Vector2 position)
    {
        rb.position = position;
        transform.position = position;

        Physics2D.SyncTransforms();
    }

    private void Log(string message)
    {
        if (debugLog)
            Debug.Log($"[Player] {message}");
    }
}
