using UnityEngine;
using UnityEngine.UI;

public class EnemyButton : MonoBehaviour
{
    [SerializeField] private Image icon;

    private EnemyData enemy;
    private EncyclopediaUI encyclopedia;

    public void Setup(EnemyData data, EncyclopediaUI manager)
    {
        enemy = data;
        encyclopedia = manager;

        icon.sprite = enemy.iconSprite;

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        encyclopedia.ShowEnemy(enemy);
    }
}