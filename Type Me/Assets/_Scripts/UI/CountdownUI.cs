using UnityEngine;
using TMPro;

public class CountdownUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI levelTitleText;
    [SerializeField] private TextMeshProUGUI readyText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("SFX")]
    [SerializeField] private AudioClip tickSFX;
    [SerializeField] private AudioClip goSFX;

    public void ShowCountdown(string levelName)
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        levelTitleText.text = levelName;
        readyText.text = "Be Ready!";
        countdownText.text = "";
    }

    public void UpdateCountdownNumber(int number)
    {
        countdownText.text = number.ToString();
        AudioManager.Instance?.PlaySFX(tickSFX);
    }

    public void ShowGo()
    {
        countdownText.text = "GO!";
        AudioManager.Instance?.PlaySFX(goSFX);
    }

    public void HideCountdown()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
