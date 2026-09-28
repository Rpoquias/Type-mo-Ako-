using System;
using UnityEngine;

public static class PlayerEvents
{
    public static event Action<Vector2> OnPlayerShoot;

    public static event Action OnPlayerStopShooting;

    public static void RaiseShoot(Vector2 direction)
    {
        OnPlayerShoot?.Invoke(direction);
    }

    public static void RaiseStopShooting()
    {
        OnPlayerStopShooting?.Invoke();
    }
}
