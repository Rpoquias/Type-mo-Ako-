using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    private float _gameSpeed = 1f;
    public float GameSpeed => _gameSpeed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void SetGameSpeed(float newSpeed)
    {
        _gameSpeed = newSpeed;
        Time.timeScale = _gameSpeed;
    }
}
