using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameOverUI gameOverPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Debug.Log("UIManager: Initialized");
    }

    private void Start()
    {

        SetPauseMenuActive(false);


        Debug.Log("UIManager: Start complete");
    }

    public void SetPauseMenuActive(bool active)
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(active);
    }

    public void ShowGameOverPanel(SessionResult result)
    {

        if (gameOverPanel != null)
        {
            gameOverPanel.Show(
                result.timeLasted,
                result.score,
                result.bestTime,
                result.highScore
            );
        }
        else
        {
        }
    }
}