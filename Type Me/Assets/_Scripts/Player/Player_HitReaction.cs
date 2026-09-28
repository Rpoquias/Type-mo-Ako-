using System.Collections;
using UnityEngine;

public class Player_HitReaction : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material hitFlashMaterial;
    [SerializeField] private float flashDuration = 0.15f;

    private Material _originalMaterial;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        _originalMaterial = spriteRenderer.material;
    }

    public void PlayHurtReaction()
    {
        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);

        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.material = hitFlashMaterial;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.material = _originalMaterial;
        _flashCoroutine = null;
    }
}