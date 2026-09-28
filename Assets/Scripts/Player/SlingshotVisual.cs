using UnityEngine;

/// <summary>
/// Y자 새총 스틱 프리팹에 붙는 컴포넌트.
/// 프롱(가지) 위치, 주머니(pouch) 기준점, 좌우 밴드 스프라이트를 들고 있으며
/// PlayerLauncher가 이 컴포넌트를 통해 밴드를 갱신/숨김 처리한다.
/// 스테이지마다 새 새총이 스폰되므로, 이 컴포넌트는 매번 새 인스턴스로 생성된다.
/// </summary>
public class SlingshotVisual : MonoBehaviour
{
    [Header("기준점")]
    [SerializeField] private Transform pouchAnchor;      // 새총 가운데(주머니) - 대기 시 플레이어 위치, 힘 계산 기준
    [SerializeField] private Transform leftProngAnchor;  // 왼쪽 가지 끝
    [SerializeField] private Transform rightProngAnchor; // 오른쪽 가지 끝

    [Header("밴드(선 이미지) - Pivot: Bottom 권장")]
    [SerializeField] private Transform leftBandSprite;
    [SerializeField] private Transform rightBandSprite;
    [SerializeField] private float bandAngleOffset = -90f; // 스프라이트가 세로면 -90, 가로면 0

    private Vector3 leftBandBaseScale;
    private Vector3 rightBandBaseScale;

    public Transform PouchAnchor => pouchAnchor != null ? pouchAnchor : transform;

    private void Awake()
    {
        if (leftBandSprite != null) leftBandBaseScale = leftBandSprite.localScale;
        if (rightBandSprite != null) rightBandBaseScale = rightBandSprite.localScale;
    }

    /// <summary>
    /// 좌우 밴드를 각 프롱 -> targetPos 방향/거리로 직선 갱신한다.
    /// </summary>
    public void UpdateBands(Vector2 targetPos)
    {
        UpdateSingleBand(leftBandSprite, leftBandBaseScale, leftProngAnchor, targetPos);
        UpdateSingleBand(rightBandSprite, rightBandBaseScale, rightProngAnchor, targetPos);
    }

    private void UpdateSingleBand(Transform band, Vector3 baseScale, Transform prong, Vector2 targetPos)
    {
        if (band == null || prong == null) return;

        band.gameObject.SetActive(true);

        Vector2 origin = prong.position;
        Vector2 dir = targetPos - origin;
        float distance = dir.magnitude;

        band.position = origin;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + bandAngleOffset;
        band.rotation = Quaternion.Euler(0f, 0f, angle);

        band.localScale = new Vector3(baseScale.x, distance, baseScale.z);
    }

    public void HideBands()
    {
        if (leftBandSprite != null) leftBandSprite.gameObject.SetActive(false);
        if (rightBandSprite != null) rightBandSprite.gameObject.SetActive(false);
    }
}
