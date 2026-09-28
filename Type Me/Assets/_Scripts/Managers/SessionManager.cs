using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    [Header("Fallback Settings for Direct Scene Play")]
    [SerializeField] private int fallbackStartingLives = 3;
    [SerializeField] private string fallbackLevelName = "TestLevel";

    private float sessionStartTime;
    private SessionResult? lastResult;
    private bool isSessionActive;

    // ? PRESERVE ORIGINAL LEVEL DATA
    private LevelData originalLevelData;  // Keep reference to original level
    private LevelData runtimeLevelData;   // Working copy that can be modified

    public LevelData CurrentLevel => runtimeLevelData;
    public LevelData OriginalLevel => originalLevelData;  // NEW: Access to original data
    public bool IsSessionActive => isSessionActive;

    // Events for other systems to listen to
    public static event System.Action OnSessionStarted;
    public static event System.Action<SessionResult> OnSessionEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Skip main menu and UI scenes
        if (IsMenuScene(scene.name)) return;

        // MODIFIED: Don't auto-start session immediately - let GameInitializer handle it
        if (originalLevelData != null && scene.name == originalLevelData.levelName)
        {
            // Restore original level data for restart
            runtimeLevelData = CreateLevelDataCopy(originalLevelData);
        }

        // Create fallback session if needed but DON'T start it yet
        if (runtimeLevelData == null)
        {
            CreateFallbackSession(scene.name);
        }

        // DON'T start session here - GameInitializer will start it after countdown
    }

    // NEW: Method to start session after countdown
    public void StartSessionAfterCountdown()
    {
        if (runtimeLevelData != null && !isSessionActive)
        {
            StartSessionInternal();
        }
    }


    private bool IsMenuScene(string sceneName)
    {
        return sceneName.ToLower().Contains("menu") ||
               sceneName.ToLower().Contains("lobby") ||
               sceneName.ToLower().Contains("start");
    }

    private void CreateFallbackSession(string sceneName)
    {
        // Create a fallback LevelData for direct scene testing
        var fallbackLevel = ScriptableObject.CreateInstance<LevelData>();
        fallbackLevel.levelName = sceneName;
        fallbackLevel.startingLives = fallbackStartingLives;

        // NEW: Enable countdown for fallback sessions
        fallbackLevel.requiresCountdown = true;
        fallbackLevel.countdownDuration = 3;

        // Set both original and runtime data to fallback
        originalLevelData = fallbackLevel;
        runtimeLevelData = CreateLevelDataCopy(fallbackLevel);
    }

    private LevelData CreateLevelDataCopy(LevelData original)
    {
        var copy = ScriptableObject.CreateInstance<LevelData>();
        copy.levelName = original.levelName;
        copy.description = original.description;
        copy.startingLives = original.startingLives;

        // NEW: Copy countdown settings
        copy.requiresCountdown = original.requiresCountdown;
        copy.countdownDuration = original.countdownDuration;

        return copy;
    }

    public void StartSession(LevelData level)
    {
        // ? PRESERVE ORIGINAL LEVEL DATA
        originalLevelData = level;
        runtimeLevelData = CreateLevelDataCopy(level);
        lastResult = null;
    }

    private void StartSessionInternal()
    {
        if (isSessionActive)
        {
            return;
        }

        sessionStartTime = Time.time;
        isSessionActive = true;

        // Notify other systems that session has started
        OnSessionStarted?.Invoke();
    }

    public void EndSession(bool playerDied = false)
    {

        if (!isSessionActive)
        {
            if (playerDied)
            {
                CreateEmergencySessionResult();
            }
            return;
        }

        if (runtimeLevelData == null)
        {
            CreateEmergencySessionResult();
            return;
        }

        isSessionActive = false;

        // Get final time and score
        float finalTime = Time.time - sessionStartTime;
        int finalScore = ScoreManager.Instance?.currentScore ?? 0;

        // Stop the timer if it exists
        GameTimer.Instance?.StopLevel();

        // Use timer's elapsed time if available, otherwise use calculated time
        if (GameTimer.Instance != null)
        {
            finalTime = GameTimer.Instance.ElapsedTime;
        }

        // Update persistent stats BEFORE creating result
        if (LevelStatsManager.Instance != null)
        {
            LevelStatsManager.Instance.UpdateLevelStats(
                runtimeLevelData.levelName,
                finalScore,
                finalTime
            );

            // Get the updated high scores for the result
            int highScore = LevelStatsManager.Instance.GetHighScore(runtimeLevelData.levelName);
            float longestTime = LevelStatsManager.Instance.GetLongestTime(runtimeLevelData.levelName);

            // Create session result with updated persistent data
            lastResult = new SessionResult(
                finalScore,
                finalTime,
                highScore,  // Use actual high score from PlayerPrefs
                longestTime // Use actual longest time from PlayerPrefs
            );
        }
        else
        {
            // Create session result without saving
            lastResult = new SessionResult(
                finalScore,
                finalTime,
                finalScore,
                finalTime
            );
        }

        OnSessionEnded?.Invoke(lastResult.Value);
    }

    private void CreateEmergencySessionResult()
    {
        // Create a basic session result when no session was properly started
        float emergencyTime = GameTimer.Instance?.ElapsedTime ?? 0f;
        int emergencyScore = ScoreManager.Instance?.currentScore ?? 0;

        // Still try to save stats if possible
        if (LevelStatsManager.Instance != null && runtimeLevelData != null)
        {
            LevelStatsManager.Instance.UpdateLevelStats(
                runtimeLevelData.levelName,
                emergencyScore,
                emergencyTime
            );
        }

        lastResult = new SessionResult(
            emergencyScore,
            emergencyTime,
            emergencyScore,
            emergencyTime
        );


    }

    public SessionResult? GetLastResult()
    {

        return lastResult;
    }

    // ?MODIFIED: Reset session but preserve original level data
    public void ResetSessionForRestart()
    {
        isSessionActive = false;
        lastResult = null;

        // DON'T clear level data - it will be restored from originalLevelData on scene load

    }

    //  KEEP: Full clear for returning to main menu
    public void ClearSession()
    {
        originalLevelData = null;
        runtimeLevelData = null;
        isSessionActive = false;
        lastResult = null;

    }

    public void ForceEndSession()
    {

        if (isSessionActive)
        {
            EndSession();
        }
        else
        {
            CreateEmergencySessionResult();
        }
    }

}
