using UnityEngine;

public class StartPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] protected bool canLand = true;

    public bool CanLand => canLand;

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canLand)
            return;

        PlayerController player =
            collision.gameObject.GetComponent<PlayerController>();

        if (player == null)
            return;

        // 플레이어가 플랫폼의 위쪽에서 내려오는 경우에만 착지
        if (collision.contacts.Length == 0)
            return;

        ContactPoint2D contact = collision.contacts[0];

        if (contact.normal.y > 0.5f)
        {
            OnPlayerLanded(player);
        }
    }

    protected virtual void OnPlayerLanded(PlayerController player)
    {
        player.Land();
    }
}