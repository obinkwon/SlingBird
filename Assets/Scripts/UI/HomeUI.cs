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

    private void Start()
    {
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
