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

        // 스프라이트 원본 길이로 나눠서, 이미지 크기/PPU와 무관하게 정확히 distance만큼 늘림
        band.localScale = new Vector3(baseScale.x, distance / spriteLength, baseScale.z);
    }

    public void HideBands()
    {
        if (leftBandSprite != null) leftBandSprite.gameObject.SetActive(false);
        if (rightBandSprite != null) rightBandSprite.gameObject.SetActive(false);
    }
}