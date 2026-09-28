using UnityEngine;
using System;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    // Events
    public event Action<float> OnTimerUpdated;
    public event Action<float> OnBestTimeUpdated;

    // Properties
    public float ElapsedTime => _elapsedTime;
    public float BestTime => _bestTime;
    public bool IsRunning => _isRunning;
    public LevelData CurrentLevel => _currentLevel;

    // Private fields
    private float _elapsedTime;
    private float _bestTime;
    private bool _isRunning;
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
        SessionManager.OnSessionEnded += OnSessionEnded;
    }

    private void OnDisable()
    {
        SessionManager.OnSessionStarted -= OnSessionStarted;
        SessionManager.OnSessionEnded -= OnSessionEnded;
    }

    private void Update()
    {
        if (!_isRunning) return;

        _elapsedTime += Time.deltaTime;
        OnTimerUpdated?.Invoke(_elapsedTime);
    }


    private void OnSessionStarted()
    {
        
    }

    // NEW: Public method for manual start
    public void StartTimerManually()
    {
        if (SessionManager.Instance?.CurrentLevel != null)
        {
            StartLevel(SessionManager.Instance.CurrentLevel);
        }
    }

    private void OnSessionEnded(SessionResult result)
    {
        // Auto-stop timer when session ends
        StopLevel();
    }

    public void StartLevel(LevelData level)
    {
        _currentLevel = level;
        _elapsedTime = 0f;
        _isRunning = true;

        LoadBestTime(level);

        // Notify listeners
        OnTimerUpdated?.Invoke(_elapsedTime);
        OnBestTimeUpdated?.Invoke(_bestTime);


    }

    public void StopLevel()
    {
        if (!_isRunning) return;

        _isRunning = false;
        UpdateBestTime();

    }

    public void PauseTimer()
    {
        _isRunning = false;
 
    }

    public void ResumeTimer()
    {
        if (_currentLevel != null)
        {
            _isRunning = true;

        }
    }

    public void ResetTimer()
    {
        _elapsedTime = 0f;
        _isRunning = false;
        OnTimerUpdated?.Invoke(_elapsedTime);

    }

    public float GetBestTime(string levelName)
    {
        if (string.IsNullOrEmpty(levelName)) return 0f;
        return PlayerPrefs.GetFloat($"BestTime_{levelName}", 0f);
    }

    public void SaveCurrentProgress()
    {
        if (_isRunning) // Only update if currently playing
        {
            UpdateBestTime();
        }
    }

    private void UpdateBestTime()
    {
        if (_currentLevel == null) return;

        // Only update if current time is better (longer survival time)
        if (_elapsedTime > _bestTime)
        {
            _bestTime = _elapsedTime;
            SaveBestTime(_currentLevel);
            OnBestTimeUpdated?.Invoke(_bestTime);
         
        }
    }

    private void SaveBestTime(LevelData level)
    {
        if (level == null) return;

        PlayerPrefs.SetFloat($"BestTime_{level.levelName}", _bestTime);
        PlayerPrefs.Save();
  
    }

    private void LoadBestTime(LevelData level)
    {
        if (level == null)
        {
            _bestTime = 0f;
            return;
        }

        _bestTime = PlayerPrefs.GetFloat($"BestTime_{level.levelName}", 0f);
      
    }

    public string FormatTime(float time)
    {
        if (time <= 0) return "00:00";

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public string FormatTimeDetailed(float time)
    {
        if (time <= 0) return "00:00.00";

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time % 1f) * 100f);
        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }

   

}