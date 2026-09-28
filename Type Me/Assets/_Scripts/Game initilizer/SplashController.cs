using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashController : MonoBehaviour
{
    [SerializeField] private CanvasGroup logoCanvasGroup;
    [SerializeField] private string nextScene = "MainMenu";

    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float holdDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1.5f;

    private void Start()
    {
        StartCoroutine(PlaySplashSequence());
    }

    private IEnumerator PlaySplashSequence()
    {
        // Fade In
        yield return StartCoroutine(Fade(0f, 1f, fadeInDuration));

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Fade Out
        yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration));

        // Load Main Menu
        SceneManager.LoadScene(nextScene);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            logoCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        logoCanvasGroup.alpha = to;
    }
}
