using UnityEngine;

/// <summary>
/// 비행 중에만 날갯짓 스프라이트를 반복 재생하고, 착지/사망하면 원래 스프라이트로 되돌린다.
/// </summary>
// ponytail: 프레임이 스킨과 무관하게 고정이다. 스킨을 실제로 쓰게 되면(PlayerSkinApplier를 씬에 추가) PlayerSkinData로 옮길 것.
[RequireComponent(typeof(PlayerController), typeof(SpriteRenderer))]
public class PlayerWingFlap : MonoBehaviour
{
    [Tooltip("순서대로 반복 재생할 날갯짓 프레임")]
    [SerializeField] private Sprite[] flyingFrames;
    [Tooltip("초당 프레임 수")]
    [SerializeField] private float framesPerSecond = 12f;

    private PlayerController player;
    private SpriteRenderer spriteRenderer;
    private Sprite idleSprite;
    private float elapsed;
    private bool wasFlying;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        idleSprite = spriteRenderer.sprite;
    }

    private void Update()
    {
        if (flyingFrames == null || flyingFrames.Length == 0)
            return;

        if (player.IsFlying())
        {
            // 발사할 때마다 첫 프레임부터 시작
            if (!wasFlying)
                elapsed = 0f;

            wasFlying = true;

            // deltaTime 기준이라 일시정지(timeScale 0) 중에는 날갯짓도 멈춘다
            elapsed += Time.deltaTime;
            int frame = (int)(elapsed * framesPerSecond) % flyingFrames.Length;
            spriteRenderer.sprite = flyingFrames[frame];
        }
        else if (wasFlying)
        {
            wasFlying = false;
            spriteRenderer.sprite = idleSprite;
        }
    }
}
