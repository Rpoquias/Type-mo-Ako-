using System;
using UnityEngine;

public class Enemy_Movement : MonoBehaviour
{
    public event Action<Vector2> OnMovement;

    private bool _isMoving = false;
    private bool _isForceDisabled = false; // NEW: For knockback override
    private Transform _player;
    private float _speed;

    private void Update()
    {
        // IMPORTANT: Check force disabled first
        if (_player == null || !_isMoving || _isForceDisabled) return;

        // Calculate 2D direction
        Vector2 direction = ((Vector2)_player.position - (Vector2)transform.position).normalized;

        // Move enemy
        transform.position = Vector2.MoveTowards(transform.position, _player.position, _speed * Time.deltaTime);

        // Send direction to visuals
        OnMovement?.Invoke(direction);
    }

    public void SetDirection(Vector2 direction)
    {
        Vector2 dominantDir = GetDominantDirection(direction);
        OnMovement?.Invoke(dominantDir);
    }

    private Vector2 GetDominantDirection(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return new Vector2(Mathf.Sign(dir.x), 0);
        else
            return new Vector2(0, Mathf.Sign(dir.y));
    }

    public void Initialize(float moveSpeed)
    {
        _player = Player.Instance.transform;
        _speed = moveSpeed;
        _isMoving = true;
        _isForceDisabled = false; // Reset on initialize
    }

    public void StopMoving()
    {
        _isMoving = false;
        OnMovement?.Invoke(Vector2.zero);
    }

    public void StartMoving()
    {
        if (!_isForceDisabled) // Only start if not force disabled
        {
            _isMoving = true;
        }
    }

    // NEW: Force disable during knockback
    public void ForceDisable()
    {
        _isForceDisabled = true;
        _isMoving = false;
        OnMovement?.Invoke(Vector2.zero);
    }

    // NEW: Re-enable after knockback
    public void ForceEnable()
    {
        _isForceDisabled = false;
        _isMoving = true;
    }

    // NEW: Check if movement is available
    public bool CanMove()
    {
        return !_isForceDisabled && _isMoving;
    }
}
