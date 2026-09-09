using UnityEngine;

public class Platform : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] protected bool canLand = true;

    [Header("Landing")]
    [SerializeField] private float minimumLandingNormalY = 0.5f;

    public bool CanLand => canLand;

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canLand)
            return;

        PlayerController player =
            collision.gameObject.GetComponent<PlayerController>();

        if (player == null)
            return;

        // 날아가는 중이 아니면 착지 처리하지 않음
        if (!player.IsFlying())
            return;

        if (collision.contactCount == 0)
            return;

        // 여러 접점 중 하나라도 위쪽을 향하고 있으면 착지
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact =
                collision.GetContact(i);

            if (contact.normal.y >= minimumLandingNormalY)
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