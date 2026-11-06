using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 홈 화면 스킨 상점 패널. SkinManager의 스킨마다 버튼 한 줄을 만든다.
/// 이 컴포넌트는 상점 패널 루트에 붙이고, HomeUI가 패널을 켜고 끈다.
/// </summary>
// ponytail: 스크롤이 없다. 스킨이 4~5개를 넘으면 content를 ScrollRect 안으로 옮길 것.
public class ShopUI : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Text coinText;
    [Tooltip("스킨 버튼들이 들어갈 부모 (VerticalLayoutGroup 권장)")]
    [SerializeField] private Transform content;
    [Tooltip("복제해서 쓸 스킨 버튼 (비활성 상태로 둔다). 자식 Text에 라벨이 들어간다")]
    [SerializeField] private Button rowTemplate;
    [Tooltip("스킨이 하나도 없을 때 보여줄 문구 (선택)")]
    [SerializeField] private GameObject emptyLabel;

    private readonly List<Button> rows = new List<Button>();

    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (rowTemplate != null) rowTemplate.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Open() => gameObject.SetActive(true);

    public void Close() => gameObject.SetActive(false);

    private void Refresh()
    {
        if (coinText != null)
            coinText.text = $"COIN : {CoinWallet.Coins}";

        IReadOnlyList<PlayerSkinData> skins = SkinManager.Instance.AllSkins;

        if (emptyLabel != null)
            emptyLabel.SetActive(skins.Count == 0);

        if (rowTemplate == null || content == null)
            return;

        // 스킨 수만큼 버튼을 맞춘다 (모자라면 복제, 남으면 숨김)
        while (rows.Count < skins.Count)
            rows.Add(Instantiate(rowTemplate, content));

        for (int i = 0; i < rows.Count; i++)
        {
            bool used = i < skins.Count;
            rows[i].gameObject.SetActive(used);

            if (used)
                SetupRow(rows[i], skins[i]);
        }
    }

    private void SetupRow(Button row, PlayerSkinData skin)
    {
        SkinManager manager = SkinManager.Instance;

        // 기존 UI처럼 영문 라벨 (레거시 폰트에 한글 글리프가 없을 수 있음)
        string state = manager.IsSelected(skin) ? "EQUIPPED"
            : manager.IsUnlocked(skin) ? "SELECT"
            : $"{skin.price} COIN";

        Text label = row.GetComponentInChildren<Text>(true);
        if (label != null)
            label.text = $"{skin.skinName}  {state}";

        // 구매할 코인이 모자라면 누를 수 없게
        row.interactable = manager.IsUnlocked(skin) || CoinWallet.Coins >= skin.price;

        row.onClick.RemoveAllListeners();
        row.onClick.AddListener(() => OnRowClicked(skin));
    }

    private void OnRowClicked(PlayerSkinData skin)
    {
        SkinManager manager = SkinManager.Instance;

        if (!manager.IsUnlocked(skin) && !manager.TryBuy(skin))
            return;

        manager.SelectSkin(skin);
        Refresh();
    }
}
