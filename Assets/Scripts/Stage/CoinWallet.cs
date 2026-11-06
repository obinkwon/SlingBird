using UnityEngine;

/// <summary>
/// 보유 코인 저장/사용 전담 정적 클래스 (PlayerPrefs 사용).
/// </summary>
public static class CoinWallet
{
    private const string CoinsKey = "Coins";

    public static int Coins => PlayerPrefs.GetInt(CoinsKey, 0);

    // 앱이 갑자기 꺼져도 주운 코인이 남도록 매번 저장한다
    public static void Add(int amount)
    {
        if (amount <= 0)
            return;

        PlayerPrefs.SetInt(CoinsKey, Coins + amount);
        PlayerPrefs.Save();
    }

    /// <summary>코인이 충분하면 차감하고 true 반환</summary>
    public static bool TrySpend(int amount)
    {
        if (amount < 0 || Coins < amount)
            return false;

        PlayerPrefs.SetInt(CoinsKey, Coins - amount);
        PlayerPrefs.Save();
        return true;
    }
}
