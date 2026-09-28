using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Enemy _target;
    [SerializeField] private float speed = 15f;
    private ObjectPooler _pool;

    private float _knockbackForce = 0f;
    private float _knockbackDuration = 0f;

    public void SetTarget(Enemy target, ObjectPooler pool)
    {
        _target = target;
        _pool = pool;

        if (_target != null && _target.gameObject.activeInHierarchy)
        {
            _target.AddProjectileTarget();
            RotateTowardsTarget();
        }



    }



    public void SetKnockback(float force, float duration)
    {
        _knockbackForce = force;
        _knockbackDuration = duration;
    }

    private void Update()
    {
        if (_target == null || !_target.gameObject.activeInHierarchy)
        {
            ReturnToPool();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, speed * Time.deltaTime);
        RotateTowardsTarget();

        if (Vector3.Distance(transform.position, _target.transform.position) < 0.1f)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayEnemyHit();

            Vector2 knockbackDir = (_target.transform.position - transform.position).normalized;
            Vector3 hitPosition = transform.position + transform.up * 0.1f;

            _target.HandleProjectileHit(knockbackDir, hitPosition);

            ReturnToPool(); // This calls RemoveProjectileTarget() which handles death effect
        }
    }

    private void RotateTowardsTarget()
    {
        if (_target == null || !_target.gameObject.activeInHierarchy) return;

        Vector2 dir = (_target.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void ReturnToPool()
    {
        if (_target != null)
            _target.RemoveProjectileTarget();

        _target = null;
        gameObject.SetActive(false);
    }

}
