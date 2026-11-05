using UnityEngine;

public class Platform : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] protected bool canLand = true;

    [Header("Landing")]
    [SerializeField] private float minimumLandingNormalY = 0.5f;

    [Header("Landing Safety")]
    [Tooltip("위쪽 속도가 이 값보다 크면 착지로 치지 않는다")]
    [SerializeField] private float maximumLandingVelocityY = 0.1f;

    public bool CanLand => canLand;

    private void Awake()
    {
        // Platform을 정사각형 크기로 설정
        transform.localScale = new Vector3(1f, 1f, 1f);

        // Order in Layer 설정
        SpriteRenderer spriteRenderer =
            GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 1;
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        TryLand(collision);
    }

    // Goal.cs의 OnTriggerStay2D와 같은 이유로 추가.
    // 최초 접촉 프레임(OnCollisionEnter2D)에서 속도/노멀 조건을
    // 살짝 못 만족했다가 다음 물리 프레임에 조건을 만족하게 되는
    // 경우(모서리에 스치듯 닿는 등)를 놓치지 않기 위함.
    private void OnCollisionStay2D(Collision2D collision)
    {
        TryLand(collision);
    }

    protected virtual void TryLand(Collision2D collision)
    {
        if (!canLand)
            return;

        // =====================================================
        // Player 확인
        // =====================================================

        PlayerController player =
            collision.gameObject.GetComponent<PlayerController>();

        if (player == null)
            return;

        // =====================================================
        // Flying 상태인지 확인
        // =====================================================

        if (!player.IsFlying())
            return;

        // =====================================================
        // Collision 확인
        // =====================================================

        if (collision.contactCount == 0)
            return;

        // =====================================================
        // Rigidbody 확인
        // =====================================================

        Rigidbody2D playerRb =
            player.GetComponent<Rigidbody2D>();

        if (playerRb == null)
            return;

        // =====================================================
        // 위로 올라가는 중이 아닌지 확인
        // (충돌 콜백 시점에는 이미 속도가 0에 가까워서 "아래로 이동 중"으로 검사하면 놓친다)
        // =====================================================

        if (playerRb.linearVelocity.y >
            maximumLandingVelocityY)
        {
            return;
        }

        // =====================================================
        // 접촉 방향 확인: 플레이어(원형 콜라이더) 중심이 접촉점보다 위에 있어야 윗면 착지다
        // (contact.normal은 부호가 콜백을 받는 쪽 기준이라 플랫폼에서 보면 아래를 향한다)
        // =====================================================

        for (int i = 0;
             i < collision.contactCount;
             i++)
        {
            ContactPoint2D contact =
                collision.GetContact(i);

            Vector2 fromContact =
                (playerRb.position - contact.point).normalized;

            if (fromContact.y >=
                minimumLandingNormalY)
            {
                OnPlayerLanded(player);

                return;
            }
        }
    }

    protected virtual void OnPlayerLanded(
        PlayerController player)
    {
        player.Land();
    }
}
