using UnityEngine;
using System.Collections;

public class CountdownManager : MonoBehaviour
{
    public static CountdownManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private GameSceneConfiguration sceneConfig;

    private CountdownUI countdownUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void InitializeGameplayScene(string sceneName)
    {
        var currentLevel = SessionManager.Instance?.CurrentLevel;
        var sceneInfo = sceneConfig?.GetGameplaySceneInfo(sceneName);

        bool needsCountdown = GetCountdownRequired(currentLevel, sceneInfo);

        if (needsCountdown)
        {
            int duration = GetCountdownDuration(currentLevel, sceneInfo);
            string displayName = GetDisplayName(currentLevel, sceneName);
            StartCoroutine(RunCountdownSequence(displayName, duration));
        }
        else
        {
            StartGameplayImmediately();
        }
    }

    private bool GetCountdownRequired(LevelData levelData, GameSceneConfiguration.GameplaySceneInfo sceneInfo)
    {
        if (levelData != null)
            return levelData.requiresCountdown;
        if (sceneInfo != null)
            return sceneInfo.forceCountdown;
        return false;
    }

    private int GetCountdownDuration(LevelData levelData, GameSceneConfiguration.GameplaySceneInfo sceneInfo)
    {
        if (levelData != null && levelData.countdownDuration > 0)
            return levelData.countdownDuration;
        if (sceneInfo != null)
            return sceneInfo.defaultCountdownDuration;
        return 3;
    }

    private string GetDisplayName(LevelData levelData, string sceneName)
    {
        if (levelData != null && !string.IsNullOrEmpty(levelData.levelName))
            return levelData.levelName;
        return sceneName;
    }

    public void StartGameplayImmediately()
    {
        InputBlocker.Instance?.UnblockInput("Gameplay started immediately");
        GameManager.Instance?.ResumeGame();
        SessionManager.Instance?.StartSessionAfterCountdown();
        GameTimer.Instance?.StartTimerManually();
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.enabled = true;
            SpawnManager.Instance.StartSpawning();
        }
        StartBGM();
    }

    private IEnumerator RunCountdownSequence(string levelName, int duration)
    {

        InputBlocker.Instance?.BlockInput("Countdown in progress");

        StopGameplaySystems();
        StopBGM();
        FindCountdownUI();


        if (countdownUI != null)
        {
            countdownUI.ShowCountdown(levelName);
            yield return new WaitForSeconds(0.5f);

            for (int i = duration; i > 0; i--)
            {
                countdownUI.UpdateCountdownNumber(i);
                yield return new WaitForSeconds(1f);
            }

            countdownUI.ShowGo();
            yield return new WaitForSeconds(1.5f);
            countdownUI.HideCountdown();
        }


        InputBlocker.Instance?.UnblockInput("Countdown finished");

        StartGameplayImmediately();
    }

    private void FindCountdownUI()
    {
        countdownUI = FindObjectOfType<CountdownUI>();
    }

    private void StopGameplaySystems()
    {
        if (GameTimer.Instance != null)
            GameTimer.Instance.PauseTimer();
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.StopSpawning();
            SpawnManager.Instance.enabled = false;
        }
    }

    private void StopBGM()
    {
        AudioManager.Instance?.StopBGM();
    }

    private void StartBGM()
    {
        AudioManager.Instance?.LoadAndPlayCurrentLevelMusic();
    }
}
