using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("SFX Clips")]
    public AudioClip buttonClickSFX;
    public AudioClip shootingSFX;
    public AudioClip loseSFX;
    public AudioClip enemyHitSFX;
    public AudioClip playerHurtSFX;
    public AudioClip defaultDeathSFX;


    [Header("BGM Configuration")]
    [SerializeField] private LevelMusicConfig levelMusicConfig;
    [SerializeField] private bool fadeTransitions = true;
    [SerializeField] private float fadeSpeed = 2f;

    [Header("Game Over Audio Settings")]
    [SerializeField] private float bgmFadeOutSpeed = 3f;
    [SerializeField] private bool enableGameOverSilence = true;

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;

    // Audio Sources
    private AudioSource sfxAudioSource;
    private AudioSource bgmAudioSource;
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private int poolSize = 5;

    // Game Over State
    private bool isGameOverAudioPlaying = false;
    private float originalBGMVolume;

    // PlayerPrefs keys
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string BGM_VOLUME_KEY = "BGMVolume";

    #region Initialization
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
            InitializeAudioPool();
            LoadSavedVolumes();
        }
        else
        {
            Destroy(gameObject);
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

    private void SetupAudioSources()
    {
        // Create SFX Audio Source
        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource.volume = sfxVolume;
        sfxAudioSource.playOnAwake = false;

        // Create BGM Audio Source
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        bgmAudioSource.volume = bgmVolume;
        bgmAudioSource.loop = true;
        bgmAudioSource.playOnAwake = false;
    }

    private void InitializeAudioPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject audioObj = new GameObject($"AudioSource_Pool_{i}");
            audioObj.transform.SetParent(transform);
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.volume = sfxVolume;
            audioSourcePool.Enqueue(source);
        }
    }

    private void LoadSavedVolumes()
    {
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
        originalBGMVolume = bgmVolume;

        SetSFXVolume(sfxVolume);
        SetBGMVolume(bgmVolume);
    }
    #endregion

    #region Scene Management
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetGameOverAudioState();


        if (scene.name == "MainMenu")
        {
            LoadAndPlayCurrentLevelMusic();
        }

    }

    public void LoadAndPlayCurrentLevelMusic()
    {
        // Don't play BGM if game over audio is active
        if (isGameOverAudioPlaying) return;

        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayMusicForLevel(currentSceneName);
    }


    public void PlayMusicForLevel(string sceneName)
    {
        // Don't play BGM if game over audio is active
        if (isGameOverAudioPlaying) return;

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

    }
    #endregion

    #region SFX Methods
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableAudioSource();
        source.volume = sfxVolume * volumeScale;
        source.PlayOneShot(clip);
    }

    public void PlayButtonClickSFX()
    {
        PlaySFX(buttonClickSFX);
    }

    public void PlayShootingSFX()
    {
        PlaySFX(shootingSFX);
    }

    public void PlayLoseSFX(float volumeScale = 1f)
    {
        if (loseSFX != null)
        {
            // ✅ START GAME OVER AUDIO SEQUENCE
            StartCoroutine(GameOverAudioSequence(volumeScale));
        }

    }

    public void PlayEnemyHit()
    {
        PlaySFX(enemyHitSFX);
    }
    public void PlayPlayerHurt()
    {
        PlaySFX(playerHurtSFX);
    }

    public void PlayEnemyDeath(AudioClip customDeathSound = null)
    {
        AudioClip soundToPlay = customDeathSound != null ? customDeathSound : defaultDeathSFX;
        PlaySFX(soundToPlay);
    }


    private AudioSource GetAvailableAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            AudioSource source = audioSourcePool.Dequeue();
            StartCoroutine(ReturnToPool(source, 2f));
            return source;
        }
        return sfxAudioSource; // Fallback to main SFX source
    }

    private IEnumerator ReturnToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (source != null)
        {
            audioSourcePool.Enqueue(source);
        }
    }
    #endregion

    #region Game Over Audio Sequence
    private IEnumerator GameOverAudioSequence(float loseSFXVolumeScale = 1f)
    {
        isGameOverAudioPlaying = true;


        // ✅ STEP 1: Fade out BGM quickly
        yield return StartCoroutine(FadeOutBGM());

        // ✅ STEP 2: Play lose SFX
        PlaySFX(loseSFX, loseSFXVolumeScale);

        // ✅ STEP 3: Wait for lose SFX to finish + extra silence
        if (loseSFX != null)
        {
            float loseSFXDuration = loseSFX.length;
            yield return new WaitForSecondsRealtime(loseSFXDuration);
        }

        // ✅ STEP 4: Maintain silence (BGM won't restart until scene change)
        if (enableGameOverSilence)
        {
        }
    }

    private IEnumerator FadeOutBGM()
    {
        if (bgmAudioSource == null || !bgmAudioSource.isPlaying) yield break;

        float startVolume = bgmAudioSource.volume;

        while (bgmAudioSource.volume > 0.01f)
        {
            bgmAudioSource.volume -= Time.unscaledDeltaTime * bgmFadeOutSpeed;
            yield return null;
        }

        bgmAudioSource.volume = 0f;
        bgmAudioSource.Stop();

    }

    public void ResetGameOverAudioState()
    {
        isGameOverAudioPlaying = false;

        // Restore original BGM volume
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = originalBGMVolume;
        }

    }

    public bool IsGameOverAudioActive()
    {
        return isGameOverAudioPlaying;
    }
    #endregion

    #region BGM Methods
    public void PlayBGM(AudioClip clip, float volumeMultiplier = 1f, bool loop = true)
    {
        if (clip == null || bgmAudioSource == null) return;

        // Don't play BGM during game over sequence
        if (isGameOverAudioPlaying)
        {
            return;
        }

        if (fadeTransitions && bgmAudioSource.isPlaying)
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
        bgmAudioSource.clip = clip;
        bgmAudioSource.loop = loop;
        bgmAudioSource.volume = bgmVolume * volumeMultiplier;
        bgmAudioSource.Play();
    }

    private IEnumerator FadeToNewMusic(AudioClip newClip, float volumeMultiplier, bool loop)
    {
        float targetVolume = bgmVolume * volumeMultiplier;

        // Fade out current music
        while (bgmAudioSource.volume > 0.01f)
        {
            bgmAudioSource.volume -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        // Switch to new music
        StartBGM(newClip, volumeMultiplier, loop);
        bgmAudioSource.volume = 0f;

        // Fade in new music
        while (bgmAudioSource.volume < targetVolume - 0.01f)
        {
            bgmAudioSource.volume += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        bgmAudioSource.volume = targetVolume;
    }

    public void StopBGM()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }
    }

    public void PauseBGM()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (bgmAudioSource != null && !isGameOverAudioPlaying)
        {
            bgmAudioSource.UnPause();
        }
    }

    // ✅ NEW: Force restart BGM (for debugging or special cases)
    public void ForceRestartBGM()
    {
        ResetGameOverAudioState();
        LoadAndPlayCurrentLevelMusic();
    }
    #endregion

    #region Volume Control
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        // Update main SFX source
        if (sfxAudioSource != null)
            sfxAudioSource.volume = sfxVolume;

        // Update pooled sources
        foreach (AudioSource pooledSource in audioSourcePool)
        {
            pooledSource.volume = sfxVolume;
        }

        // Save to PlayerPrefs
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        originalBGMVolume = bgmVolume;

        if (bgmAudioSource != null && !isGameOverAudioPlaying)
            bgmAudioSource.volume = bgmVolume;

        // Save to PlayerPrefs
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, bgmVolume);
        PlayerPrefs.Save();
    }


    public float GetSFXVolume()
    {
        return sfxVolume;
    }


    public float GetBGMVolume()
    {
        return bgmVolume;
    }
    #endregion

    #region Utility
    public bool IsBGMPlaying()
    {
        return bgmAudioSource != null && bgmAudioSource.isPlaying;
    }

    [ContextMenu("Play Current Level Music")]
    public void PlayCurrentLevelMusicDebug()
    {
        LoadAndPlayCurrentLevelMusic();
    }

    [ContextMenu("Test Game Over Audio")]
    public void TestGameOverAudio()
    {
        PlayLoseSFX();
    }


    [ContextMenu("Reset Game Over State")]
    public void ResetGameOverStateDebug()
    {
        ResetGameOverAudioState();
    }
    #endregion
}
