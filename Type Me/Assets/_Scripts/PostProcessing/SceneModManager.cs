using UnityEngine;
using UnityEngine.Rendering;

public class SceneMoodManager : MonoBehaviour
{
    [Header("URP Volume Profiles")]
    [SerializeField] private VolumeProfile dayProfile;
    [SerializeField] private VolumeProfile sunsetProfile;
    [SerializeField] private VolumeProfile nightProfile;


    private Volume globalVolume;

    private void Awake()
    {
        globalVolume = GetComponent<Volume>();
        if (globalVolume == null)
        {
            globalVolume = gameObject.AddComponent<Volume>();
            globalVolume.isGlobal = true;
        }
    }

    private void Start()
    {
        SetMoodForCurrentScene();

     
    }

    private void SetMoodForCurrentScene()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentScene.Contains("Easy"))
        {
            ApplyProfile(dayProfile, "URP Day mood applied");
        }
        else if (currentScene.Contains("Normal"))
        {
            ApplyProfile(sunsetProfile, "URP Sunset mood applied");
        }
        else if (currentScene.Contains("Hard"))
        {
            ApplyProfile(nightProfile, "URP Night mood applied");
        }
        else
        {
            ApplyProfile(dayProfile, "URP Default day mood applied");
        }
    }

    private void ApplyProfile(VolumeProfile profile, string logMessage)
    {
        if (profile != null && globalVolume != null)
        {
            globalVolume.profile = profile;
        }
        else
        {
        }
    }

  
    // Public methods for manual control
    public void ApplyDayMood() => ApplyProfile(dayProfile, "URP Day mood manually applied");
    public void ApplyNightMood() => ApplyProfile(nightProfile, "URP Night mood manually applied");
    public void ApplySunsetMood() => ApplyProfile(sunsetProfile, "URP Sunset mood manually applied");
}
