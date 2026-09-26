using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSkinData", menuName = "Game/Player Skin")]
public class PlayerSkinData : ScriptableObject
{
    [Header("Identity")]
    public string skinId;
    public string skinName;

    [Header("Sprites per State")]
    public Sprite readySprite;
    public Sprite aimingSprite;   // 비워두면 readySprite 사용
    public Sprite flyingSprite;
    public Sprite landedSprite;   // 비워두면 readySprite 사용
    public Sprite deadSprite;

    [Header("Shop (optional)")]
    public int price;
    public Sprite thumbnail;

    public Sprite GetSpriteForState(PlayerController.PlayerState state)
    {
        return state switch
        {
            PlayerController.PlayerState.Ready => readySprite,
            PlayerController.PlayerState.Aiming => aimingSprite != null ? aimingSprite : readySprite,
            PlayerController.PlayerState.Flying => flyingSprite,
            PlayerController.PlayerState.Landed => landedSprite != null ? landedSprite : readySprite,
            PlayerController.PlayerState.Dead => deadSprite,
            _ => readySprite
        };
    }
}