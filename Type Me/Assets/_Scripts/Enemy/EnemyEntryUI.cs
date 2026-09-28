using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyEntryUI : MonoBehaviour
{
    [Header("UI References")]
    public Image enemyImage;
    public TMP_Text enemyNameText;
    public TMP_Text enemyDescriptionText;
    public TMP_Text wordDifficultyText;
    public TMP_Text statsText;

    public void Setup(EnemyData enemy)
    {
        if (enemyImage != null && enemy.portraitSprite != null)
            enemyImage.sprite = enemy.portraitSprite; ;

        if (enemyNameText != null)
            enemyNameText.text = enemy.enemyName;

        if (enemyDescriptionText != null)
            enemyDescriptionText.text = enemy.description;

        if (wordDifficultyText != null)
            wordDifficultyText.text = $"Word Difficulty: {enemy.wordDifficulty}";

        if (statsText != null)
        {
            statsText.text =
                $"Speed: {enemy.speed}\n" +
                $"Score: +{enemy.wordScore}\n" +
                $"Damage: {enemy.damage}";
        }
    }
}
