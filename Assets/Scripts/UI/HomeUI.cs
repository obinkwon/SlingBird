using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeUI : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeSettingsButton;

    [Header("Layout")]
    [Tooltip("세 버튼 공통 크기")]
    [SerializeField] private Vector2 buttonSize = new Vector2(300f, 110f);
    [Tooltip("버튼 위치 (앵커 기준 anchoredPosition)")]
    [SerializeField] private Vector2 startButtonPosition = new Vector2(0f, 0f);
    [SerializeField] private Vector2 settingsButtonPosition = new Vector2(0f, -140f);
    [SerializeField] private Vector2 quitButtonPosition = new Vector2(0f, -280f);

    private void Start()
    {
        ApplyLayout();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    private void OnDestroy()
    {
        if (startButton != null) startButton.onClick.RemoveListener(StartGame);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
        if (closeSettingsButton != null) closeSettingsButton.onClick.RemoveListener(CloseSettings);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitGame);
    }

    // 인스펙터 우클릭 메뉴로 플레이 없이도 배치를 미리 볼 수 있다
    [ContextMenu("Apply Layout")]
    private void ApplyLayout()
    {
        Place(startButton, startButtonPosition);
        Place(settingsButton, settingsButtonPosition);
        Place(quitButton, quitButtonPosition);
    }

    private void Place(Button button, Vector2 position)
    {
        if (button == null) return;

        // 프리팹 인스턴스 루트의 크기는 프리팹을 따라가지 않으므로 여기서 통일한다
        var rect = (RectTransform)button.transform;
        rect.sizeDelta = buttonSize;
        rect.anchoredPosition = position;
    }

    private void StartGame() => SceneManager.LoadScene(gameSceneName);

    private void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    private void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
