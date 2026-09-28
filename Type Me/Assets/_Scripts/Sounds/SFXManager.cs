using UnityEngine;
using System.Collections;

public class SFXManager : MonoBehaviour
{
    void Start()
    {
        
        StartCoroutine(InitializeAudioManager());
    }

    private IEnumerator InitializeAudioManager()
    {
     
        while (AudioManager.Instance == null)
        {
        
            Debug.Log("Waiting for AudioManager to be initialized...");
            yield return null; // Wait for the next frame and keep checking
        }

  
        Debug.Log("AudioManager is initialized. You can now play SFX.");
    }

    // Public method to play button click sound
    public void PlayClickSFX()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("❌ AudioManager is missing from the scene!");
            return;
        }

        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSFX);
    }
}
