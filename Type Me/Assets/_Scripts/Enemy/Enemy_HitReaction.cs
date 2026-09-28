using System.Collections;
using UnityEngine;

public class Enemy_HitReaction : MonoBehaviour
{
    private Enemy_Movement movement;
    private Enemy_Visuals visuals;
    private EnemyData enemyData;

    private bool isReacting = false;
    private Coroutine currentReactionCoroutine;

    private void Awake()
    {
        movement = GetComponent<Enemy_Movement>();
        visuals = GetComponent<Enemy_Visuals>();

        var enemy = GetComponent<Enemy>();
        enemyData = enemy?.GetEnemyData();
    }

    public void HandleProjectileHit(Vector2 projectileDirection, Vector3 hitPosition)
    {
        if (enemyData == null)
        {
            DefaultKnockback(projectileDirection);
            visuals?.PlayArrowHitEffect(hitPosition);
            visuals?.PlayHitFlash();
            return;
        }

        if (currentReactionCoroutine != null)
        {
            StopCoroutine(currentReactionCoroutine);
        }

        switch (enemyData.hitReactionType)
        {
            case HitReactionType.Knockback:
                currentReactionCoroutine = StartCoroutine(KnockbackReaction(projectileDirection));
                break;

            case HitReactionType.ReducedKnockback:
                currentReactionCoroutine = StartCoroutine(ReducedKnockbackReaction(projectileDirection));
                break;

            case HitReactionType.Stun:
                currentReactionCoroutine = StartCoroutine(StunReaction());
                break;

            case HitReactionType.None:
                break;
        }

        visuals?.PlayArrowHitEffect(hitPosition);
        visuals?.PlayHitFlash();
    }
    private IEnumerator KnockbackReaction(Vector2 direction)
    {
        isReacting = true;
        movement?.ForceDisable();

        float force = enemyData.GetKnockbackForce();
        float duration = enemyData.GetKnockbackDuration();

        yield return StartCoroutine(ApplyKnockback(direction, force, duration));

        movement?.ForceEnable();
        isReacting = false;
        currentReactionCoroutine = null;
    }

    private IEnumerator ReducedKnockbackReaction(Vector2 direction)
    {
        isReacting = true;
        movement?.ForceDisable();

        float force = enemyData.GetKnockbackForce();
        float duration = enemyData.GetKnockbackDuration() * 0.5f;

        yield return StartCoroutine(ApplyKnockback(direction, force, duration));

        movement?.ForceEnable();
        isReacting = false;
        currentReactionCoroutine = null;
    }

    private IEnumerator StunReaction()
    {
        isReacting = true;
        movement?.ForceDisable();

        float stunDuration = enemyData.GetStunDuration();
        Color stunColor = enemyData.GetStunFlashColor();

        yield return StartCoroutine(StunVisualEffect(stunColor, stunDuration));

        movement?.ForceEnable();
        isReacting = false;
        currentReactionCoroutine = null;
    }

    private IEnumerator ApplyKnockback(Vector2 direction, float force, float duration)
    {
        float timer = 0f;
        Vector2 normalizedDirection = direction.normalized;

        while (timer < duration)
        {
            float currentForce = force * (1f - timer / duration);
            transform.position += (Vector3)(normalizedDirection * currentForce * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator StunVisualEffect(Color stunColor, float duration)
    {
        float flashInterval = 0.1f;
        float timer = 0f;
        bool isFlashing = false;

        while (timer < duration)
        {
            if (timer % (flashInterval * 2) < flashInterval)
            {
                if (!isFlashing)
                {
                    visuals?.PlayHitFlash(flashInterval);
                    isFlashing = true;
                }
            }
            else
            {
                isFlashing = false;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void DefaultKnockback(Vector2 direction)
    {
        currentReactionCoroutine = StartCoroutine(ApplyKnockback(direction, 5f, 0.3f));
    }

    public bool IsReacting() => isReacting;
    public HitReactionType GetReactionType() => enemyData?.hitReactionType ?? HitReactionType.Knockback;
}