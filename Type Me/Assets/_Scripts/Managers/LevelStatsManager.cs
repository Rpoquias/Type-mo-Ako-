using UnityEngine;

public class LevelStatsManager : MonoBehaviour
{
    public static LevelStatsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GetHighScore(string levelName) =>
        PlayerPrefs.GetInt($"HighScore_{levelName}", 0);

    public float GetLongestTime(string levelName) =>
        PlayerPrefs.GetFloat($"LongestTime_{levelName}", 0f);

    public void UpdateLevelStats(string levelName, int score, float timePlayed)
    {
        if (score > GetHighScore(levelName))
            PlayerPrefs.SetInt($"HighScore_{levelName}", score);

        if (timePlayed > GetLongestTime(levelName))
            PlayerPrefs.SetFloat($"LongestTime_{levelName}", timePlayed);

        PlayerPrefs.Save();
    }

    public string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}