using UnityEngine;

/// <summary>
/// 새총(슬링샷) 막대의 두 갈래(fork)에서 당겨진 위치로 이어지는 줄(밴드)을
/// 그리는 시각효과 전담 컴포넌트. PlayerLauncher가 조준 상태를 알려주면
/// 그에 맞춰 LineRenderer 두 개를 업데이트한다.
/// </summary>
public class SlingshotBand : MonoBehaviour
{
    [Header("Bands")]
    [SerializeField] private LineRenderer leftBand;
    [SerializeField] private LineRenderer rightBand;

    [Header("Fork Offset")]
    [Tooltip("조준 시작 위치 기준 왼쪽 갈래 끝 상대 오프셋")]
    [SerializeField] private Vector2 leftForkOffset = new Vector2(-0.25f, 0.15f);
    [Tooltip("조준 시작 위치 기준 오른쪽 갈래 끝 상대 오프셋")]
    [SerializeField] private Vector2 rightForkOffset = new Vector2(0.25f, 0.15f);

    private Vector2 leftForkAnchor;
    private Vector2 rightForkAnchor;

    private void Awake()
    {
        if (leftBand != null)
            leftBand.sortingOrder = 1;

        if (rightBand != null)
            rightBand.sortingOrder = 1;

        Hide();
    }

    /// <summary>
    /// 조준을 시작한 위치를 기준으로 fork(갈래) anchor를 새로 계산하고 줄을 표시한다.
    /// PlayerLauncher.StartDrag()에서 호출.
    /// </summary>
    public void BeginAim(Vector2 originPosition)
    {
        leftForkAnchor = originPosition + leftForkOffset;
        rightForkAnchor = originPosition + rightForkOffset;

        Show();
        UpdatePull(originPosition);
    }

    /// <summary>
    /// 현재 당겨진(드래그 중인) 위치로 줄 끝을 갱신한다.
    /// PlayerLauncher.UpdateDrag() / DrawAim()에서 호출.
    /// </summary>
    public void UpdatePull(Vector2 pullPosition)
    {
        if (leftBand != null)
        {
            leftBand.positionCount = 2;
            leftBand.SetPosition(0, leftForkAnchor);
            leftBand.SetPosition(1, pullPosition);
        }

        if (rightBand != null)
        {
            rightBand.positionCount = 2;
            rightBand.SetPosition(0, rightForkAnchor);
            rightBand.SetPosition(1, pullPosition);
        }
    }

    public void Show()
    {
        if (leftBand != null)
            leftBand.enabled = true;

        if (rightBand != null)
            rightBand.enabled = true;
    }

    public void Hide()
    {
        if (leftBand != null)
            leftBand.enabled = false;

        if (rightBand != null)
            rightBand.enabled = false;
    }
}
