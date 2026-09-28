using UnityEngine;
using System.Collections;

public class Player_ShootingController : MonoBehaviour
{
    [SerializeField] private ObjectPooler projectilePool;
    [SerializeField] private Transform firePoint;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 2f;
    [SerializeField] private float knockbackDuration = 0.2f;

    private void Start()
    {
        StartCoroutine(WaitForWordManagerAndSubscribe());
    }

    private IEnumerator WaitForWordManagerAndSubscribe()
    {
        while (WordManager.Instance == null)
            yield return null;

        WordManager.Instance.OnCorrectLetterTyped += FireProjectileAtEnemy;
    }

    private void OnDisable()
    {
        if (WordManager.Instance != null)
            WordManager.Instance.OnCorrectLetterTyped -= FireProjectileAtEnemy;
    }

    private void FireProjectileAtEnemy(Enemy target)
    {
        if (target == null) return;

        Vector2 direction = (target.transform.position - firePoint.position).normalized;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.shootingSFX);
        }

        PlayerEvents.RaiseShoot(direction);

        if (projectilePool == null || firePoint == null) return;

        GameObject projGO = projectilePool.GetPooledObject();
        if (projGO == null) return;

        projGO.transform.position = firePoint.position;
        projGO.SetActive(true);

        Projectile proj = projGO.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetTarget(target, projectilePool);

            proj.SetKnockback(knockbackForce, knockbackDuration);
        }
    }

}
