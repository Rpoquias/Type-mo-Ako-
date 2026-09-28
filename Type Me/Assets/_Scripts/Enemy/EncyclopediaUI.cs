using UnityEngine;

public class EncyclopediaUI : MonoBehaviour
{
    [SerializeField] private EnemyData[] enemies;
    [SerializeField] private EnemyButton buttonPrefab;
    [SerializeField] private Transform content;
    [SerializeField] private EnemyEntryUI infoPanel;

    private void Start()
    {
        foreach (EnemyData enemy in enemies)
        {
            EnemyButton button = Instantiate(buttonPrefab, content);
            button.Setup(enemy, this);
        }

        if (enemies.Length > 0)
            ShowEnemy(enemies[0]);
    }

    public void ShowEnemy(EnemyData enemy)
    {
        infoPanel.Setup(enemy);
    }
}