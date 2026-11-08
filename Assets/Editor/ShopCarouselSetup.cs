using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 일회용: Home 씬 상점 패널을 목록형에서 캐러셀 배치로 바꾼다. 실행 후 이 파일은 지워도 된다.
/// </summary>
public static class ShopCarouselSetup
{
    private const string HomeScenePath = "Assets/Scenes/Home.unity";
    private const string ButtonPrefabPath = "Assets/Prefabs/BannerButton/BannerButton.prefab";

    [MenuItem("SlingBird/상점 캐러셀 설정")]
    private static void Setup()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var scene = EditorSceneManager.OpenScene(HomeScenePath);

        ShopUI shop = Object.FindAnyObjectByType<ShopUI>(FindObjectsInactive.Include);
        if (shop == null)
        {
            Debug.LogError("Home 씬에서 ShopUI를 찾지 못했습니다.");
            return;
        }

        var so = new SerializedObject(shop);
        if (so.FindProperty("previewImage").objectReferenceValue != null)
        {
            Debug.Log("상점 캐러셀이 이미 설정되어 있습니다.");
            return;
        }

        var closeButton = (Button)so.FindProperty("closeButton").objectReferenceValue;
        var coinText = (Text)so.FindProperty("coinText").objectReferenceValue;
        var actionButton = (Button)so.FindProperty("actionButton").objectReferenceValue;
        if (closeButton == null || coinText == null || actionButton == null)
        {
            Debug.LogError("ShopUI의 closeButton / coinText / actionButton 연결이 필요합니다.");
            return;
        }

        Transform window = closeButton.transform.parent;

        // 목록용 Content 대신 구매 버튼을 창에 직접 둔다
        Transform oldContent = actionButton.transform.parent;
        actionButton.transform.SetParent(window, false);
        actionButton.gameObject.SetActive(true);
        Place(actionButton.transform, new Vector2(0f, -205f), new Vector2(320f, 90f));
        if (oldContent != window)
            Object.DestroyImmediate(oldContent.gameObject);

        // 미리보기 이미지
        var preview = new GameObject("Preview", typeof(RectTransform), typeof(Image));
        preview.layer = window.gameObject.layer;
        preview.transform.SetParent(window, false);
        Place(preview.transform, new Vector2(0f, -5f), new Vector2(300f, 220f));
        var previewImage = preview.GetComponent<Image>();
        previewImage.preserveAspect = true;
        previewImage.raycastTarget = false;

        // 스킨 이름 (코인 텍스트 스타일 복제)
        var nameObj = Object.Instantiate(coinText.gameObject, window);
        nameObj.name = "SkinNameText";
        Place(nameObj.transform, new Vector2(0f, -130f), new Vector2(560f, 60f));

        // 좌우 버튼
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonPrefabPath);
        Button prev = MakeArrow(prefab, window, "PrevButton", "<", -300f);
        Button next = MakeArrow(prefab, window, "NextButton", ">", 300f);

        so.FindProperty("previewImage").objectReferenceValue = previewImage;
        so.FindProperty("nameText").objectReferenceValue = nameObj.GetComponent<Text>();
        so.FindProperty("prevButton").objectReferenceValue = prev;
        so.FindProperty("nextButton").objectReferenceValue = next;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("상점 캐러셀 설정 완료. Assets/Editor/ShopCarouselSetup.cs 는 지워도 됩니다.");
    }

    private static Button MakeArrow(GameObject prefab, Transform parent, string name, string label, float x)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.name = name;
        go.SetActive(true);
        Place(go.transform, new Vector2(x, -5f), new Vector2(100f, 100f));

        Text text = go.GetComponentInChildren<Text>(true);
        if (text != null)
            text.text = label;

        return go.GetComponent<Button>();
    }

    private static void Place(Transform t, Vector2 pos, Vector2 size)
    {
        var rt = (RectTransform)t;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
