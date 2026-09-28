using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
    [Header("Scoring Rules")]
    [SerializeField] private float reachedPlayerPenalty = 0.5f; // 50% penalty

    private void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
        Enemy.OnEnemyReachedPlayer += HandleEnemyReachedPlayer;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
        Enemy.OnEnemyReachedPlayer -= HandleEnemyReachedPlayer;
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        AwardFullScore(enemy);
    }

    private void HandleEnemyReachedPlayer(Enemy enemy)
    {
        AwardPenaltyScore(enemy);
    }

    private void AwardFullScore(Enemy enemy)
    {
        EnemyData data = enemy.GetEnemyData();
        if (data != null && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(data.wordScore, enemy.transform.position);
        }
    }

    private void AwardPenaltyScore(Enemy enemy)
    {
        EnemyData data = enemy.GetEnemyData();
        if (data != null && ScoreManager.Instance != null)
        {
            int penaltyScore = Mathf.RoundToInt(data.wordScore * reachedPlayerPenalty);
            ScoreManager.Instance.AddScore(penaltyScore, enemy.transform.position);
        }
    }
}
