using UnityEngine;
using UnityEngine.InputSystem;
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
    [Tooltip("보유 코인 표시 (선택)")]
    [SerializeField] private Text coinText;

    [Header("게임오버 UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text bestScoreText;
    [SerializeField] private GameObject newBestLabel;   // "NEW BEST!" 표시용 (선택)
    [SerializeField] private Button restartButton;
    [SerializeField] private Button homeButton;

    [Header("일시정지 UI (안드로이드 뒤로가기 / Esc)")]
    [SerializeField] private GameObject pausePanel;
    [Tooltip("우측 상단 설정 버튼. 게임을 멈추고 설정 창을 연다")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseHomeButton;

    [Header("씬")]
    [Tooltip("재시작 시 이동할 게임 씬 이름. 비워두면 현재 씬을 다시 로드")]
    [SerializeField] private string gameSceneName = "";
    [Tooltip("홈 버튼 클릭 시 이동할 홈 씬 이름. 홈이 같은 씬의 패널이면 비워두기")]
    [SerializeField] private string homeSceneName = "";

    // 게임오버 처리를 한 번만 하기 위한 플래그
    private bool gameOverHandled;

    // 점수가 바뀔 때만 문자열을 다시 만들기 위한 캐시
    private int shownScore = -1;
    private int shownCoins = -1;

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (newBestLabel != null) newBestLabel.SetActive(false);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (homeButton != null) homeButton.onClick.AddListener(GoHome);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(Resume);
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (pauseHomeButton != null) pauseHomeButton.onClick.AddListener(GoHome);

        gameOverHandled = false;
        RefreshScore();
    }

    private void OnDestroy()
    {
        // 일시정지 상태로 씬을 떠나도 다음 씬이 멈춰 있지 않도록
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (gameOverHandled) return;

        // 안드로이드 뒤로가기 버튼은 Input System에서 Esc 키로 들어온다
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Time.timeScale == 0f) Resume();
            else Pause();
        }

        RefreshScore();

        // 플레이어가 죽으면 딱 한 번만 게임오버 처리
        if (player.State == PlayerController.PlayerState.Dead)
        {
            HandleGameOver();
        }
    }

    private void RefreshScore()
    {
        int coins = CoinWallet.Coins;
        if (coinText != null && coins != shownCoins)
        {
            shownCoins = coins;
            coinText.text = $"COIN : {coins}";
        }

        int score = stageManager.GetScore();
        if (scoreText == null || score == shownScore) return;

        shownScore = score;
        scoreText.text = $"SCORE : {score}";
    }

    private void HandleGameOver()
    {
        gameOverHandled = true;

        int score = stageManager.GetScore();

        // ★ 점수 저장 (최고 점수 갱신 여부 반환)
        bool isNewBest = ScoreSaver.SaveIfBest(score);

        if (finalScoreText != null) finalScoreText.text = $"SCORE : {score}";
        if (bestScoreText != null) bestScoreText.text = $"BEST : {ScoreSaver.BestScore}";
        if (newBestLabel != null) newBestLabel.SetActive(isNewBest);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    private void Pause()
    {
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    private void OpenSettings()
    {
        Time.timeScale = 0f;
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    // 일시정지 창과 설정 창 어느 쪽에서든 게임으로 돌아간다 (Esc 포함)
    private void Resume()
    {
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
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

    private void GoHome()
    {
        // 홈 화면을 건너뛰지 않도록 플래그 해제
        GameFlow.SkipHome = false;

        if (string.IsNullOrEmpty(homeSceneName))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // 홈이 같은 씬의 패널인 경우
        else
            SceneManager.LoadScene(homeSceneName);                            // 홈이 별도 씬인 경우
    }

    // 앱이 백그라운드로 가거나 종료될 때, 게임오버 전이라도 현재 점수를 보존하고 싶다면 사용
    // (원하지 않으면 이 메서드는 삭제해도 됩니다)
    private void OnApplicationPause(bool paused)
    {
        if (paused && !gameOverHandled && stageManager != null)
            ScoreSaver.SaveIfBest(stageManager.GetScore());
    }
}
