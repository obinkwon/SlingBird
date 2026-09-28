using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 점수 표시 / 게임오버 패널 / 재시작 버튼 + 게임오버 시 점수 저장.
/// 레거시 UI Text 사용.
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private StageManager stageManager;
    [SerializeField] private PlayerController player;

    [Header("플레이 중 UI")]
    [SerializeField] private Text scoreText;

    [Header("게임오버 UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text bestScoreText;
    [SerializeField] private GameObject newBestLabel;   // "NEW BEST!" 표시용 (선택)
    [SerializeField] private Button restartButton;

    [Header("씬")]
    [Tooltip("재시작 시 이동할 게임 씬 이름. 비워두면 현재 씬을 다시 로드")]
    [SerializeField] private string gameSceneName = "";

    // 게임오버 처리를 한 번만 하기 위한 플래그
    private bool gameOverHandled;

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (newBestLabel != null) newBestLabel.SetActive(false);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);

        gameOverHandled = false;
        RefreshScore();
    }

    private void Update()
    {
        if (gameOverHandled) return;

        RefreshScore();

        // 플레이어가 죽으면 딱 한 번만 게임오버 처리
        if (player.State == PlayerController.PlayerState.Dead)
        {
            HandleGameOver();
        }
    }

    private void RefreshScore()
    {
        if (scoreText != null)
            scoreText.text = stageManager.GetScore().ToString();
    }

    private void HandleGameOver()
    {
        gameOverHandled = true;

        int score = stageManager.GetScore();

        // ★ 점수 저장 (최고 점수 갱신 여부 반환)
        bool isNewBest = ScoreSaver.SaveIfBest(score);

        if (finalScoreText != null) finalScoreText.text = $"SCORE  {score}";
        if (bestScoreText != null) bestScoreText.text = $"BEST  {ScoreSaver.BestScore}";
        if (newBestLabel != null) newBestLabel.SetActive(isNewBest);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    private void Restart()
    {
        // 재시작 시 홈 화면을 건너뛰고 바로 게임 화면으로
        GameFlow.SkipHome = true;

        if (string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else
            SceneManager.LoadScene(gameSceneName);
    }

    // 앱이 백그라운드로 가거나 종료될 때, 게임오버 전이라도 현재 점수를 보존하고 싶다면 사용
    // (원하지 않으면 이 메서드는 삭제해도 됩니다)
    private void OnApplicationPause(bool paused)
    {
        if (paused && !gameOverHandled && stageManager != null)
            ScoreSaver.SaveIfBest(stageManager.GetScore());
    }
}
