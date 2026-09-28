using UnityEngine;

public class EffectPoolManager : MonoBehaviour
{
    public static EffectPoolManager Instance { get; private set; }

    [SerializeField] private ObjectPooler deathEffectPool;
    [SerializeField] private ObjectPooler arrowHitEffectPool;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public ObjectPooler GetDeathEffectPool() => deathEffectPool;
    public ObjectPooler GetArrowHitEffectPool() => arrowHitEffectPool;
}
