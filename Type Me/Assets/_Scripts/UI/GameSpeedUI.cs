using TMPro;
using UnityEngine;

public class GameSpeedUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text speedText;

    private void OnEnable()
    {
        GameManager.OnGameSpeedChanged += UpdateSpeedUI;
    }

    private void OnDisable()
    {
        GameManager.OnGameSpeedChanged -= UpdateSpeedUI;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            UpdateSpeedUI(GameManager.Instance.CurrentGameSpeed);
        }
    }

    public void SetSpeed(float speed)
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.SetGameSpeed(speed);
    }

    public void IncreaseSpeed()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.IncreaseGameSpeed();
    }

    public void ResetSpeed()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.ResetGameSpeed();
    }

    private void UpdateSpeedUI(float speed)
    {
        if (speedText == null)
            return;

        speedText.text = $"x{speed:0}";
    }
}