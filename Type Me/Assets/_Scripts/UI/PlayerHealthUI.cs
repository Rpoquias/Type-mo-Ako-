using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Display")]
    [SerializeField] private bool showHealthText = false;

    [Header("Auto Hide")]
    [SerializeField] private bool autoHide = true;
    [SerializeField] private float visibleDuration = 3f;

    private Coroutine hideRoutine;

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += UpdateHealthUI;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthUI;
    }

    private void Start()
    {
        UpdateHealthUI(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());

        if (autoHide)
            canvasGroup.alpha = 0f;
    }

    private void UpdateHealthUI(int current, int max)
    {
        float percent = (float)current / max;

        if (healthFill != null)
            healthFill.fillAmount = percent;

        if (healthText != null)
        {
            healthText.gameObject.SetActive(showHealthText);
            healthText.text = $"{current}/{max} HP";
        }

        if (autoHide)
        {
            ShowTemporary();
        }
    }

    private void ShowTemporary()
    {
        canvasGroup.alpha = 1f;

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(visibleDuration);

        canvasGroup.alpha = 0f;
    }
}