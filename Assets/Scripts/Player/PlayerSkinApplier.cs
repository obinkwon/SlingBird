using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerSkinApplier : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerSkinData defaultSkin;

    private PlayerController playerController;
    private PlayerSkinData currentSkin;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

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