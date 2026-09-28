using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject systemPrefab;

    private void Awake()
    {
        // Spawn SYSTEM if it's not already present
        if (FindObjectOfType<SessionManager>() == null)
        {
            GameObject system = Instantiate(systemPrefab);
            DontDestroyOnLoad(system);
        }
    }
}
