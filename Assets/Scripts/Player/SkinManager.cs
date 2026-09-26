using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }

    [SerializeField] private List<PlayerSkinData> allSkins = new();

    private const string SelectedSkinKey = "SelectedSkinId";
    private const string UnlockedSkinsKey = "UnlockedSkinIds"; // comma-separated ids

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IReadOnlyList<PlayerSkinData> AllSkins => allSkins;

    public PlayerSkinData GetSelectedSkin()
    {
        string savedId = PlayerPrefs.GetString(SelectedSkinKey, "");
        PlayerSkinData found = allSkins.FirstOrDefault(s => s.skinId == savedId);
        return found != null ? found : allSkins.FirstOrDefault();
    }

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