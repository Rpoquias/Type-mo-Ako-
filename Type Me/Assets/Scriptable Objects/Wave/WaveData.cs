using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    [Header("Enemy Configuration")]
    public Enemy_Type enemyType;
    public float spawnInterval;
    public int enemiesPerWave;


}