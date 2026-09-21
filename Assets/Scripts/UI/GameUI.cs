using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StageManager stageManager;
    [SerializeField] private PlayerController player;

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private Button restartButton;

    private bool gameOverShown;
    private int lastScore = -1;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        UpdateScoreText();
    }

    private void OnDestroy()
    {
        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);
    }

    private void Update()
    {
        UpdateScoreText();

        if (!gameOverShown &&
            player != null &&
            player.State == PlayerController.PlayerState.Dead)
        {
            ShowGameOver();
        }
    }

    private void UpdateScoreText()
    {
        if (stageManager == null || scoreText == null)
            return;

        int score = stageManager.GetScore();
        if (score == lastScore)
            return; // 점수가 바뀔 때만 갱신

        lastScore = score;
        scoreText.text = $"SCORE : {score}";
    }

    private void ShowGameOver()
    {
        gameOverShown = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null && stageManager != null)
            finalScoreText.text = $"SCORE : {stageManager.GetScore()}";

        // Time.timeScale = 0f; // 멈출 경우 RestartGame에서 1f로 복구 필수
    }

    private void RestartGame()
    {
        // Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
