using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    public static GameInitializer Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private GameSceneConfiguration sceneConfig;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Application.targetFrameRate = 30;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        // Skip Bootstrap and MainMenu
        if (sceneConfig.IsBootstrapScene(sceneName) ||
            sceneConfig.IsMainMenuScene(sceneName))
            return;

        // Handle gameplay scenes
        if (sceneConfig.IsGameplayScene(sceneName))
        {
            CountdownManager.Instance?.InitializeGameplayScene(sceneName);
        }
    }

}
