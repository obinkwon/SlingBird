using UnityEngine;

public class Platform : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] protected bool canLand = true;

    [Header("Landing")]
    [SerializeField] private float minimumLandingNormalY = 0.5f;

    [Header("Landing Safety")]
    [SerializeField] private float minimumDownwardVelocity = -0.1f;

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

    protected virtual void OnCollisionEnter2D(
        Collision2D collision)
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
        // 현재 출발 플랫폼과 충돌한 경우
        // =====================================================

        if (player.IsCurrentPlatform(this))
        {
            return;
        }

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
        // 아래 방향으로 이동 중인지 확인
        // =====================================================

        if (playerRb.linearVelocity.y >
            minimumDownwardVelocity)
        {
            return;
        }

        // =====================================================
        // Contact Normal 확인
        // =====================================================

        for (int i = 0;
             i < collision.contactCount;
             i++)
        {
            ContactPoint2D contact =
                collision.GetContact(i);

            if (contact.normal.y >=
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