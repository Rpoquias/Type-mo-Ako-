using UnityEngine;
using UnityEngine.UI;

public class StatsPanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject statsPanel;   // The whole panel with time/score
    [SerializeField] private GameObject restoreButton; // The button next to pause button

    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f; // Seconds between clicks

    private void Start()
    {
       
    }

    // Hook this to your transparent Button's OnClick
    public void OnPanelClicked()
    {
        float currentTime = Time.time;

        if (currentTime - lastClickTime < doubleClickThreshold)
        {
            // Detected double-click ? hide panel
            TogglePanel(false);
        }

        lastClickTime = currentTime;
    }

    // Hook this to your restoreButton's OnClick
    public void OnRestoreButtonClicked()
    {
        TogglePanel(true);
    }

    private void TogglePanel(bool show)
    {
        if (statsPanel != null)
            statsPanel.SetActive(show);

        if (restoreButton != null)
            restoreButton.SetActive(!show);
    }
}
