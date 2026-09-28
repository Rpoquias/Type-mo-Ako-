using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEntryUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text levelNameText;
    public TMP_Text descriptionText;
    public TMP_Text startingLivesText;
    public TMP_Text highScoreText;
    public TMP_Text longestTimeText;

    [Header("Visual Elements")]
    public Image panelImage; // NEW: Reference to the background panel image
    public Image levelMapImage;

    [Header("Button")]
    public Button playButton;

    private LevelData currentLevel;

    public void Setup(LevelData level)
    {
        currentLevel = level;

        // Set basic level info
        levelNameText.text = level.levelName;
        descriptionText.text = level.description;
        startingLivesText.text = $"Starting lives: {level.startingLives}";

        if (panelImage != null)
        {
            panelImage.sprite = level.bannerSprite;
        }


        if (levelMapImage != null)
        {
            levelMapImage.sprite = level.levelmap;

            // Hide it if no map exists
            levelMapImage.enabled = level.levelmap != null;
        }
        // Refresh stats display
        RefreshStats();
    }

    public void RefreshStats()
    {
        if (currentLevel == null) return;

        // Always use LevelStatsManager for consistent data
        if (LevelStatsManager.Instance != null)
        {
            int highScore = LevelStatsManager.Instance.GetHighScore(currentLevel.levelName);
            float longestTime = LevelStatsManager.Instance.GetLongestTime(currentLevel.levelName);

            highScoreText.text = $"High Score: {highScore}";
            longestTimeText.text = $"Longest Time: {LevelStatsManager.Instance.FormatTime(longestTime)}";
        }
        else
        {
            // Fallback to direct PlayerPrefs access
            int highScore = PlayerPrefs.GetInt($"HighScore_{currentLevel.levelName}", 0);
            float longestTime = PlayerPrefs.GetFloat($"LongestTime_{currentLevel.levelName}", 0f);

            highScoreText.text = $"High Score: {highScore}";
            longestTimeText.text = $"Longest Time: {FormatTime(longestTime)}";
        }
    }

    public Button GetPlayButton() => playButton;
    public LevelData GetLevel() => currentLevel;

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}
