using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{

    [SerializeField] private Player_HitReaction hitReaction;
    private int currentHealth;
    private int maxHealth;
    private bool isDead = false;

    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action OnPlayerDied;

    private void Awake()
    {
        // Get health from SessionManager or use fallback
        if (SessionManager.Instance?.CurrentLevel != null)
        {
            maxHealth = SessionManager.Instance.CurrentLevel.startingLives;
        }
        else
        {
            maxHealth = 3; // fallback
        }

        currentHealth = maxHealth;
        isDead = false;

    }

    private void OnEnable()
    {
        Enemy.OnEnemyReachedPlayer += HandleEnemyReachedPlayer;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyReachedPlayer -= HandleEnemyReachedPlayer;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void HandleEnemyReachedPlayer(Enemy enemy)
    {
        if (isDead) return;

        EnemyData enemyData = enemy.GetEnemyData();
        int damage = enemyData?.damage ?? 1;

        TakeDamage(damage);
    }

    public void TakeDamage(int amount)
    {
        if (isDead || amount <= 0) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPlayerHurt();

        hitReaction?.PlayHurtReaction();

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;


        if (OnPlayerDied != null)
        {
            int listenerCount = OnPlayerDied.GetInvocationList().Length;
            OnPlayerDied.Invoke();
        }

    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;





}