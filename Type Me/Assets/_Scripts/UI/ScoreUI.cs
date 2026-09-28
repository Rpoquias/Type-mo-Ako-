using TMPro;
using UnityEngine;
using System.Collections;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;



    private void Start()
    {
        StartCoroutine(WaitForScoreManagerAndSubscribe());
    }

    private IEnumerator WaitForScoreManagerAndSubscribe()
    {
        while (ScoreManager.Instance == null)
            yield return null;

        // Subscribe to both score and high score changes
        ScoreManager.Instance.OnScoreChanged += UpdateScore;
        ScoreManager.Instance.OnHighScoreChanged += UpdateHighScore; // NEW: Listen for high score changes

        // Initialize both texts
        UpdateScore(ScoreManager.Instance.currentScore);
        UpdateHighScore(ScoreManager.Instance.highScore);



    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScore;
            ScoreManager.Instance.OnHighScoreChanged -= UpdateHighScore; // NEW: Unsubscribe
        }
    }

    private void UpdateScore(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {newScore}";
        }
    }

    private void UpdateHighScore(int newHighScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {newHighScore}";
           
        }
    }
}
