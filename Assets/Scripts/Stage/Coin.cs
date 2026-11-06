using UnityEngine;

/// <summary>
/// 비행 중인 플레이어가 지나가면 획득되는 코인 (트리거).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    [SerializeField] private int value = 1;

    private bool collected;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected)
            return;

        PlayerController player = collision.GetComponentInParent<PlayerController>();

        if (player == null || !player.IsFlying())
            return;

        collected = true;
        CoinWallet.Add(value);
        AudioManager.PlayCoin();
        Destroy(gameObject);
    }
}
