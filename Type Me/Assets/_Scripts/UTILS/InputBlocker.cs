using UnityEngine;

public class InputBlocker : MonoBehaviour
{
    public static InputBlocker Instance { get; private set; }

    [Header("Input State")]
    [SerializeField] private bool isInputBlocked = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void BlockInput(string reason = "")
    {
        isInputBlocked = true;
    }

    public void UnblockInput(string reason = "")
    {
        isInputBlocked = false;
        Debug.Log($"Input unblocked: {reason}");
    }

    // Check if input is currently blocked
    public bool IsInputBlocked()
    {
        return isInputBlocked;
    }

    // Force unblock (for safety)
    public void ForceUnblock()
    {
        isInputBlocked = false;
        Debug.Log("Input force unblocked");
    }
}
