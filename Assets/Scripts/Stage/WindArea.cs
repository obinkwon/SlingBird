using UnityEngine;

/// <summary>
/// 이 영역 안에서만 좌우 바람이 분다.
/// 바람 줄기 스프라이트가 영역 안을 흘러가는 이펙트로 위치와 방향을 보여준다.
/// (UnityEngine.WindZone과 이름이 겹치지 않도록 WindArea로 이름 지음)
/// </summary>
public class WindArea : MonoBehaviour
{
    // 한 번에 하나의 바람 영역만 존재한다 (StageManager가 스테이지마다 교체)
    public static WindArea Active { get; private set; }

    private Rect area;
    private float wind;
    private Rigidbody2D playerRb;
    private PlayerController player;

    private Transform[] streaks;
    private SpriteRenderer[] streakRenderers;
    private float[] streakSpeeds;

    /// <summary>pos에서 받는 바람 가속도 (영역 밖이면 0). 궤적 미리보기/도달 검사도 같은 값을 쓴다.</summary>
    public static Vector2 AccelAt(Vector2 pos) =>
        Active != null && Active.area.Contains(pos) ? new Vector2(Active.wind, 0f) : Vector2.zero;

    public void Init(Rect windArea, float windAccel, Sprite streakSprite, PlayerController target, int streakCount)
    {
        area = windArea;
        wind = windAccel;
        player = target;
        playerRb = target != null ? target.GetComponent<Rigidbody2D>() : null;
        Active = this;

        if (streakSprite == null)
            return;

        streaks = new Transform[streakCount];
        streakRenderers = new SpriteRenderer[streakCount];
        streakSpeeds = new float[streakCount];

        for (int i = 0; i < streakCount; i++)
        {
            GameObject go = new GameObject($"WindStreak_{i}");
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * Random.Range(0.4f, 0.6f);
            go.transform.position = new Vector2(
                Random.Range(area.xMin, area.xMax),
                Random.Range(area.yMin, area.yMax));

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = streakSprite;
            sr.flipX = wind < 0f;
            sr.sortingOrder = 2;

            streaks[i] = go.transform;
            streakRenderers[i] = sr;
            // 바람이 셀수록 줄기도 빨리 흘러간다
            streakSpeeds[i] = Mathf.Abs(wind) * Random.Range(0.8f, 1.2f) + 1.5f;
        }
    }

    private void Update()
    {
        if (streaks == null)
            return;

        float dir = Mathf.Sign(wind);

        for (int i = 0; i < streaks.Length; i++)
        {
            Vector3 p = streaks[i].position;
            p.x += dir * streakSpeeds[i] * Time.deltaTime;

            // 바람 방향 끝을 넘으면 반대쪽(바람이 들어오는 쪽)에서 다시 시작
            if (p.x > area.xMax || p.x < area.xMin)
            {
                p.x = dir > 0f ? area.xMin : area.xMax;
                p.y = Random.Range(area.yMin, area.yMax);
            }

            streaks[i].position = p;

            // 영역 양 끝에서 서서히 나타나고 사라지게
            float edge = Mathf.Min(p.x - area.xMin, area.xMax - p.x);
            Color c = streakRenderers[i].color;
            c.a = Mathf.Clamp01(edge / 0.8f) * 0.85f;
            streakRenderers[i].color = c;
        }
    }

    private void FixedUpdate()
    {
        if (player == null || playerRb == null || !player.IsFlying())
            return;

        if (area.Contains(playerRb.position))
            playerRb.linearVelocity += new Vector2(wind * Time.fixedDeltaTime, 0f);
    }

    private void OnDestroy()
    {
        if (Active == this)
            Active = null;
    }
}
