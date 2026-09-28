using UnityEngine;

public class PauseInput : MonoBehaviour
{
    private void Update()
    {
        // IMPORTANT: Check if input is blocked first
        if (InputBlocker.Instance != null && InputBlocker.Instance.IsInputBlocked())
        {
            return; // Don't process any input during countdown
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.IsPaused)
                GameManager.Instance.ResumeGame();
            else
                GameManager.Instance.PauseGame();
        }
    }
}
