using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AudioButton : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip customClickSound;  // Optional override
    [SerializeField] private bool playClickSound = true;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null && playClickSound)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance == null) return;

        // Use custom sound if provided, otherwise use default
        AudioClip soundToPlay = customClickSound != null ? customClickSound : AudioManager.Instance.buttonClickSFX;
        AudioManager.Instance.PlaySFX(soundToPlay);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }

    // Public method to disable click sound for specific buttons
    public void SetClickSoundEnabled(bool enabled)
    {
        playClickSound = enabled;
    }
}
