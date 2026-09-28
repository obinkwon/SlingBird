using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    public void OnClickStart() => SceneManager.LoadScene("Game");
    public void OnClickSettings() => settingsPanel.SetActive(true);
    public void OnClickCloseSettings() => settingsPanel.SetActive(false);

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
