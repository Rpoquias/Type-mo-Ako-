using UnityEngine;
using TMPro;
using System.Collections;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private void Start()
    {
        // Wait for GameTimer to initialize before subscribing
        StartCoroutine(WaitForGameTimerAndSubscribe());
    }

    private IEnumerator WaitForGameTimerAndSubscribe()
    {
        // Wait until GameTimer.Instance is available
        while (GameTimer.Instance == null)
        {
            yield return null;
        }

        // Now safely subscribe to the event
        GameTimer.Instance.OnTimerUpdated += UpdateTimerUI;
        Debug.Log(" TimerUI: Successfully subscribed to GameTimer events!");
    }

    private void OnDisable()
    {
        // Add null check before unsubscribing
        if (GameTimer.Instance != null)
            GameTimer.Instance.OnTimerUpdated -= UpdateTimerUI;
    }

    private void UpdateTimerUI(float elapsedTime)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
