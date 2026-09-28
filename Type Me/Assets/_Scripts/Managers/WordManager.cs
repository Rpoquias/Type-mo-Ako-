using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WordManager : MonoBehaviour
{
    public static WordManager Instance { get; private set; }

    public event Action<Enemy> OnCorrectLetterTyped;
    public event Action<ITypeableWord> OnFocusChanged;

    [Header("Targeting Settings")]
    [SerializeField] private float maxTargetingDistance = 15f;
    [SerializeField] private float directionWeight = 0.3f;

    private readonly List<ITypeableWord> _activeWords = new List<ITypeableWord>();
    private ITypeableWord _currentFocusedWord;
    private ITypeableWord _previousFocusedWord;

    public ITypeableWord CurrentFocusedWord => _currentFocusedWord;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void RegisterWord(ITypeableWord wordHandler)
    {
        if (!_activeWords.Contains(wordHandler))
            _activeWords.Add(wordHandler);
    }

    public void UnregisterWord(ITypeableWord wordHandler)
    {
        if (_currentFocusedWord == wordHandler)
        {
            SetFocusedWord(null);
        }
        _activeWords.Remove(wordHandler);
    }

    private ITypeableWord GetNearestEnemyWithLetter(char inputChar)
    {
        if (_activeWords.Count == 0) return null;

        Vector3 playerPos = GetPlayerPosition();
        if (playerPos == Vector3.zero) return null;

        var candidates = _activeWords
    .Where(wh =>
    {
        string remaining = wh.CurrentWord;
        if (string.IsNullOrEmpty(remaining)) return false;
        return remaining[0] == inputChar;
    })
            .OfType<Enemy_WordHandler>()
            .Where(ewh => ewh != null)
            .ToList();

        if (candidates.Count == 0) return null;
        if (candidates.Count == 1) return candidates[0];

        //  Sort by distance first
        candidates = candidates
            .OrderBy(c => Vector3.Distance(playerPos, c.transform.position))
            .ToList();

        // Take the nearest enemy(s)
        float nearestDist = Vector3.Distance(playerPos, candidates[0].transform.position);
        var nearestCandidates = candidates
            .Where(c => Mathf.Abs(Vector3.Distance(playerPos, c.transform.position) - nearestDist) < 0.1f)
            .ToList();

        if (nearestCandidates.Count > 1)
            return GetSmartTarget(nearestCandidates, playerPos);

        return candidates[0];
    }


    private ITypeableWord GetSmartTarget(List<Enemy_WordHandler> candidates, Vector3 playerPos)
    {
        float bestScore = float.MinValue;
        Enemy_WordHandler bestTarget = null;

        foreach (var candidate in candidates)
        {
            float score = CalculateTargetScore(candidate, playerPos);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = candidate;
            }
        }

        return bestTarget;
    }

    private float CalculateTargetScore(Enemy_WordHandler candidate, Vector3 playerPos)
    {
        Vector3 enemyPos = candidate.transform.position;
        float distance = Vector3.Distance(playerPos, enemyPos);

        if (distance > maxTargetingDistance) return float.MinValue;

        // Distance Score (closer = better)
        float distanceScore = (maxTargetingDistance - distance) / maxTargetingDistance;

        // Direction Score (enemies moving toward player = higher priority)
        float directionScore = CalculateDirectionScore(candidate, playerPos, enemyPos);

        // Threat Score (faster enemies or closer = higher priority)
        float threatScore = CalculateThreatScore(candidate, distance);

        // Word Progress Score (partially typed = higher priority)
        float progressScore = CalculateProgressScore(candidate);

        float finalScore = (distanceScore * 0.4f) +
                          (directionScore * directionWeight) +
                          (threatScore * 0.2f) +
                          (progressScore * 0.1f);

        return finalScore;
    }


    private float CalculateDirectionScore(Enemy_WordHandler candidate, Vector3 playerPos, Vector3 enemyPos)
    {
        Vector3 toPlayer = (playerPos - enemyPos).normalized;

        float dot = Vector3.Dot(Vector3.down, toPlayer);
        return (dot + 1f) * 0.5f;
    }

    private float CalculateThreatScore(Enemy_WordHandler candidate, float distance)
    {
        var enemy = candidate.GetComponent<Enemy>();
        if (enemy?.GetEnemyData() == null) return 0f;

        var enemyData = enemy.GetEnemyData();
        float speedThreat = enemyData.speed / 5f;
        float distanceThreat = (maxTargetingDistance - distance) / maxTargetingDistance;

        return (speedThreat + distanceThreat) * 0.5f;
    }

    private float CalculateProgressScore(Enemy_WordHandler candidate)
    {
        string originalWord = candidate.GetOriginalWord();
        string remainingWord = candidate.CurrentWord;

        if (string.IsNullOrEmpty(originalWord)) return 0f;

        float progress = 1f - (remainingWord.Length / (float)originalWord.Length);
        return progress;
    }

    private Vector3 GetPlayerPosition()
    {
        if (Player.Instance != null) return Player.Instance.transform.position;

        GameObject player = GameObject.FindWithTag("Player");
        return player != null ? player.transform.position : Vector3.zero;
    }

    private void SetFocusedWord(ITypeableWord newFocus)
    {
        if (_currentFocusedWord == newFocus) return;

        _previousFocusedWord = _currentFocusedWord;
        _currentFocusedWord = newFocus;

        if (_previousFocusedWord is Enemy_WordHandler prevEnemyWh)
        {
            prevEnemyWh.SetFocused(false);
        }

        if (_currentFocusedWord is Enemy_WordHandler newEnemyWh)
        {
            newEnemyWh.SetFocused(true);
        }

        OnFocusChanged?.Invoke(_currentFocusedWord);
    }

    public void ProcessKey(char inputChar)
    {
        bool typedCorrectly = false;

        // If no focus, try to lock onto the nearest valid enemy
        if (_currentFocusedWord == null)
        {
            var newFocus = GetNearestEnemyWithLetter(inputChar);
            SetFocusedWord(newFocus);
        }

        // If we already have a focus, check typing
        if (_currentFocusedWord != null)
        {
            string remaining = _currentFocusedWord.CurrentWord;

            if (!string.IsNullOrEmpty(remaining) &&
              remaining[0] == inputChar)
            {
                // ✅ Correct letter
                _currentFocusedWord.AdvanceLetter();

                if (_currentFocusedWord is Enemy_WordHandler enemyWh)
                {
                    OnCorrectLetterTyped?.Invoke(enemyWh.GetComponent<Enemy>());
                }

                typedCorrectly = true;

                // Word completed → remove and unlock
                if (string.IsNullOrEmpty(_currentFocusedWord.CurrentWord))
                {
                    UnregisterWord(_currentFocusedWord);
                    SetFocusedWord(null);
                }
            }
        }

        if (!typedCorrectly)
        {
            if (_currentFocusedWord != null)
            {
                var temp = _currentFocusedWord;
                SetFocusedWord(null);
                temp.MistypeWord(0.5f);
            }
        }


    }


    public void SelectEnemy(Enemy_WordHandler enemyWordHandler)
    {
        if (_activeWords.Contains(enemyWordHandler))
        {
            SetFocusedWord(enemyWordHandler);
        }
    }
}
