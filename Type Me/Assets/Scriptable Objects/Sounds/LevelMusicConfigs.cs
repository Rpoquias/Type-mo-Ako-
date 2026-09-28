using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "LevelMusicConfig", menuName = "Audio/Level Music Configuration")]
public class LevelMusicConfig : ScriptableObject
{
    [System.Serializable]
    public class LevelMusic
    {
        [Header("Scene Information")]
        public string sceneName;
        [TextArea(2, 3)]
        public string description;

        [Header("Music")]
        public AudioClip backgroundMusic;

        [Header("Settings")]
        [Range(0f, 1f)]
        public float volume = 1f;
        public bool loopMusic = true;
    }

    [Header("Level Music Configurations")]
    public LevelMusic[] levelMusics;

    [Header("Default Settings")]
    public AudioClip defaultBGM;
    [Range(0f, 1f)]
    public float defaultVolume = 1f;

    public LevelMusic GetMusicForScene(string sceneName)
    {
        foreach (var levelMusic in levelMusics)
        {
            if (levelMusic.sceneName.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return levelMusic;
            }
        }

        return null; // Return null if not found
    }
}
