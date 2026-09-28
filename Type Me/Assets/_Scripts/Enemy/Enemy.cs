using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static event Action<Enemy> OnEnemyReachedPlayer;
    public static event Action<Enemy> OnEnemyDestroyed;

    [SerializeField] private EnemyData enemyData;

    private Enemy_Movement _movement;
    private Enemy_Visuals _visuals;
    private Enemy_WordHandler _wordHandler;
    private Enemy_HitReaction _hitReaction;

    private bool _isMarkedForDeath = false;
    private int _projectilesTargeting = 0;

    public Enemy_Visuals GetVisuals() => _visuals;

    private void Awake()
    {
        _movement = GetComponent<Enemy_Movement>();
        _visuals = GetComponent<Enemy_Visuals>();
        _wordHandler = GetComponent<Enemy_WordHandler>();
        _hitReaction = GetComponent<Enemy_HitReaction>();
    }

    private void OnEnable()
    {
        _isMarkedForDeath = false;
        _projectilesTargeting = 0;

        if (_movement != null)
            _movement.OnMovement += _visuals.UpdateMovementAnimation;

        _visuals?.ResetVisuals();
    }

    private void OnDisable()
    {
        if (_movement != null)
            _movement.OnMovement -= _visuals.UpdateMovementAnimation;

        CancelInvoke(); // prevents a stale DisableReachedEnemy() firing after this object is reused from a pool
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        _movement.Initialize(enemyData.speed);
    }

    public void OnWordCompleted()
    {
        _movement.StopMoving();
        _isMarkedForDeath = true;

        // If no projectiles are coming, play death effect immediately
        if (_projectilesTargeting <= 0)
        {
            _visuals.PlayDeathAnimation(transform.position);
            StartCoroutine(DelayedDisable());
        }
    }

    public EnemyData GetEnemyData() => enemyData;

    public void AssignWord(string word)
    {
        _wordHandler.SetWord(word);
    }

    public void AddProjectileTarget()
    {
        _projectilesTargeting++;
    }

    public void RemoveProjectileTarget()
    {
        _projectilesTargeting--;

        // If marked for death and this was the last projectile, play death effect and disable
        if (_isMarkedForDeath && _projectilesTargeting <= 0)
        {
            _visuals.PlayDeathAnimation(transform.position);
            StartCoroutine(DelayedDisable());
        }
    }

    private IEnumerator DelayedDisable()
    {
        yield return new WaitForSeconds(0.1f);
        DisableEnemy();
    }

    public bool IsMarkedForDeath() => _isMarkedForDeath;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bool isReacting = _hitReaction != null && _hitReaction.IsReacting();
            if (!isReacting)
                _movement.StopMoving();

            OnEnemyReachedPlayer?.Invoke(this);
            Invoke(nameof(DisableReachedEnemy), 0.5f);
        }
    }

    private void DisableReachedEnemy()
    {
        // Don't fire OnEnemyDestroyed for enemies that reached player
        gameObject.SetActive(false);
    }

    public void HandleProjectileHit(Vector2 projectileDirection, Vector3 hitPosition)
    {
        if (_hitReaction == null)
        {
            Debug.LogWarning($"{name} is missing an Enemy_HitReaction component.", this);
            return;
        }

        _hitReaction.HandleProjectileHit(projectileDirection, hitPosition);
    }

    private void DisableEnemy()
    {
        OnEnemyDestroyed?.Invoke(this);
        gameObject.SetActive(false);
    }
}