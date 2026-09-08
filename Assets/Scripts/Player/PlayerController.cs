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

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        State = PlayerState.Ready;
    }

    public void StartAiming()
    {
        if (State != PlayerState.Ready &&
            State != PlayerState.Landed)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        State = PlayerState.Aiming;
    }

    public void Launch(Vector2 velocity)
    {
        if (State != PlayerState.Aiming)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.linearVelocity =
            Vector2.ClampMagnitude(
                velocity,
                maxSpeed
            );

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