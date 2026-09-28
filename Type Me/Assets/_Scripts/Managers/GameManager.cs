using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsPaused { get; private set; }

    private PlayerHealth playerHealth;
    private bool gameOverTriggered = false;

    public static event System.Action OnGameManagerReady;

    [Header("Game Speed")]
    [SerializeField] private float[] gameSpeeds = { 1f, 2f, 4f, 8f };

    private int currentSpeedIndex = 0;

    public float CurrentGameSpeed => gameSpeeds[currentSpeedIndex];

    public static event System.Action<float> OnGameSpeedChanged;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        gameOverTriggered = false;
        currentSpeedIndex = 0;
        Time.timeScale = 1f;

        OnGameManagerReady?.Invoke();
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        StartCoroutine(InitializeAfterFrame());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied -= HandleGameOver;
        }
    }


    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            IncreaseGameSpeed();
        }


        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ResetGameSpeed();
        }
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameOverTriggered = false;


        currentSpeedIndex = 0;
        Time.timeScale = 1f;

        StartCoroutine(InitializeAfterFrame());
    }

    private IEnumerator InitializeAfterFrame()
    {
        yield return null;

        FindAndConnectPlayerHealth();
    }

    private void FindAndConnectPlayerHealth()
    {
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied -= HandleGameOver;
            playerHealth = null;
        }

        playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied += HandleGameOver;
        }
    }

    public void PauseGame()
    {
        if (gameOverTriggered)
            return;

        IsPaused = true;

        Time.timeScale = 0f;

        UIManager.Instance?.SetPauseMenuActive(true);
    }

    public void ResumeGame()
    {
        if (gameOverTriggered)
            return;

        IsPaused = false;
        ApplyGameSpeed();

        UIManager.Instance?.SetPauseMenuActive(false);
    }

    public void IncreaseGameSpeed()
    {
        // Don't allow speed changes while paused or game over
        if (IsPaused || gameOverTriggered)
            return;

        if (currentSpeedIndex < gameSpeeds.Length - 1)
        {
            currentSpeedIndex++;

            ApplyGameSpeed();
        }
    }

    public void ResetGameSpeed()
    {
        // Don't allow speed changes while paused or game over
        if (IsPaused || gameOverTriggered)
            return;

        currentSpeedIndex = 0;

        ApplyGameSpeed();
    }

    public void SetGameSpeed(float speed)
    {
        // Don't allow speed changes while paused or game over
        if (IsPaused || gameOverTriggered)
            return;

        for (int i = 0; i < gameSpeeds.Length; i++)
        {
            if (Mathf.Approximately(gameSpeeds[i], speed))
            {
                currentSpeedIndex = i;

                ApplyGameSpeed();

                return;
            }
        }

        Debug.LogWarning(
            $"Game speed {speed}x is not available."
        );
    }

    private void ApplyGameSpeed()
    {
        Time.timeScale = gameSpeeds[currentSpeedIndex];

        OnGameSpeedChanged?.Invoke(CurrentGameSpeed);

        Debug.Log($"Game Speed: {CurrentGameSpeed}x");
    }


    public void RestartLevel()
    {
        Time.timeScale = 1f;

        currentSpeedIndex = 0;

        gameOverTriggered = false;

        AudioManager.Instance?.ResetGameOverAudioState();

        SessionManager.Instance?.ResetSessionForRestart();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        currentSpeedIndex = 0;

        gameOverTriggered = false;

        SessionManager.Instance?.ClearSession();

        SceneManager.LoadScene("MainMenu");
    }

    private void HandleGameOver()
    {
        if (gameOverTriggered)
        {
            return;
        }

        gameOverTriggered = true;

        Time.timeScale = 0f;

        PlayLoseSFX();

        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.ForceEndSession();

            var result = SessionManager.Instance.GetLastResult();

            if (result.HasValue)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowGameOverPanel(result.Value);
                }
            }
            else
            {
                ShowEmergencyGameOver();
            }
        }
        else
        {
            ShowEmergencyGameOver();
        }
    }


    private void PlayLoseSFX()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLoseSFX();
        }
    }


    private void ShowEmergencyGameOver()
    {
        var emergencyResult = new SessionResult(
            0,
            0f,
            0,
            0f
        );

        UIManager.Instance?.ShowGameOverPanel(emergencyResult);
    }


    public bool IsGameOver()
    {
        return gameOverTriggered;
    }
}