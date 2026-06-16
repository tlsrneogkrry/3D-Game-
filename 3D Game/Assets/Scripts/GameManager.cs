using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float gameDuration = 60f;
    public int clearScore = 100; // 이 점수 이상이면 게임 클리어

    private float timeRemaining;
    private int score = 0;
    private bool gameRunning = false;

    public enum GameState { Playing, GameOver, GameClear }
    private GameState currentState;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => StartGame();

    void Update()
    {
        if (!gameRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            // 시간 종료 → 점수에 따라 클리어 or 게임오버
            if (score >= clearScore)
                TriggerGameClear();
            else
                TriggerGameOver();
        }
    }

    public void StartGame()
    {
        score = 0;
        timeRemaining = gameDuration;
        gameRunning = true;
        currentState = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void AddScore(int amount)
    {
        if (!gameRunning) return;
        score += amount;

        // 점수 달성 즉시 클리어 판정
        if (score >= clearScore)
            TriggerGameClear();
    }

    public void TriggerGameOver()
    {
        if (!gameRunning) return;
        gameRunning = false;
        currentState = GameState.GameOver;
        Time.timeScale = 0f; // 게임 일시정지
    }

    public void TriggerGameClear()
    {
        if (!gameRunning) return;
        gameRunning = false;
        currentState = GameState.GameClear;
        Time.timeScale = 0f;
    }

    // 프로퍼티
    public float TimeRemaining => timeRemaining;
    public int Score => score;
    public int ClearScore => clearScore;
    public GameState CurrentState => currentState;
    public bool GameRunning => gameRunning;
}