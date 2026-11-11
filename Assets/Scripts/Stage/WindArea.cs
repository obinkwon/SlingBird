using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 가로로 긴 띠(한 줄) 안에서만 좌우 바람이 분다. 여러 줄이 동시에 있을 수 있다.
/// 바람 줄기 스프라이트가 띠 안 여기저기서 생겼다 흘러가며 사라지는 이펙트로 보여준다.
/// (UnityEngine.WindZone과 이름이 겹치지 않도록 WindArea로 이름 지음)
/// </summary>
public class WindArea : MonoBehaviour
{
    // 현재 존재하는 바람 줄 전체 (StageManager가 스테이지마다 교체)
    private static readonly List<WindArea> all = new List<WindArea>();

    private Rect area;
    private float wind;
    private Rigidbody2D playerRb;
    private PlayerController player;

    private Transform[] streaks;
    private SpriteRenderer[] streakRenderers;
    private float[] streakSpeeds;
    private float[] streakAge;
    private float[] streakLife;
    private float[] streakBaseY;
    private float[] streakPhase;

    /// <summary>pos에서 받는 바람 가속도 합 (바람 줄 밖이면 0). 궤적 미리보기/도달 검사도 같은 값을 쓴다.</summary>
    public static Vector2 AccelAt(Vector2 pos)
    {
        float sum = 0f;

        foreach (WindArea w in all)
        {
            if (w != null && w.area.Contains(pos))
                sum += w.wind;
        }

        return new Vector2(sum, 0f);
    }

    /// <summary>
    /// 모든 바람 줄 삭제. Destroy는 프레임 끝에 처리되므로 목록에서는 즉시 빼서
    /// 같은 프레임의 다음 스테이지 도달 검사에 옛 바람이 섞이지 않게 한다.
    /// </summary>
    public static void DestroyAll()
    {
        foreach (WindArea w in all)
        {
            if (w != null)
                Destroy(w.gameObject);
        }

        all.Clear();
    }

    public void Init(Rect windArea, float windAccel, Sprite streakSprite, PlayerController target, int streakCount)
    {
        area = windArea;
        wind = windAccel;
        player = target;
        playerRb = target != null ? target.GetComponent<Rigidbody2D>() : null;
        all.Add(this);

        if (streakSprite == null)
            return;

        streaks = new Transform[streakCount];
        streakRenderers = new SpriteRenderer[streakCount];
        streakSpeeds = new float[streakCount];
        streakAge = new float[streakCount];
        streakLife = new float[streakCount];
        streakBaseY = new float[streakCount];
        streakPhase = new float[streakCount];

        for (int i = 0; i < streakCount; i++)
        {
            GameObject go = new GameObject($"WindStreak_{i}");
            go.transform.SetParent(transform, false);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = streakSprite;
            sr.flipX = wind < 0f;
            sr.sortingOrder = 2;

            streaks[i] = go.transform;
            streakRenderers[i] = sr;

            Respawn(i);
            // 처음부터 한꺼번에 나타나지 않도록 나이를 흩어 둔다
            streakAge[i] = Random.Range(0f, streakLife[i]);
        }
    }

    // 띠 안의 무작위 위치에서 새 줄기를 시작한다
    private void Respawn(int i)
    {
        streakAge[i] = 0f;
        streakLife[i] = Random.Range(0.7f, 1.4f);
        streakBaseY[i] = Random.Range(area.yMin, area.yMax);
        streakPhase[i] = Random.Range(0f, Mathf.PI * 2f);
        // 바람이 셀수록 줄기도 빨리 흘러간다
        streakSpeeds[i] = Mathf.Abs(wind) * 0.3f * Random.Range(0.8f, 1.2f) + 1.5f;

        streaks[i].localScale = Vector3.one * Random.Range(0.3f, 0.5f);
        streaks[i].position = new Vector2(Random.Range(area.xMin, area.xMax), streakBaseY[i]);
        streakRenderers[i].color = new Color(1f, 1f, 1f, 0f);
    }

    private void Update()
    {
        if (streaks == null)
            return;

        float dir = Mathf.Sign(wind);

        for (int i = 0; i < streaks.Length; i++)
        {
            streakAge[i] += Time.deltaTime;

            Vector3 p = streaks[i].position;
            p.x += dir * streakSpeeds[i] * Time.deltaTime;
            // 일자로 가지 않고 살짝 위아래로 일렁이며 흘러간다
            p.y = streakBaseY[i] + Mathf.Sin(streakAge[i] * 4f + streakPhase[i]) * 0.15f;

            if (streakAge[i] >= streakLife[i] || p.x > area.xMax || p.x < area.xMin)
            {
                Respawn(i);
                continue;
            }

            streaks[i].position = p;

            // 생겨날 때 서서히 나타나고 사라질 때 서서히 흐려진다 + 띠 양 끝에서도 흐려짐
            float lifeAlpha = Mathf.Sin(streakAge[i] / streakLife[i] * Mathf.PI);
            float edgeAlpha = Mathf.Clamp01(Mathf.Min(p.x - area.xMin, area.xMax - p.x) / 0.8f);
            Color c = streakRenderers[i].color;
            c.a = lifeAlpha * edgeAlpha * 0.85f;
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
        all.Remove(this);
    }
}
