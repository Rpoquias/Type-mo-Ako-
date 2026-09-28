using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private int maxEnemiesPerWave = 30;

    [System.Serializable]
    public struct EnemyDifficultyMapping
    {
        public GameObject enemyPrefab;
        public WordDifficulty difficulty;
    }


    public static event Action<Enemy> OnEnemySpawned;
    [SerializeField] private WordBank wordBank;
    [SerializeField] private List<EnemyDifficultyMapping> enemyMappings;


    public static SpawnManager Instance { get; private set; }

    public static event Action<int> OnWaveChanged;


    [Header("Wave Configuration")]
    [SerializeField] private WaveData[] waves;
    private WaveData[] _runtimeWaves;
    private int _currentWaveIndex = 0;
    private int _waveCounter = 0;

    [Header("Spawn Points")]
    [SerializeField] private GameObject[] spawnPoints;

    private WaveData CurrentWave => _runtimeWaves[_currentWaveIndex];

    private bool _isSpawning = true;
    private float _spawnTimer;
    private int _spawnCounter;
    private int _enemiesRemoved;
    private int _loopCount = 0; // how many times all waves have finished


    [Header("Object Pools")]
    [SerializeField] private ObjectPooler normalEnemyPool;
    [SerializeField] private ObjectPooler fastEnemyPool;
    [SerializeField] private ObjectPooler eliteEnemyPool;
    [SerializeField] private ObjectPooler bossEnemyPool;

    private Dictionary<Enemy_Type, ObjectPooler> _poolDictionary;

    [Header("Wave Settings")]
    private float _timeBetweenWaves = 2f;
    private float _waveCooldown;
    private bool _isBetweenWaves = false;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        _poolDictionary = new Dictionary<Enemy_Type, ObjectPooler>()
        {
            { Enemy_Type.NormalEnemy, normalEnemyPool },
            { Enemy_Type.FastEnemy, fastEnemyPool },
            { Enemy_Type.EliteEnemy, eliteEnemyPool },
            { Enemy_Type.BossEnemy, bossEnemyPool },
        };
        ValidateSpawnPoints();

        _runtimeWaves = new WaveData[waves.Length];
        for (int i = 0; i < waves.Length; i++)
        {
            _runtimeWaves[i] = Instantiate(waves[i]);
        }
    }
    private void ValidateSpawnPoints()
    {
        // Remove any null spawn points
        List<GameObject> validSpawnPoints = new List<GameObject>();

        if (spawnPoints != null)
        {
            foreach (GameObject point in spawnPoints)
            {
                if (point != null)
                {
                    validSpawnPoints.Add(point);
                }
            }
        }

        spawnPoints = validSpawnPoints.ToArray();

        // If no spawn points are set, use the spawner's transform as default
        if (spawnPoints.Length == 0)
        {
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (spawnPoints.Length == 0)
        {
            // Fallback to spawner's position
            return transform.position;
        }
        else if (spawnPoints.Length == 1)
        {
            // Only one spawn point, use it
            return spawnPoints[0].transform.position;
        }
        else
        {
            // Multiple spawn points, choose randomly
            int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
            return spawnPoints[randomIndex].transform.position;
        }
    }

    private void OnEnable()
    {
        Enemy.OnEnemyReachedPlayer += HandleEnemyReachedPlayer;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;

        if (Application.isPlaying && enabled)
        {
            StartSpawning();
        }
    }

    private void OnDisable()
    {
        Enemy.OnEnemyReachedPlayer -= HandleEnemyReachedPlayer;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }
    private void Start()
    {
        OnWaveChanged?.Invoke(_waveCounter);
    }

    void Update()
    {
        if (!_isSpawning) return;

        if (_isBetweenWaves)
        {
            _waveCooldown -= Time.deltaTime;
            if (_waveCooldown <= 0f)
            {
                _currentWaveIndex++;

                if (_currentWaveIndex >= _runtimeWaves.Length)
                {
                    _currentWaveIndex = 0;
                    _loopCount++;
                    ScaleDifficulty();
                }

                _waveCounter++;
                OnWaveChanged?.Invoke(_currentWaveIndex);

                _spawnCounter = 0;
                _enemiesRemoved = 0;
                _spawnTimer = 0f;
                _isBetweenWaves = false;
            }
        }
        else
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0 && _spawnCounter < CurrentWave.enemiesPerWave)
            {
                _spawnTimer = CurrentWave.spawnInterval;
                SpawnEnemy();
                _spawnCounter++;
            }
            else if (_spawnCounter >= CurrentWave.enemiesPerWave && _enemiesRemoved >= CurrentWave.enemiesPerWave)
            {
                _isBetweenWaves = true;
                _waveCooldown = _timeBetweenWaves;
            }
        }
    }


    private void ScaleDifficulty()
    {
        foreach (var wave in _runtimeWaves)
        {
            wave.enemiesPerWave = Mathf.Min(wave.enemiesPerWave + 2, maxEnemiesPerWave);
            wave.spawnInterval = Mathf.Max(0.5f, wave.spawnInterval - 0.1f);
        }
    }
    public void StartSpawning()
    {
        ResetSpawningState();
        _isSpawning = true;
        OnWaveChanged?.Invoke(_waveCounter);
    }
    private void ResetSpawningState()
    {
        _currentWaveIndex = 0;
        _waveCounter = 0;
        _spawnCounter = 0;
        _enemiesRemoved = 0;
        _spawnTimer = 0f;
        _isBetweenWaves = false;
        _waveCooldown = 0f;
        _loopCount = 0;
    }

    public void StopSpawning()
    {
        _isSpawning = false;
        _isBetweenWaves = true;
        _waveCooldown = float.MaxValue;
    }

    private void SpawnEnemy()
    {
        if (_poolDictionary.TryGetValue(CurrentWave.enemyType, out var pool))
        {
            GameObject spawnedObject = pool.GetPooledObject();


            if (spawnedObject == null)
            {
                Debug.LogWarning($"No available objects in pool for {CurrentWave.enemyType}");
                return;
            }

            Vector3 spawnPosition = GetRandomSpawnPosition();
            spawnedObject.transform.position = spawnPosition;

            Enemy enemy = spawnedObject.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.LogError($"Spawned object {spawnedObject.name} doesn't have Enemy component!");
                return;
            }


            Vector2 dirToPlayer = (Vector2)Player.Instance.transform.position - (Vector2)spawnPosition;
            enemy.GetComponent<Enemy_Movement>()?.SetDirection(dirToPlayer);

            WordDifficulty wordDifficulty = GetEnemyWordDifficulty(enemy);
            string word = wordBank.GetRandomWord(wordDifficulty);


            spawnedObject.SetActive(true);
            enemy.AssignWord(word);
            enemy.Initialize();

            OnEnemySpawned?.Invoke(enemy);
        }
        else
        {
            Debug.LogError($"No pool found for enemy type: {CurrentWave.enemyType}");
        }
    }




    private void HandleEnemyReachedPlayer(Enemy enemy)
    {
        _enemiesRemoved++;

        var wordHandler = enemy.GetComponent<Enemy_WordHandler>();
        if (wordHandler != null)
            WordManager.Instance.UnregisterWord(wordHandler);
    }


    private void HandleEnemyDestroyed(Enemy enemy)
    {
        _enemiesRemoved++;

        var wordHandler = enemy.GetComponent<Enemy_WordHandler>();
        if (wordHandler != null)
            WordManager.Instance.UnregisterWord(wordHandler);
    }




    // Helper method to add spawn points at runtime if needed
    public void AddSpawnPoint(GameObject spawnPoint)
    {
        if (spawnPoint != null)
        {
            List<GameObject> pointsList = new List<GameObject>(spawnPoints);
            pointsList.Add(spawnPoint);
            spawnPoints = pointsList.ToArray();
        }
    }


    private WordDifficulty GetEnemyWordDifficulty(Enemy enemy)
    {

        var enemyData = enemy.GetEnemyData();

        if (enemyData != null)
        {
            return enemyData.wordDifficulty;
        }

        return WordDifficulty.Normal;
    }


    public void RemoveSpawnPoint(GameObject spawnPoint)
    {
        if (spawnPoint != null)
        {
            List<GameObject> pointsList = new List<GameObject>(spawnPoints);
            pointsList.Remove(spawnPoint);
            spawnPoints = pointsList.ToArray();
        }
    }



}