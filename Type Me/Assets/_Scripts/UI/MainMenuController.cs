using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectionPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject enemiesPanel;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private LevelEntryUI levelEntryPrefab;

    [Header("Tutorial Parts")]
    [SerializeField] private GameObject tutorial1stPart;
    [SerializeField] private GameObject tutorial2ndPart;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button enemiesButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button backFromLevelButton;
    [SerializeField] private Button backFromSettingsButton;
    [SerializeField] private Button backFromEnemiesButton;
    [SerializeField] private Button backFromTutorialButton;

    [Header("Tutorial Navigation Buttons")]
    [SerializeField] private Button nextTo2ndPartButton;
    [SerializeField] private Button backTo1stPartButton;

    [Header("Level Container")]
    [SerializeField] private Transform levelButtonContainer;

    [Header("Levels")]
    [SerializeField] private LevelData[] levels;


    [Header("Credits Panel")]
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private RectTransform creditsContent;   // Content under Viewport
    [SerializeField] private ScrollRect creditsScrollRect;   // ScrollRect on Scroll View

    [Header("Credits Buttons")]
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button backFromCreditsButton;
    private List<LevelEntryUI> levelEntries = new List<LevelEntryUI>();

    private void Awake()
    {
        SetupButtons();
        ShowPanel(mainMenuPanel);

        // Ensure managers are ready
        EnsureManagersExist();
    }

    private void OnEnable()
    {
        // Refresh level stats when returning to main menu
        StartCoroutine(RefreshStatsAfterFrame());
    }

    private void EnsureManagersExist()
    {
        // With Bootstrap pattern, managers should already exist
        // Just log if they're missing (shouldn't happen in production)
        if (SessionManager.Instance == null)
            Debug.LogError("SessionManager not found! Make sure Bootstrap is in the scene.");

        if (ScoreManager.Instance == null)
            Debug.LogError("ScoreManager not found! Make sure it's in the system prefab.");

        if (GameTimer.Instance == null)
            Debug.LogError("GameTimer not found! Make sure it's in the system prefab.");
    }

    private IEnumerator RefreshStatsAfterFrame()
    {
        yield return null; // Wait one frame for managers to initialize
        RefreshAllLevelStats();
    }

    private void SetupButtons()
    {
        // Main menu buttons
        playButton.onClick.AddListener(OpenLevelSelection);
        settingsButton.onClick.AddListener(OpenSettings);
        enemiesButton.onClick.AddListener(OpenEnemies);
        tutorialButton.onClick.AddListener(OpenTutorial);
        quitButton.onClick.AddListener(QuitGame);

        // Back buttons
        backFromLevelButton.onClick.AddListener(() => ShowPanel(mainMenuPanel));
        backFromSettingsButton.onClick.AddListener(() => ShowPanel(mainMenuPanel));
        backFromEnemiesButton.onClick.AddListener(() => ShowPanel(mainMenuPanel));
        backFromTutorialButton.onClick.AddListener(() => ShowPanel(mainMenuPanel));

        // Tutorial navigation buttons
        nextTo2ndPartButton.onClick.AddListener(ShowTutorial2ndPart);
        backTo1stPartButton.onClick.AddListener(ShowTutorial1stPart);

        // Credits button
        creditsButton.onClick.AddListener(OpenCredits);
        backFromCreditsButton.onClick.AddListener(() => ShowPanel(mainMenuPanel));
    }

    private void ShowPanel(GameObject panelToShow)
    {
        mainMenuPanel.SetActive(false);
        levelSelectionPanel.SetActive(false);
        settingsPanel.SetActive(false);
        enemiesPanel.SetActive(false);
        tutorialPanel.SetActive(false);
        creditsPanel.SetActive(false);

        panelToShow.SetActive(true);
    }

    private void OpenLevelSelection()
    {
        ShowPanel(levelSelectionPanel);
        PopulateLevelPanel();
        RefreshAllLevelStats();
    }

    private void OpenSettings()
    {
        ShowPanel(settingsPanel);
    }

    private void OpenEnemies()
    {
        ShowPanel(enemiesPanel);
    }

    private void OpenTutorial()
    {
        ShowPanel(tutorialPanel);
        ShowTutorial1stPart(); // Always start with first part
    }

    private void ShowTutorial1stPart()
    {
        tutorial1stPart.SetActive(true);
        tutorial2ndPart.SetActive(false);
    }

    private void ShowTutorial2ndPart()
    {
        tutorial1stPart.SetActive(false);
        tutorial2ndPart.SetActive(true);
    }
    private void OpenCredits()
    {
        ShowPanel(creditsPanel);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(creditsContent);


        creditsScrollRect.verticalNormalizedPosition = 1f;
    }

    private void QuitGame()
    {
        Debug.Log("Quit Game");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    private void PopulateLevelPanel()
    {
        if (levelEntryPrefab == null || levelButtonContainer == null || levels == null) return;

        // Clear existing entries
        foreach (Transform child in levelButtonContainer)
            Destroy(child.gameObject);

        levelEntries.Clear();

        // Create new entries
        foreach (LevelData level in levels)
        {
            if (level == null) continue;

            LevelEntryUI entry = Instantiate(levelEntryPrefab, levelButtonContainer);
            entry.Setup(level);
            levelEntries.Add(entry);

            // Setup button click
            Button playBtn = entry.GetPlayButton();
            if (playBtn != null)
            {
                playBtn.onClick.RemoveAllListeners();
                playBtn.onClick.AddListener(() => StartLevel(level));
            }
        }
    }

    private void RefreshAllLevelStats()
    {
        if (levelEntries == null || levelEntries.Count == 0) return;

        foreach (var entry in levelEntries)
        {
            if (entry != null)
                entry.RefreshStats();
        }
    }

    public void StartLevel(LevelData level)
    {
        if (level == null)
        {
            Debug.LogError("Cannot start level - LevelData is null!");
            return;
        }

        Debug.Log($"Starting level: {level.levelName}");

        // Ensure SessionManager exists
        if (SessionManager.Instance == null)
        {
            Debug.LogError("SessionManager not found - cannot start level!");
            return;
        }

        // Clear any existing session and start new one
        SessionManager.Instance.ClearSession();
        SessionManager.Instance.StartSession(level);

        // Load the scene
        if (!string.IsNullOrEmpty(level.levelName))
        {
            SceneManager.LoadScene(level.levelName);
        }
        else
        {
            Debug.LogError($"Level {level.name} has no scene name specified!");
        }
    }

    // Public method for external calls
    public void RefreshLevelStats()
    {
        RefreshAllLevelStats();
    }

    // For debugging - test level loading
    [ContextMenu("Test Level Load")]
    private void TestLevelLoad()
    {
        if (levels != null && levels.Length > 0)
        {
            StartLevel(levels[0]);
        }
    }
}
