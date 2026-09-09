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

        // 기존 속도 제거
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 당기는 동안 플레이어 위치 고정
        rb.constraints =
            originalConstraints |
            RigidbodyConstraints2D.FreezePosition |
            RigidbodyConstraints2D.FreezeRotation;

        State = PlayerState.Aiming;
    }

    // --------------------------------------------------
    // 발사
    // --------------------------------------------------

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

        Debug.Log("Player Launched");
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

        // 정상적인 물리 상태 유지
        rb.constraints = originalConstraints;

        State = PlayerState.Landed;

        Debug.Log("Player Landed");
    }

    // --------------------------------------------------
    // Goal 진입
    //
    // 중요:
    // Goal은 도착해서 멈추는 지점이 아니다.
    // 다음 발사를 위한 새로운 위치를 만드는 트리거다.
    //
    // 따라서 여기서는 플레이어를 얼리지 않는다.
    // StageManager가 새로운 Platform을 만든 뒤
    // 필요한 위치에서 Land()를 호출한다.
    // --------------------------------------------------

    public void ReachGoal()
    {
        if (State != PlayerState.Flying)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.constraints = originalConstraints;

        Debug.Log("Goal Reached");
    }

    // --------------------------------------------------
    // 다음 Platform 위치로 이동
    //
    // StageManager가 새로운 Platform을 만든 뒤
    // 플레이어를 해당 위치로 이동시키기 위해 사용.
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

        Debug.Log("Player moved to new platform");
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
