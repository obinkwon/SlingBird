using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 스킨 목록 / 선택 / 해금(구매) 관리.
/// 스킨은 Assets/Resources/Skins/ 폴더의 PlayerSkinData 에셋을 자동으로 읽는다.
/// 씬에 배치하지 않아도 Instance에 처음 접근할 때 자동으로 만들어진다.
/// </summary>
public class SkinManager : MonoBehaviour
{
    private static SkinManager instance;

    public static SkinManager Instance
    {
        get
        {
            // AddComponent 안에서 Awake가 instance를 설정한다
            if (instance == null)
                new GameObject("SkinManager").AddComponent<SkinManager>();

            return instance;
        }
    }

    [Tooltip("비워두면 Resources/Skins 폴더에서 자동으로 읽어온다")]
    [SerializeField] private List<PlayerSkinData> allSkins = new();

    private const string SkinsResourcePath = "Skins";
    private const string SelectedSkinKey = "SelectedSkinId";
    private const string UnlockedSkinsKey = "UnlockedSkinIds"; // comma-separated ids

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (allSkins.Count == 0)
            allSkins = Resources.LoadAll<PlayerSkinData>(SkinsResourcePath).ToList();

        // 상점에 싼 것부터 보이도록
        allSkins = allSkins
            .Where(s => s != null)
            .OrderBy(s => s.price)
            .ThenBy(s => s.skinName)
            .ToList();
    }

    public IReadOnlyList<PlayerSkinData> AllSkins => allSkins;

    public PlayerSkinData GetSelectedSkin()
    {
        string savedId = PlayerPrefs.GetString(SelectedSkinKey, "");
        PlayerSkinData found = allSkins.FirstOrDefault(s => s.skinId == savedId && IsUnlocked(s));
        return found != null ? found : allSkins.FirstOrDefault(IsUnlocked);
    }

    public bool IsSelected(PlayerSkinData skin) => skin != null && GetSelectedSkin() == skin;

    public void SelectSkin(PlayerSkinData skin)
    {
        if (skin == null || !IsUnlocked(skin))
            return;

        PlayerPrefs.SetString(SelectedSkinKey, skin.skinId);
        PlayerPrefs.Save();
    }

    public bool IsUnlocked(PlayerSkinData skin)
    {
        if (skin.price <= 0)
            return true;

        return GetUnlockedIds().Contains(skin.skinId);
    }

    /// <summary>코인이 충분하면 차감 후 해금하고 true 반환</summary>
    public bool TryBuy(PlayerSkinData skin)
    {
        if (skin == null || IsUnlocked(skin))
            return false;

        if (!CoinWallet.TrySpend(skin.price))
            return false;

        Unlock(skin);
        return true;
    }

    public void Unlock(PlayerSkinData skin)
    {
        HashSet<string> unlocked = GetUnlockedIds();
        if (unlocked.Add(skin.skinId))
        {
            PlayerPrefs.SetString(UnlockedSkinsKey, string.Join(",", unlocked));
            PlayerPrefs.Save();
        }
    }

    private HashSet<string> GetUnlockedIds()
    {
        string raw = PlayerPrefs.GetString(UnlockedSkinsKey, "");
        return raw.Length == 0
            ? new HashSet<string>()
            : new HashSet<string>(raw.Split(','));
    }
}
