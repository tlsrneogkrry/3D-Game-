using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameOverTitleText;

    [Header("Game Clear Panel")]
    public GameObject gameClearPanel;
    public TextMeshProUGUI clearScoreText;
    public TextMeshProUGUI clearStarText; // 별점 등 연출용 (선택)

    [Header("Timer Warning")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public float warningThreshold = 10f;

    private GameManager.GameState lastState = GameManager.GameState.Playing;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        UpdateHUD();
        CheckStateChange();
    }

    void UpdateHUD()
    {
        // 타이머
        if (timerText != null)
        {
            float t = GameManager.Instance.TimeRemaining;
            int m = Mathf.FloorToInt(t / 60f);
            int s = Mathf.FloorToInt(t % 60f);
            timerText.text = $"⏱ {m:00}:{s:00}";
            timerText.color = (t <= warningThreshold) ? warningColor : normalColor;
        }

        // 점수
        if (scoreText != null)
        {
            int cur = GameManager.Instance.Score;
            int goal = GameManager.Instance.ClearScore;
            scoreText.text = $"점수: {cur} / {goal}";
        }
    }

    void CheckStateChange()
    {
        var state = GameManager.Instance.CurrentState;
        if (state == lastState) return;
        lastState = state;

        if (state == GameManager.GameState.GameOver) ShowGameOver();
        if (state == GameManager.GameState.GameClear) ShowGameClear();
    }

    void ShowGameOver()
    {
        if (gameOverPanel == null) return;
        gameOverPanel.SetActive(true);

        if (gameOverTitleText != null)
            gameOverTitleText.text = "💀 GAME OVER";

        if (gameOverScoreText != null)
            gameOverScoreText.text = $"최종 점수\n{GameManager.Instance.Score}점";
    }

    void ShowGameClear()
    {
        if (gameClearPanel == null) return;
        gameClearPanel.SetActive(true);

        if (clearScoreText != null)
            clearScoreText.text = $"🎉 클리어!\n{GameManager.Instance.Score}점";

        if (clearStarText != null)
        {
            // 남은 시간 기준 별점 연출
            float t = GameManager.Instance.TimeRemaining;
            string stars = t > 40f ? "⭐⭐⭐" : t > 20f ? "⭐⭐" : "⭐";
            clearStarText.text = stars;
        }
    }

    // 버튼 연결용
    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuitButton()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}