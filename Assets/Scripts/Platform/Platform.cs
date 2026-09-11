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
        // 핵심
        // =====================================================
        // 현재 출발 플랫폼과 충돌한 경우에는
        // 절대로 착지 처리하지 않는다.
        //
        // 예:
        //
        //      Goal
        //        ↓
        //
        //       ↙
        //      Player
        //   ───────────
        //   StartPlatform
        //
        // 플레이어가 아래 방향으로 쏘이면
        // 바로 출발 플랫폼에 다시 부딪힐 수 있다.
        //
        // 이때 Land()를 호출하면 속도가 0이 되어
        // "조금 움직이고 바로 멈추는" 문제가 발생한다.
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
        //
        // 플랫폼 위에 착지하려면 플레이어가 내려오고 있어야 한다.
        //
        // 위로 올라가는 중에 옆면을 스치거나
        // 아래 플랫폼에 부딪힌 경우에는 착지 처리하지 않는다.
        // =====================================================

        if (playerRb.linearVelocity.y >
            minimumDownwardVelocity)
        {
            return;
        }

        // =====================================================
        // Contact Normal 확인
        // =====================================================
        //
        // 플랫폼의 윗면에 충돌했을 때
        // normal.y가 양수 방향으로 나온다.
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