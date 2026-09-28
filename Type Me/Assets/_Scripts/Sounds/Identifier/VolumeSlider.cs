using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public enum VolumeType { BGM, SFX }
    public VolumeType type;
    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();

        // Load correct saved value
        if (type == VolumeType.BGM)
            slider.SetValueWithoutNotify(AudioManager.Instance.GetBGMVolume());
        else
            slider.SetValueWithoutNotify(AudioManager.Instance.GetSFXVolume());

        // Add listener for changes
        slider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void OnVolumeChanged(float value)
    {
        if (type == VolumeType.BGM)
            AudioManager.Instance.SetBGMVolume(value);
        else
            AudioManager.Instance.SetSFXVolume(value);
    }

    void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnVolumeChanged);
    }
}
