using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CreditsScroller : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private float scrollSpeed = 80f;

    private bool isScrolling;

    private void OnEnable()
    {
        StartCoroutine(StartScroll());
    }

    private void OnDisable()
    {
        isScrolling = false;
        StopAllCoroutines();
    }

    private IEnumerator StartScroll()
    {
        isScrolling = false;

        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();

        // Place content just below the visible area
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, -viewport.rect.height);

        isScrolling = true;
    }

    private void Update()
    {
        if (!isScrolling) return;

        content.anchoredPosition += new Vector2(0f, scrollSpeed * Time.deltaTime);

        // Read height live — stops once the bottom of the text exits through the top
        if (content.anchoredPosition.y >= content.rect.height)
            isScrolling = false;
    }
}
