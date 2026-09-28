using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Basic Info")]
    public string levelName;
    public string description;
    public int startingLives;

    [Header("Level Start Settings")]
    public bool requiresCountdown = false;
    [Range(3, 10)] public int countdownDuration = 3;

    [Header("Visual Theme")]
    public Sprite bannerSprite; // From previous enhancement
    public Sprite levelmap;
}
