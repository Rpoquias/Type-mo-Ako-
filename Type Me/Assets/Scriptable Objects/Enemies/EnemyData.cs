using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Properties")]
    public Sprite portraitSprite;
    public Sprite iconSprite;

    public string enemyName;
    public string description;
    public int wordScore;
    public Enemy_Type enemyType;
    public float speed = 2f;
    public int damage = 1;

    [Header("Word Properties")]
    public WordDifficulty wordDifficulty = WordDifficulty.Normal;

    [Header("Audio")]
    public AudioClip deathSound;  // Unique death sound for this enemy

    [Header("Hit Reaction Settings")]
    public HitReactionType hitReactionType = HitReactionType.Knockback;


    public float knockbackForce = 5f;
    public float knockbackDuration = 0.3f;
    public float knockbackMultiplier = 1f; // For reduced knockback on elites

    [Header("Stun Settings")]
    public float stunDuration = 0.5f;
    public Color stunFlashColor = Color.yellow;


    public float GetKnockbackForce() => knockbackForce * knockbackMultiplier;
    public float GetKnockbackDuration() => knockbackDuration;
    public float GetStunDuration() => stunDuration;
    public Color GetStunFlashColor() => stunFlashColor;

    public string GetEnemyTypeString()
    {
        switch (enemyType)
        {
            case Enemy_Type.NormalEnemy:
                return "Normal";
            case Enemy_Type.FastEnemy:
                return "Fast";
            case Enemy_Type.EliteEnemy:
                return "Elite";
            case Enemy_Type.BossEnemy:
                return "Boss";
            default:
                return "Normal";
        }
    }


    private void OnValidate()
    {
        // Auto-configure hit reactions based on enemy type
        switch (enemyType)
        {
            case Enemy_Type.NormalEnemy:
            case Enemy_Type.FastEnemy:
                hitReactionType = HitReactionType.Knockback;
                knockbackMultiplier = 1f;
                break;

            case Enemy_Type.EliteEnemy:
                hitReactionType = HitReactionType.ReducedKnockback;
                knockbackMultiplier = 0.3f; // Much less knockback
                break;

            case Enemy_Type.BossEnemy:
                hitReactionType = HitReactionType.Stun;
                knockbackMultiplier = 0f; // No knockback
                break;
        }
    }
}
