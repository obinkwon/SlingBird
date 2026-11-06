using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerSkinApplier : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerSkinData defaultSkin;

    private PlayerController playerController;
    private PlayerWingFlap wingFlap;
    private PlayerSkinData currentSkin;

    public bool HasSkin => currentSkin != null;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        wingFlap = GetComponent<PlayerWingFlap>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            Debug.LogError("PlayerSkinApplier: SpriteRenderer가 없습니다.");
    }

    private void OnEnable()
    {
        playerController.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        playerController.OnStateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        // SkinManager가 씬에 있으면 저장된 선택 스킨을, 없으면 defaultSkin을 사용
        PlayerSkinData initialSkin = SkinManager.Instance != null
            ? SkinManager.Instance.GetSelectedSkin()
            : defaultSkin;

        ApplySkin(initialSkin);
    }

    /// <summary>상점/선택 UI 등 외부에서 스킨을 바꿀 때 호출</summary>
    public void ApplySkin(PlayerSkinData skin)
    {
        if (skin == null)
            skin = defaultSkin;

        currentSkin = skin;

        // 스킨의 날갯짓 프레임으로 교체. 비어 있으면 날갯짓 없이 flyingSprite만 보인다.
        // (스킨이 없을 때는 PlayerWingFlap의 기본 프레임을 그대로 쓴다)
        if (wingFlap != null && skin != null)
            wingFlap.SetFrames(skin.flyingFrames);

        Refresh();
    }

    /// <summary>현재 상태에 맞는 스킨 스프라이트로 다시 맞춘다 (날갯짓이 끝났을 때 등)</summary>
    public void Refresh()
    {
        UpdateSprite(playerController.State);
    }

    private void HandleStateChanged(PlayerController.PlayerState newState)
    {
        UpdateSprite(newState);
    }

    private void UpdateSprite(PlayerController.PlayerState state)
    {
        if (currentSkin == null || spriteRenderer == null)
            return;

        Sprite sprite = currentSkin.GetSpriteForState(state);
        if (sprite != null)
            spriteRenderer.sprite = sprite;
    }
}