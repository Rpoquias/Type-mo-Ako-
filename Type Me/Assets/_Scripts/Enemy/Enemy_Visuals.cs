using System.Collections;
using UnityEngine;

public class Enemy_Visuals : MonoBehaviour
{

    private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material whiteFlashMaterial;


    [SerializeField] private bool defaultFacingRight = true;


    private Material _originalMaterial;
    private Coroutine _hitFlashCoroutine;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        _originalMaterial = spriteRenderer.material;
    }

    public void UpdateMovementAnimation(Vector2 direction)
    {
        bool isMoving = direction.sqrMagnitude > 0.01f;
        animator.SetBool("isMoving", isMoving);

        if (!isMoving) return;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            animator.SetFloat("moveX", Mathf.Sign(direction.x));
            animator.SetFloat("moveY", 0f);
        }
        else
        {
            animator.SetFloat("moveX", 0f);
            animator.SetFloat("moveY", Mathf.Sign(direction.y));
        }
    }

    public void PlayDeathAnimation(Vector3 spawnPosition)
    {
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null && AudioManager.Instance != null)
        {
            EnemyData enemyData = enemy.GetEnemyData();
            AudioManager.Instance.PlayEnemyDeath(enemyData?.deathSound);
        }

        if (EffectPoolManager.Instance != null)
        {
            GameObject effect = EffectPoolManager.Instance.GetDeathEffectPool().GetPooledObject();
            effect.transform.position = spawnPosition; // enemy's exact position
            effect.transform.rotation = Quaternion.identity; // optional: reset rotation
            effect.SetActive(true);

            var pooled = effect.GetComponent<PooledEffect>();
            pooled?.SetPool(EffectPoolManager.Instance.GetDeathEffectPool());
        }
    }



    public void PlayArrowHitEffect(Vector3 spawnPosition)
    {
        if (EffectPoolManager.Instance != null)
        {
            GameObject effect = EffectPoolManager.Instance.GetArrowHitEffectPool().GetPooledObject();
            effect.transform.position = spawnPosition;
            effect.SetActive(true);

            var pooled = effect.GetComponent<PooledEffect>();
            pooled?.SetPool(EffectPoolManager.Instance.GetArrowHitEffectPool());
        }
    }

    public void PlayIdleAnimation()
    {
        animator.SetBool("isMoving", false);
    }

    public void PlayHitFlash(float duration = 0.1f)
    {
        if (_hitFlashCoroutine != null)
            StopCoroutine(_hitFlashCoroutine);

        _hitFlashCoroutine = StartCoroutine(HitFlashRoutine(duration));
    }

    private IEnumerator HitFlashRoutine(float duration)
    {
        if (!gameObject.activeInHierarchy) yield break;

        spriteRenderer.material = whiteFlashMaterial;

        float timer = 0f;
        while (timer < duration)
        {
            if (!gameObject.activeInHierarchy) yield break;
            timer += Time.deltaTime;
            yield return null;
        }

        if (gameObject.activeInHierarchy)
            spriteRenderer.material = _originalMaterial;

        _hitFlashCoroutine = null;
    }


    public void ResetVisuals()
    {
        if (_hitFlashCoroutine != null)
        {
            StopCoroutine(_hitFlashCoroutine);
            _hitFlashCoroutine = null;
        }

        spriteRenderer.material = _originalMaterial;
        animator.SetBool("isMoving", false);
        animator.SetFloat("moveX", 0f);
        animator.SetFloat("moveY", -1f); // Default facing down
    }
}
