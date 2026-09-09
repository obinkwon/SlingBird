using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Ready,
        Aiming,
        Flying,
        Landed,
        Arrived,
        Dead
    }

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 15f;

    public PlayerState State { get; private set; }

    private RigidbodyConstraints2D originalConstraints;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        originalConstraints = rb.constraints;

        State = PlayerState.Ready;
    }

    public void SetStartPosition(Vector2 position)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.position = position;
        transform.position = position;

        Physics2D.SyncTransforms();

        State = PlayerState.Ready;
    }

    public void StartAiming()
    {
        if (State != PlayerState.Ready &&
            State != PlayerState.Landed)
        {
            return;
        }

        // 기존 움직임 제거
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 당기는 동안 위치와 회전을 완전히 고정
        rb.constraints =
            originalConstraints |
            RigidbodyConstraints2D.FreezePosition |
            RigidbodyConstraints2D.FreezeRotation;

        State = PlayerState.Aiming;
    }

    public void Launch(Vector2 velocity)
    {
        if (State != PlayerState.Aiming)
        {
            return;
        }

        // 물리 고정 해제
        rb.constraints = originalConstraints;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 최대 속도 제한
        velocity =
            Vector2.ClampMagnitude(
                velocity,
                maxSpeed
            );

        rb.linearVelocity = velocity;

        State = PlayerState.Flying;
    }

    public void Land()
    {
        if (State != PlayerState.Flying)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        State = PlayerState.Landed;

        Debug.Log("Player Landed");
    }

    public void Arrive()
    {
        if (State != PlayerState.Flying &&
            State != PlayerState.Landed)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        State = PlayerState.Arrived;

        Debug.Log("Player Arrived");
    }

    public void ResetReady()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.constraints = originalConstraints;

        State = PlayerState.Ready;
    }

    public void Die()
    {
        if (State == PlayerState.Dead)
        {
            return;
        }

        State = PlayerState.Dead;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Debug.Log("Player Dead");
    }

    public bool CanAim()
    {
        return State == PlayerState.Ready ||
               State == PlayerState.Landed;
    }

    public bool IsFlying()
    {
        return State == PlayerState.Flying;
    }
}