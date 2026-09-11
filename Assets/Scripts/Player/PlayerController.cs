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

    public PlayerState State { get; private set; }

    private RigidbodyConstraints2D originalConstraints;

    // 현재 플레이어가 서 있는 플랫폼
    private Platform currentPlatform;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb == null)
        {
            Debug.LogError(
                "PlayerController: Rigidbody2D가 없습니다."
            );

            return;
        }

        originalConstraints = rb.constraints;

        State = PlayerState.Ready;
    }

    // =========================================================
    // Platform
    // =========================================================

    /// <summary>
    /// 현재 플레이어가 서 있는 플랫폼을 등록한다.
    /// </summary>
    public void SetCurrentPlatform(Platform platform)
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

    /// <summary>
    /// 전달된 플랫폼이 현재 출발/착지 플랫폼인지 확인한다.
    /// </summary>
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

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.constraints = originalConstraints;

        rb.position = position;
        transform.position = position;

        Physics2D.SyncTransforms();

        State = PlayerState.Ready;
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

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 조준 중에는 플레이어 이동을 완전히 막는다.
        rb.constraints =
            originalConstraints |
            RigidbodyConstraints2D.FreezePosition |
            RigidbodyConstraints2D.FreezeRotation;

        State = PlayerState.Aiming;

        Debug.Log("Player Aiming");
    }

    // =========================================================
    // Launch
    // =========================================================

    public void Launch(Vector2 velocity)
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

        // 조준 중 걸어둔 Freeze 해제
        rb.constraints = originalConstraints;

        rb.angularVelocity = 0f;

        // 최대 속도 제한
        velocity =
            Vector2.ClampMagnitude(
                velocity,
                maxSpeed
            );

        // 실제 발사
        rb.linearVelocity = velocity;

        State = PlayerState.Flying;

        Debug.Log(
            $"Player Launched | Velocity: {rb.linearVelocity}"
        );
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

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.constraints = originalConstraints;

        State = PlayerState.Landed;

        Debug.Log("Player Landed");
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

        // 여기서는 속도를 0으로 만들지 않는다.
        // StageManager가 새 플랫폼으로 이동시킨다.
    }

    // =========================================================
    // Move To New Platform
    // =========================================================

    public void MoveToPlatform(Vector2 position)
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.constraints = originalConstraints;

        rb.position = position;
        transform.position = position;

        Physics2D.SyncTransforms();

        State = PlayerState.Landed;

        Debug.Log(
            $"Player moved to new platform: {position}"
        );
    }

    // =========================================================
    // Reset
    // =========================================================

    public void ResetReady()
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.constraints = originalConstraints;

        State = PlayerState.Ready;

        Debug.Log("Player Reset Ready");
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

        State = PlayerState.Dead;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            rb.constraints = originalConstraints;
        }

        Debug.Log("Player Dead");
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
        return State == PlayerState.Flying;
    }

    public bool IsLanded()
    {
        return State == PlayerState.Landed;
    }
}