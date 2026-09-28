// File: /Assets/_Scripts/UTILS/Configuration/GameSceneConfiguration.cs
using UnityEngine;

[CreateAssetMenu(fileName = "GameSceneConfiguration", menuName = "Configuration/Game Scene Configuration")]
public class GameSceneConfiguration : ScriptableObject
{
    [System.Serializable]
    public class GameplaySceneInfo
    {
        [Header("Scene Info")]
        public string sceneName;
        public bool forceCountdown = false;
        public int defaultCountdownDuration = 3;

        [Header("Audio")]
        public bool hasBGM = true;
    }

    [Header("Gameplay Scenes")]
    public GameplaySceneInfo[] gameplayScenes;

    public GameplaySceneInfo GetGameplaySceneInfo(string sceneName)
    {
        foreach (var scene in gameplayScenes)
        {
            if (scene.sceneName == sceneName)
                return scene;
        }
        return null;
    }

    public bool IsGameplayScene(string sceneName)
    {
        return GetGameplaySceneInfo(sceneName) != null;
    }

    public bool IsBootstrapScene(string sceneName)
    {
        return sceneName == "Bootstrap";
    }

    public bool IsMainMenuScene(string sceneName)
    {
        return sceneName == "MainMenu";
    }
}
