using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    [Header("Level Music Configuration")]
    [SerializeField] private LevelMusicConfig levelMusicConfig;

    [Header("BGM Settings")]
    [SerializeField] private float defaultVolume = 1f;
    [SerializeField] private bool fadeTransitions = true;
    [SerializeField] private float fadeSpeed = 2f;

    private AudioSource audioSource;
    private const string BGM_VOLUME_KEY = "BGMVolume";

    public static BGMManager Instance { get; private set; }

    #region Initialization
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSource();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        LoadAndPlayCurrentLevelMusic();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        ConfigureAudioSource();
    }

    private void ConfigureAudioSource()
    {
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        LoadSavedVolume();
    }

    private void LoadSavedVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, defaultVolume);
        SetBGMVolume(savedVolume);
    }
    #endregion

    #region Scene Management
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadAndPlayCurrentLevelMusic();
    }

    private void LoadAndPlayCurrentLevelMusic()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayMusicForLevel(currentSceneName);
    }

    public void PlayMusicForLevel(string sceneName)
    {
        if (levelMusicConfig == null)
        {
            return;
        }

        var levelMusic = levelMusicConfig.GetMusicForScene(sceneName);

        if (levelMusic != null && levelMusic.backgroundMusic != null)
        {
            PlayBGM(levelMusic.backgroundMusic, levelMusic.volume, levelMusic.loopMusic);
        }
        else if (levelMusicConfig.defaultBGM != null)
        {
            PlayBGM(levelMusicConfig.defaultBGM, levelMusicConfig.defaultVolume, true);
        }
        else
        {
        }
    }
    #endregion

    #region BGM Control
    public void PlayBGM(AudioClip clip, float volumeMultiplier = 1f, bool loop = true)
    {
        if (clip == null || audioSource == null) return;

        if (fadeTransitions && audioSource.isPlaying)
        {
            StartCoroutine(FadeToNewMusic(clip, volumeMultiplier, loop));
        }
        else
        {
            StartBGM(clip, volumeMultiplier, loop);
        }
    }

    private void StartBGM(AudioClip clip, float volumeMultiplier, bool loop)
    {
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.volume = GetBGMVolume() * volumeMultiplier;
        audioSource.Play();
    }

    private System.Collections.IEnumerator FadeToNewMusic(AudioClip newClip, float volumeMultiplier, bool loop)
    {
        float targetVolume = GetBGMVolume() * volumeMultiplier;

        // Fade out current music
        while (audioSource.volume > 0.01f)
        {
            audioSource.volume -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        // Switch to new music
        StartBGM(newClip, volumeMultiplier, loop);
        audioSource.volume = 0f;

        // Fade in new music
        while (audioSource.volume < targetVolume - 0.01f)
        {
            audioSource.volume += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    public void StopBGM()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void PauseBGM()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (audioSource != null)
        {
            audioSource.UnPause();
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume);
        }
    }

    public float GetBGMVolume()
    {
        return audioSource != null ? audioSource.volume : defaultVolume;
    }

    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }
    #endregion

    #region Debug
    [ContextMenu("Play Current Level Music")]
    public void PlayCurrentLevelMusicDebug()
    {
        LoadAndPlayCurrentLevelMusic();
    }
    #endregion
}
