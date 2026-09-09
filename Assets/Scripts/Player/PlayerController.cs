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

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        originalConstraints = rb.constraints;

        State = PlayerState.Ready;
    }

    // --------------------------------------------------
    // 시작 위치 설정
    // --------------------------------------------------

    public void SetStartPosition(Vector2 position)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.constraints = originalConstraints;

        rb.position = position;
        transform.position = position;

        Physics2D.SyncTransforms();

        State = PlayerState.Ready;
    }

    // --------------------------------------------------
    // 당기기 시작
    // --------------------------------------------------

    public void StartAiming()
    {
        if (State != PlayerState.Ready &&
            State != PlayerState.Landed)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.constraints =
            originalConstraints |
            RigidbodyConstraints2D.FreezePosition |
            RigidbodyConstraints2D.FreezeRotation;

        State = PlayerState.Aiming;

        Debug.Log("Player Aiming");
    }

    // --------------------------------------------------
    // 발사
    // --------------------------------------------------

    public void Launch(Vector2 velocity)
    {
        if (State != PlayerState.Aiming)
        {
            Debug.LogWarning(
                $"Launch rejected. Current State: {State}"
            );

            return;
        }

        // 물리 고정 해제
        rb.constraints = originalConstraints;

        rb.angularVelocity = 0f;

        // 최대 속도 제한
        velocity =
            Vector2.ClampMagnitude(
                velocity,
                maxSpeed
            );

        // 발사 속도 적용
        rb.linearVelocity = velocity;

        State = PlayerState.Flying;

        Debug.Log(
            $"Player Launched | Velocity: {rb.linearVelocity}"
        );
    }

    // --------------------------------------------------
    // Platform 착지
    // --------------------------------------------------

    public void Land()
    {
        if (State != PlayerState.Flying)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.constraints = originalConstraints;

        State = PlayerState.Landed;

        Debug.Log("Player Landed");
    }

    // --------------------------------------------------
    // Goal 진입
    //
    // Goal은 도착해서 즉시 멈추는 지점이 아니다.
    // StageManager가 다음 Platform을 만든 뒤
    // MoveToPlatform()을 호출한다.
    //
    // 따라서 여기서는 velocity를 0으로 만들지 않는다.
    // --------------------------------------------------

    public void ReachGoal()
    {
        if (State != PlayerState.Flying)
        {
            return;
        }

        Debug.Log(
            $"Goal Reached | Current Velocity: {rb.linearVelocity}"
        );

        // 여기서는 속도를 건드리지 않는다.
        // StageManager가 다음 위치를 결정한 뒤
        // MoveToPlatform()을 호출한다.
    }

    // --------------------------------------------------
    // 다음 Platform 위치로 이동
    // --------------------------------------------------

    public void MoveToPlatform(Vector2 position)
    {
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

    // --------------------------------------------------
    // Ready 상태로 초기화
    // --------------------------------------------------

    public void ResetReady()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.constraints = originalConstraints;

        State = PlayerState.Ready;

        Debug.Log("Player Reset Ready");
    }

    // --------------------------------------------------
    // 사망
    // --------------------------------------------------

    public void Die()
    {
        if (State == PlayerState.Dead)
        {
            return;
        }

        State = PlayerState.Dead;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.constraints = originalConstraints;

        Debug.Log("Player Dead");
    }

    // --------------------------------------------------
    // 현재 조준 가능 여부
    // --------------------------------------------------

    public bool CanAim()
    {
        return State == PlayerState.Ready ||
               State == PlayerState.Landed;
    }

    // --------------------------------------------------
    // 현재 비행 중인지
    // --------------------------------------------------

    public bool IsFlying()
    {
        return State == PlayerState.Flying;
    }

    // --------------------------------------------------
    // 현재 착지 상태인지
    // --------------------------------------------------

    public bool IsLanded()
    {
        return State == PlayerState.Landed;
    }
}
