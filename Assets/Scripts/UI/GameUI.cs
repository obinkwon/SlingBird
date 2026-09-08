using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StageManager stageManager;
    [SerializeField] private PlayerController player;

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Button restartButton;

    private void Start()
    {
        gameOverPanel.SetActive(false);

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    private void Update()
    {
        if (stageManager != null && scoreText != null)
        {
            scoreText.text =
                "SCORE : " + stageManager.GetScore();
        }

        if (player != null &&
            player.State == PlayerController.PlayerState.Dead)
        {
            ShowGameOver();
        }
    }

    private void ShowGameOver()
    {
        if (gameOverPanel.activeSelf)
            return;

        gameOverPanel.SetActive(true);

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "SCORE : " + stageManager.GetScore();
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}