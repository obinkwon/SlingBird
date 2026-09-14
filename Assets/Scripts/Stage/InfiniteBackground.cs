using System.Collections.Generic;
using UnityEngine;

public class InfiniteBackground : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Background")]
    [SerializeField] private int tilesAbove = 2;
    [SerializeField] private int tilesBelow = 2;

    private SpriteRenderer spriteRenderer;
    private float tileHeight;

    private readonly List<Transform> tiles =
        new List<Transform>();

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Main Camera 자동 연결
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }

        // SpriteRenderer 확인
        if (spriteRenderer == null)
        {
            Debug.LogError(
                $"{gameObject.name}: SpriteRenderer가 필요합니다."
            );

            return;
        }

        // Camera 확인
        if (cameraTransform == null)
        {
            Debug.LogError(
                $"{gameObject.name}: Main Camera를 찾을 수 없습니다."
            );

            return;
        }

        // 배경 이미지의 실제 높이
        tileHeight = spriteRenderer.bounds.size.y;

        // 원본은 0,0,0
        transform.position = Vector3.zero;

        CreateTiles();
    }

    private void CreateTiles()
    {
        tiles.Clear();

        // --------------------------------
        // 원본
        // --------------------------------
        tiles.Add(transform);

        // --------------------------------
        // 위쪽 배경 생성
        // --------------------------------
        for (int i = 1; i <= tilesAbove; i++)
        {
            Transform tile = CreateTile();

            tile.position =
                Vector3.up * tileHeight * i;

            tiles.Add(tile);
        }

        // --------------------------------
        // 아래쪽 배경 생성
        // --------------------------------
        for (int i = 1; i <= tilesBelow; i++)
        {
            Transform tile = CreateTile();

            tile.position =
                Vector3.down * tileHeight * i;

            tiles.Add(tile);
        }
    }

    private Transform CreateTile()
    {
        GameObject newTile =
            Instantiate(
                gameObject,
                transform.parent
            );

        newTile.name =
            gameObject.name + "_Tile";

        // 복사본에서는 스크립트 비활성화
        InfiniteBackground background =
            newTile.GetComponent<InfiniteBackground>();

        if (background != null)
        {
            background.enabled = false;
        }

        // 생성 직후 위치는 0,0,0
        newTile.transform.position = Vector3.zero;

        return newTile.transform;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        if (tileHeight <= 0f)
            return;

        UpdateTiles();
    }

    private void UpdateTiles()
    {
        float cameraY =
            cameraTransform.position.y;

        // 카메라보다 충분히 위/아래에
        // 배경이 존재하도록 설정
        float topLimit =
            cameraY + tileHeight * (tilesAbove + 1);

        float bottomLimit =
            cameraY - tileHeight * (tilesBelow + 1);

        // 전체 배경을 한 바퀴 이동시키는 거리
        float recycleDistance =
            tileHeight *
            (tilesAbove + tilesBelow + 1);

        foreach (Transform tile in tiles)
        {
            if (tile == null)
                continue;

            // 아래로 벗어나면 위로 이동
            if (tile.position.y < bottomLimit)
            {
                tile.position +=
                    Vector3.up * recycleDistance;
            }

            // 위로 벗어나면 아래로 이동
            else if (tile.position.y > topLimit)
            {
                tile.position -=
                    Vector3.up * recycleDistance;
            }
        }
    }
}