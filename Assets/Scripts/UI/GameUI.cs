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

    private bool gameOverShown = false;

    private void Start()
    {
        // Game Over 패널 숨기기
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // 재시작 버튼
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();

        // 플레이어 사망 확인
        if (!gameOverShown &&
            player != null &&
            player.State == PlayerController.PlayerState.Dead)
        {
            ShowGameOver();
        }
    }

    private void UpdateUI()
    {
        if (stageManager == null)
            return;

        // 점수
        if (scoreText != null)
        {
            scoreText.text =
                "SCORE : " + stageManager.GetScore();
        }
    }

    private void ShowGameOver()
    {
        gameOverShown = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null && stageManager != null)
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
