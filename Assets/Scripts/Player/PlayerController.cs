using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Ready,
        Flying,
        Landed,
        Arrived,
        Dead
    }

    [Header("Player")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 15f;

    public PlayerState State { get; private set; }

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        State = PlayerState.Ready;
    }

    public void Launch(Vector2 velocity)
    {
        if (State != PlayerState.Ready)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.linearVelocity = Vector2.ClampMagnitude(
            velocity,
            maxSpeed
        );

        State = PlayerState.Flying;
    }

    public void Arrive()
    {
        if (State != PlayerState.Flying)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        State = PlayerState.Arrived;
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
            return;

        State = PlayerState.Dead;
    }

    public bool IsMoving()
    {
        return rb.linearVelocity.sqrMagnitude > 0.1f;
    }
}