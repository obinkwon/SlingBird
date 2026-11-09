using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 홈 화면 스킨 상점 패널 (캐러셀). 스킨 하나를 크게 보여주고 좌우 버튼/스와이프로 넘긴다.
/// 이 컴포넌트는 상점 패널 루트에 붙이고, HomeUI가 패널을 켜고 끈다.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Text coinText;
    [Tooltip("스킨이 하나도 없을 때 보여줄 문구 (선택)")]
    [SerializeField] private GameObject emptyLabel;

    [Header("Carousel")]
    [SerializeField] private Image previewImage;
    [SerializeField] private Text nameText;
    [Tooltip("구매 / 선택 버튼. 자식 Text에 라벨이 들어간다")]
    [FormerlySerializedAs("rowTemplate")]
    [SerializeField] private Button actionButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    [Tooltip("미리보기 날갯짓 초당 프레임 수")]
    [SerializeField] private float previewFps = 8f;
    [Tooltip("이 픽셀 이상 가로로 끌면 스와이프로 본다")]
    [SerializeField] private float swipeThreshold = 80f;

    private int index;
    private float previewTime;
    private bool pressing;
    private Vector2 pressStart;

    public bool IsOpen => gameObject.activeSelf;

    private IReadOnlyList<PlayerSkinData> Skins => SkinManager.Instance.AllSkins;

    private PlayerSkinData Current => Skins.Count > 0 ? Skins[index] : null;

    private void Awake()
    {
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (actionButton != null) actionButton.onClick.AddListener(OnActionClicked);
        if (prevButton != null) prevButton.onClick.AddListener(() => Move(-1));
        if (nextButton != null) nextButton.onClick.AddListener(() => Move(1));
    }

    private void OnEnable()
    {
        // 열 때는 지금 쓰고 있는 스킨부터 보여준다
        index = 0;
        PlayerSkinData selected = SkinManager.Instance.GetSelectedSkin();
        for (int i = 0; i < Skins.Count; i++)
        {
            if (Skins[i] == selected)
                index = i;
        }

        pressing = false;
        Refresh();
    }

    public void Open() => gameObject.SetActive(true);

    public void Close() => gameObject.SetActive(false);

    private void Update()
    {
        HandleSwipe();
        AnimatePreview();
    }

    private void HandleSwipe()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null)
            return;

        if (pointer.press.wasPressedThisFrame)
        {
            pressing = true;
            pressStart = pointer.position.ReadValue();
        }
        else if (pressing && pointer.press.wasReleasedThisFrame)
        {
            pressing = false;
            float dx = pointer.position.ReadValue().x - pressStart.x;

            // 왼쪽으로 밀면 다음, 오른쪽으로 밀면 이전
            if (Mathf.Abs(dx) >= swipeThreshold)
                Move(dx < 0f ? 1 : -1);
        }
    }

    private void AnimatePreview()
    {
        PlayerSkinData skin = Current;
        if (previewImage == null || skin == null || skin.flyingFrames == null || skin.flyingFrames.Length == 0)
            return;

        // 홈 화면은 timeScale과 무관하게 돌도록 unscaled 사용
        previewTime += Time.unscaledDeltaTime;
        int frame = (int)(previewTime * previewFps) % skin.flyingFrames.Length;
        previewImage.sprite = skin.flyingFrames[frame];
    }

    private void Move(int step)
    {
        if (Skins.Count == 0)
            return;

        // 끝에서 넘기면 반대쪽 끝으로 돈다
        index = (index + step + Skins.Count) % Skins.Count;
        Refresh();
    }

    private void Refresh()
    {
        if (coinText != null)
            coinText.text = $"COIN : {CoinWallet.Coins}";

        PlayerSkinData skin = Current;
        bool hasSkin = skin != null;

        if (emptyLabel != null) emptyLabel.SetActive(!hasSkin);
        if (previewImage != null) previewImage.gameObject.SetActive(hasSkin);
        if (nameText != null) nameText.gameObject.SetActive(hasSkin);
        if (actionButton != null) actionButton.gameObject.SetActive(hasSkin);

        bool canMove = Skins.Count > 1;
        if (prevButton != null) prevButton.gameObject.SetActive(canMove);
        if (nextButton != null) nextButton.gameObject.SetActive(canMove);

        if (!hasSkin)
            return;

        SkinManager manager = SkinManager.Instance;
        bool unlocked = manager.IsUnlocked(skin);

        previewTime = 0f;
        if (previewImage != null)
        {
            previewImage.sprite = skin.thumbnail != null ? skin.thumbnail : skin.readySprite;
            // 아직 안 산 스킨은 어둡게
            previewImage.color = unlocked ? Color.white : new Color(0.25f, 0.25f, 0.25f, 1f);
        }

        if (nameText != null)
            nameText.text = skin.skinName;

        if (actionButton != null)
        {
            // 기존 UI처럼 영문 라벨 (레거시 폰트에 한글 글리프가 없을 수 있음)
            string label = manager.IsSelected(skin) ? "SELECTED"
                : unlocked ? "SELECT"
                : $"BUY {skin.price}";

            Text text = actionButton.GetComponentInChildren<Text>(true);
            if (text != null)
                text.text = label;

            // 이미 장착 중이거나 코인이 모자라면 누를 수 없게
            actionButton.interactable = !manager.IsSelected(skin)
                && (unlocked || CoinWallet.Coins >= skin.price);
        }
    }

    private void OnActionClicked()
    {
        PlayerSkinData skin = Current;
        if (skin == null)
            return;

        SkinManager manager = SkinManager.Instance;

        if (!manager.IsUnlocked(skin) && !manager.TryBuy(skin))
            return;

        manager.SelectSkin(skin);
        Refresh();
    }
}
