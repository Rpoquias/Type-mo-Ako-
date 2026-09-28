using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestTimeText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button restartButton;

    private bool isSetup = false;

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false); // Start hidden
            Debug.Log("GameOverUI: Panel hidden on Awake");
        }
        else
        {
            Debug.LogError("GameOverUI: Panel reference is missing!");
        }
    }

    private void OnEnable()
    {
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (isSetup) return;

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => {
                GameManager.Instance?.RestartLevel();
            });
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(() => {
                GameManager.Instance?.GoToMainMenu();
            });
        }

        isSetup = true;
    }

    public void Show(float finalTime, int finalScore, float bestTime, int highScore)
    {
        Debug.Log($"GameOverUI: Show() called - Time: {finalTime:F2}s, Score: {finalScore}");

        if (panel == null)
        {
            Debug.LogError("GameOverUI: Cannot show - panel is null!");
            return;
        }

        // Update all text fields
        if (timeText != null)
            timeText.text = FormatTime(finalTime, "Time");
        else
            Debug.LogWarning("GameOverUI: timeText is null");

        if (scoreText != null)
            scoreText.text = $"Score: {finalScore}";
        else
            Debug.LogWarning("GameOverUI: scoreText is null");

        if (bestTimeText != null)
            bestTimeText.text = FormatTime(bestTime, "Best Time");
        else
            Debug.LogWarning("GameOverUI: bestTimeText is null");

        if (highScoreText != null)
            highScoreText.text = $"High Score: {highScore}";
        else
            Debug.LogWarning("GameOverUI: highScoreText is null");

        // Show the panel
        panel.SetActive(true);
        Debug.Log("GameOverUI: Panel activated successfully");
    }

    public void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
            Debug.Log("GameOverUI: Panel hidden");
        }
    }

    private string FormatTime(float time, string prefix)
    {
        if (time <= 0)
        {
            return $"{prefix}: 00:00";
        }

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{prefix}: {minutes:00}:{seconds:00}";
    }



}