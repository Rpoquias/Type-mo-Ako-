using System.Collections;
using UnityEngine;

public class PooledEffect : MonoBehaviour
{
    [SerializeField] private Animator childAnimator; // assign child Animator in inspector
    private ObjectPooler _pool;

    // Called once when spawned, set pool reference
    public void SetPool(ObjectPooler pool)
    {
        _pool = pool;
    }

    private void OnEnable()
    {
        if (childAnimator != null)
        {
            // Ensure animator is ready
            childAnimator.enabled = true;

            // Reset animator state completely
            childAnimator.Rebind();
            childAnimator.Update(0f);

            // Wait a frame then trigger animation for reliable timing
            StartCoroutine(PlayAnimationNextFrame());
        }
    }

    private IEnumerator PlayAnimationNextFrame()
    {
        // Wait one frame for animator to be fully ready
        yield return null;

        if (childAnimator != null && gameObject.activeInHierarchy)
        {
            childAnimator.SetTrigger("Play");
            StartCoroutine(AutoDisableRoutine());
        }
    }

    private IEnumerator AutoDisableRoutine()
    {
        if (childAnimator != null)
        {
            // Wait for animation to actually start
            yield return new WaitForSeconds(0.1f);

            // Get the current state info
            AnimatorStateInfo stateInfo = childAnimator.GetCurrentAnimatorStateInfo(0);
            float length = stateInfo.length;

            // Wait for animation to complete
            yield return new WaitForSeconds(length - 0.1f); // Subtract the wait we already did
        }

        // Return to pool
        gameObject.SetActive(false);
    }
}
