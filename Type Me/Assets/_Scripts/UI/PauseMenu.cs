using UnityEngine;
using UnityEngine.UI;
using System;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button pauseButton;
    private void OnEnable()
    {
        GameManager.OnGameManagerReady += SetupButtons;

        // If GameManager is already ready, setup immediately
        if (GameManager.Instance != null)
            SetupButtons();
    }
    private void OnDisable()
    {
        GameManager.OnGameManagerReady -= SetupButtons;
    }


    private void SetupButtons()
    {
        resumeButton?.onClick.AddListener(() => GameManager.Instance?.ResumeGame());
        restartButton?.onClick.AddListener(() => GameManager.Instance?.RestartLevel());
        homeButton?.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());
        pauseButton?.onClick.AddListener(()=> GameManager.Instance?.PauseGame());

    }

    private void OnDestroy()
    {
        resumeButton.onClick.RemoveAllListeners();
        homeButton.onClick.RemoveAllListeners();
        restartButton.onClick.RemoveAllListeners();
        pauseButton.onClick.RemoveAllListeners();
    }
}
