using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카메라의 상하좌우 이동에 맞춰 배경 타일을 격자로 재활용하는 무한 스크롤 배경.
/// 원본 오브젝트에만 이 스크립트를 붙이면 됩니다.
/// (가로모드 고정 / 캐릭터가 사방으로 이동하는 게임용)
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class InfiniteBackground : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("비워두면 Main Camera를 자동으로 사용합니다.")]
    [SerializeField] private Camera targetCamera;

    [Header("Scroll Axis")]
    [SerializeField] private bool scrollHorizontal = true;
    [SerializeField] private bool scrollVertical = true;

    [Header("Background")]
    [Tooltip("화면을 채우는 데 필요한 개수 외에 여유분으로 더 만들 타일 수 (가로/세로 각각)")]
    [SerializeField] private int extraTiles = 1;

    [Tooltip("타일 경계의 미세한 틈(seam)을 없애기 위한 겹침 값")]
    [SerializeField] private float overlap = 0.01f;

    private Transform cameraTransform;
    private SpriteRenderer sourceRenderer;

    private Vector3 originPosition;

    private float stepX;            // 타일 간 가로 간격 (폭 - 겹침)
    private float stepY;            // 타일 간 세로 간격 (높이 - 겹침)

    private int columns = 1;
    private int rows = 1;

    private float recycleDistanceX; // 가로 한 바퀴 거리
    private float recycleDistanceY; // 세로 한 바퀴 거리

    private readonly List<Transform> tiles = new List<Transform>();

    private void Start()
    {
        sourceRenderer = GetComponent<SpriteRenderer>();

        // Main Camera 자동 연결
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (sourceRenderer == null || sourceRenderer.sprite == null)
        {
            Debug.LogError($"{gameObject.name}: Sprite가 설정된 SpriteRenderer가 필요합니다.", this);
            enabled = false;
            return;
        }

        if (targetCamera == null)
        {
            Debug.LogError($"{gameObject.name}: Main Camera를 찾을 수 없습니다.", this);
            enabled = false;
            return;
        }

        cameraTransform = targetCamera.transform;

        // 원본이 놓인 위치를 격자의 기준으로 삼는다 (z 유지)
        originPosition = transform.position;

        Vector2 size = sourceRenderer.bounds.size;

        if (size.x <= 0.0001f || size.y <= 0.0001f)
        {
            Debug.LogError($"{gameObject.name}: 배경 크기가 0입니다. Scale 또는 Sprite를 확인하세요.", this);
            enabled = false;
            return;
        }

        float gap = Mathf.Max(0f, overlap);

        stepX = Mathf.Max(0.0001f, size.x - gap);
        stepY = Mathf.Max(0.0001f, size.y - gap);

        CreateTiles();
    }

    // --------------------------------
    // 타일 생성 / 배치
    // --------------------------------
    private void CreateTiles()
    {
        GetViewSize(out float viewWidth, out float viewHeight);

        columns = scrollHorizontal ? CalculateCount(viewWidth, stepX) : 1;
        rows = scrollVertical ? CalculateCount(viewHeight, stepY) : 1;

        recycleDistanceX = stepX * columns;
        recycleDistanceY = stepY * rows;

        int total = columns * rows;

        tiles.Clear();
        tiles.Add(transform); // 원본

        for (int i = 1; i < total; i++)
        {
            tiles.Add(CreateTile(i));
        }

        // 원본 격자를 유지한 채 카메라 위치에 맞춰 정렬
        Vector3 cameraPosition = cameraTransform.position;

        float snappedX = Snap(cameraPosition.x, originPosition.x, stepX, scrollHorizontal);
        float snappedY = Snap(cameraPosition.y, originPosition.y, stepY, scrollVertical);

        int halfC = columns / 2;
        int halfR = rows / 2;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                tiles[r * columns + c].position = new Vector3(
                    snappedX + stepX * (c - halfC),
                    snappedY + stepY * (r - halfR),
                    originPosition.z
                );
            }
        }
    }

    private float Snap(float cameraValue, float originValue, float step, bool axisEnabled)
    {
        if (!axisEnabled)
        {
            return originValue;
        }

        return originValue + Mathf.Round((cameraValue - originValue) / step) * step;
    }

    /// <summary>
    /// 카메라가 보는 범위를 계산. 가로 크기는 aspect(가로모드 기준)로 결정된다.
    /// </summary>
    private void GetViewSize(out float width, out float height)
    {
        if (targetCamera.orthographic)
        {
            height = targetCamera.orthographicSize * 2f;
        }
        else
        {
            float distance = Mathf.Abs(originPosition.z - cameraTransform.position.z);

            height = 2f * distance *
                Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        }

        width = height * targetCamera.aspect;
    }

    private int CalculateCount(float viewSize, float step)
    {
        int count = Mathf.CeilToInt(viewSize / step) + 2 + Mathf.Max(0, extraTiles);

        return Mathf.Max(3, count);
    }

    /// <summary>
    /// 원본을 통째로 복제하지 않고 SpriteRenderer만 가진 타일을 새로 만든다.
    /// (콜라이더나 다른 스크립트가 함께 복제되는 문제 방지)
    /// </summary>
    private Transform CreateTile(int index)
    {
        GameObject tile = new GameObject($"{gameObject.name}_Tile_{index}");

        tile.transform.SetParent(transform.parent, false);
        tile.transform.rotation = transform.rotation;
        tile.transform.localScale = transform.localScale;
        tile.layer = gameObject.layer;

        SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();

        renderer.sprite = sourceRenderer.sprite;
        renderer.color = sourceRenderer.color;
        renderer.sharedMaterial = sourceRenderer.sharedMaterial;
        renderer.sortingLayerID = sourceRenderer.sortingLayerID;
        renderer.sortingOrder = sourceRenderer.sortingOrder;
        renderer.flipX = sourceRenderer.flipX;
        renderer.flipY = sourceRenderer.flipY;
        renderer.maskInteraction = sourceRenderer.maskInteraction;
        renderer.drawMode = sourceRenderer.drawMode;

        if (sourceRenderer.drawMode != SpriteDrawMode.Simple)
        {
            renderer.size = sourceRenderer.size;
        }

        return tile.transform;
    }

    // --------------------------------
    // 재활용
    // --------------------------------
    private void LateUpdate()
    {
        if (cameraTransform == null) return;
        if (tiles.Count == 0) return;

        UpdateTiles();
    }

    private void UpdateTiles()
    {
        Vector3 cameraPosition = cameraTransform.position;

        float halfX = recycleDistanceX * 0.5f;
        float halfY = recycleDistanceY * 0.5f;

        foreach (Transform tile in tiles)
        {
            if (tile == null) continue;

            Vector3 position = tile.position;

            // 가로 재활용
            if (scrollHorizontal)
            {
                float diffX = position.x - cameraPosition.x;

                // while: 카메라가 순간이동해도 한 번에 따라붙도록
                while (diffX < -halfX)
                {
                    position.x += recycleDistanceX;
                    diffX += recycleDistanceX;
                }

                while (diffX > halfX)
                {
                    position.x -= recycleDistanceX;
                    diffX -= recycleDistanceX;
                }
            }

            // 세로 재활용
            if (scrollVertical)
            {
                float diffY = position.y - cameraPosition.y;

                while (diffY < -halfY)
                {
                    position.y += recycleDistanceY;
                    diffY += recycleDistanceY;
                }

                while (diffY > halfY)
                {
                    position.y -= recycleDistanceY;
                    diffY -= recycleDistanceY;
                }
            }

            tile.position = position;
        }
    }
}
