using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    // Events
    public event Action<Vector3, int> OnScoreAdded; // Pass both world position and points
    public event Action<int> OnScoreChanged;
    public event Action<int> OnHighScoreChanged;

    // Properties
    public int currentScore => _currentScore;
    public int highScore => _highScore;
    public LevelData CurrentLevel => _currentLevel;

    // Private fields
    private int _currentScore;
    private int _highScore;
    private LevelData _currentLevel;

    private void Awake()
    {
        // Standard singleton pattern (Bootstrap handles persistence)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        // Listen for session events to auto-initialize
        SessionManager.OnSessionStarted += OnSessionStarted;
    }

    private void OnDisable()
    {
        SessionManager.OnSessionStarted -= OnSessionStarted;
    }

    private void OnSessionStarted()
    {
        // Auto-start level when session starts
        if (SessionManager.Instance?.CurrentLevel != null)
        {
            StartLevel(SessionManager.Instance.CurrentLevel);
        }
    }

    public void StartLevel(LevelData level)
    {
        _currentLevel = level;
        _currentScore = 0;

        LoadHighScore(level);

        // Notify UI of reset
        OnScoreChanged?.Invoke(_currentScore);
        OnHighScoreChanged?.Invoke(_highScore);

   
    }

    public void AddScore(int amount, Vector3 worldPos = default)
    {
        if (amount <= 0) return;

        _currentScore += amount;

        // Trigger events
        OnScoreAdded?.Invoke(worldPos, amount); // For popups
        OnScoreChanged?.Invoke(_currentScore);  // For UI text

        // Check for new high score
        if (_currentScore > _highScore)
        {
            _highScore = _currentScore;
            SaveHighScore(_currentLevel);
            OnHighScoreChanged?.Invoke(_highScore);
    
        }
    }

    public void ResetScore()
    {
        _currentScore = 0;
        OnScoreChanged?.Invoke(_currentScore);
   
    }

    public void SetScore(int score)
    {
        _currentScore = Mathf.Max(0, score);
        OnScoreChanged?.Invoke(_currentScore);

        // Check for high score
        if (_currentScore > _highScore)
        {
            _highScore = _currentScore;
            SaveHighScore(_currentLevel);
            OnHighScoreChanged?.Invoke(_highScore);
        }
    }

    public int GetHighScore(string levelName)
    {
        if (string.IsNullOrEmpty(levelName)) return 0;
        return PlayerPrefs.GetInt($"HighScore_{levelName}", 0);
    }

    public void SaveCurrentProgress()
    {
        SaveHighScore(_currentLevel);
    }

    private void SaveHighScore(LevelData level)
    {
        if (level == null) return;

        PlayerPrefs.SetInt($"HighScore_{level.levelName}", _highScore);
        PlayerPrefs.Save();
       
    }

    private void LoadHighScore(LevelData level)
    {
        if (level == null)
        {
            _highScore = 0;
            return;
        }

        _highScore = PlayerPrefs.GetInt($"HighScore_{level.levelName}", 0);
   
    }


}