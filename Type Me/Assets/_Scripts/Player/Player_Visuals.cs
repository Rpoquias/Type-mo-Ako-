using UnityEngine;
using System.Collections;

public class Player_Visuals : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private Transform firePoint;
    private bool facingRight = true;

    [SerializeField] private float shootingTimeoutDuration = 1.5f;
    private Coroutine shootingTimeoutCoroutine;

    private Vector3 originalFirePointPosition;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        if (firePoint != null)
            originalFirePointPosition = firePoint.localPosition;
    }

    private void OnEnable()
    {
        PlayerEvents.OnPlayerShoot += PlayShootAnimation;
        PlayerEvents.OnPlayerStopShooting += StopShootingAnimation;
    }

    private void OnDisable()
    {
        PlayerEvents.OnPlayerShoot -= PlayShootAnimation;
        PlayerEvents.OnPlayerStopShooting -= StopShootingAnimation;
    }



    public void PlayShootAnimation(Vector2 shootDirection)
    {
        if (shootDirection.x > 0.2f)
            facingRight = true;
        else if (shootDirection.x < -0.2f)
            facingRight = false;

        animator.SetFloat("shootX", facingRight ? 1f : -1f);
        animator.SetFloat("shootY", shootDirection.y);

        animator.SetBool("isShooting", true);

        UpdateFirePoint(facingRight);

        RestartShootingTimeout();
    }
    private void UpdateFirePoint(bool facingRight)
    {
        if (firePoint == null) return;

        firePoint.localPosition = new Vector3(
            facingRight ? originalFirePointPosition.x : -originalFirePointPosition.x,
            originalFirePointPosition.y,
            originalFirePointPosition.z
        );
    }
    public void StopShootingAnimation()
    {
        animator.SetBool("isShooting", false);

        if (shootingTimeoutCoroutine != null)
        {
            StopCoroutine(shootingTimeoutCoroutine);
            shootingTimeoutCoroutine = null;
        }
    }

    private void RestartShootingTimeout()
    {
        if (shootingTimeoutCoroutine != null)
        {
            StopCoroutine(shootingTimeoutCoroutine);
        }

        shootingTimeoutCoroutine = StartCoroutine(ShootingTimeoutCoroutine());
    }

    private IEnumerator ShootingTimeoutCoroutine()
    {
        yield return new WaitForSeconds(shootingTimeoutDuration);


        animator.SetBool("isShooting", false);
        shootingTimeoutCoroutine = null;
    }
}
