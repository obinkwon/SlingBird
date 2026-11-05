using UnityEngine;

/// <summary>
/// Y자 새총 프리팹에 붙는 컴포넌트.
/// 프롱(가지) 위치, 주머니(pouch) 기준점, 좌우 밴드 스프라이트를 들고 있으며
/// PlayerLauncher가 이 컴포넌트를 통해 밴드를 갱신/숨김 처리한다.
/// 스테이지마다 새 새총이 스폰되므로, 이 컴포넌트는 매번 새 인스턴스로 생성된다.
///
/// 밴드 스프라이트 조건: 세로로 그려진 선 이미지(위쪽이 진행 방향), Pivot: Bottom
/// 길이는 스프라이트의 실제 세로 크기를 기준으로 자동 보정되므로 PPU를 맞출 필요 없음.
/// </summary>
public class SlingshotVisual : MonoBehaviour
{
    [Header("기준점")]
    [SerializeField] private Transform pouchAnchor;      // 새총 가운데(주머니) - 대기 시 플레이어 위치, 힘 계산 기준
    [SerializeField] private Transform leftProngAnchor;  // 왼쪽 가지 끝
    [SerializeField] private Transform rightProngAnchor; // 오른쪽 가지 끝

    [Header("밴드(선 이미지) - 세로형, Pivot: Bottom")]
    [SerializeField] private Transform leftBandSprite;
    [SerializeField] private Transform rightBandSprite;
    [Tooltip("세로로 그린 스프라이트(위쪽이 진행 방향)는 -90 그대로 사용")]
    [SerializeField] private float bandAngleOffset = -90f;

    [Header("고무줄 느낌")]
    [Tooltip("가장 많이 당겼을 때의 굵기 비율")]
    [Range(0.2f, 1f)]
    [SerializeField] private float minThicknessRatio = 0.55f;
    [Tooltip("이 길이까지 늘어나면 굵기가 가장 얇아진다 (월드 단위)")]
    [SerializeField] private float fullStretchDistance = 2.8f;
    [Tooltip("발사 후 밴드가 튕기다 멈출 때까지의 시간(초)")]
    [SerializeField] private float recoilDuration = 0.35f;
    [Tooltip("튕기는 동안 왕복하는 횟수")]
    [SerializeField] private float recoilBounces = 2.5f;

    private Vector2 recoilOffset;
    private float recoilElapsed = float.MaxValue; // 발사 전에는 튕김 없이 주머니 위치

    private Vector3 leftBandBaseScale = Vector3.one;
    private Vector3 rightBandBaseScale = Vector3.one;
    private float leftBandLength = 1f;   // 스프라이트 원본 세로 길이(월드 단위)
    private float rightBandLength = 1f;

    public Transform PouchAnchor => pouchAnchor != null ? pouchAnchor : transform;

    private void Awake()
    {
        CacheBand(leftBandSprite, out leftBandBaseScale, out leftBandLength);
        CacheBand(rightBandSprite, out rightBandBaseScale, out rightBandLength);

        // PlayerLauncher가 첫 UpdateBands를 호출하기 전에 엉뚱한 위치로 보이지 않도록 숨김
        HideBands();
    }

    private static void CacheBand(Transform band, out Vector3 baseScale, out float length)
    {
        baseScale = Vector3.one;
        length = 1f;

        if (band == null) return;

        baseScale = band.localScale;

        SpriteRenderer sr = band.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            length = Mathf.Max(0.0001f, sr.sprite.bounds.size.y);
    }

    /// <summary>
    /// 좌우 밴드를 각 프롱 -> targetPos 방향/거리로 직선 갱신한다.
    /// </summary>
    public void UpdateBands(Vector2 targetPos)
    {
        UpdateSingleBand(leftBandSprite, leftBandBaseScale, leftBandLength, leftProngAnchor, targetPos);
        UpdateSingleBand(rightBandSprite, rightBandBaseScale, rightBandLength, rightProngAnchor, targetPos);
    }

    private void UpdateSingleBand(
        Transform band, Vector3 baseScale, float spriteLength,
        Transform prong, Vector2 targetPos)
    {
        if (band == null || prong == null) return;

        if (!band.gameObject.activeSelf)
            band.gameObject.SetActive(true);

        Vector2 origin = prong.position;
        Vector2 dir = targetPos - origin;
        float distance = dir.magnitude;

        band.position = origin;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + bandAngleOffset;
        band.rotation = Quaternion.Euler(0f, 0f, angle);

        // 많이 늘어날수록 얇아진다
        float thickness = Mathf.Lerp(1f, minThicknessRatio, Mathf.InverseLerp(0f, fullStretchDistance, distance));

        // 스프라이트 원본 길이로 나눠서, 이미지 크기/PPU와 무관하게 정확히 distance만큼 늘림
        band.localScale = new Vector3(baseScale.x * thickness, distance / spriteLength, baseScale.z);
    }

    /// <summary>발사 순간 호출. 이후 UpdateReleasedBands가 놓은 위치부터 튕기는 동작을 재생한다.</summary>
    public void ReleaseBands(Vector2 releasePos)
    {
        recoilOffset = releasePos - (Vector2)PouchAnchor.position;
        recoilElapsed = 0f;
    }

    /// <summary>비행 중 매 프레임 호출. 밴드가 주머니 쪽으로 튕겼다가 잦아들어 멈춘다.</summary>
    public void UpdateReleasedBands()
    {
        recoilElapsed += Time.deltaTime;

        float t = Mathf.Clamp01(recoilElapsed / Mathf.Max(0.0001f, recoilDuration));
        float spring = (1f - t) * (1f - t) * Mathf.Cos(t * recoilBounces * 2f * Mathf.PI);

        UpdateBands((Vector2)PouchAnchor.position + recoilOffset * spring);
    }

    public void HideBands()
    {
        if (leftBandSprite != null) leftBandSprite.gameObject.SetActive(false);
        if (rightBandSprite != null) rightBandSprite.gameObject.SetActive(false);
    }
}