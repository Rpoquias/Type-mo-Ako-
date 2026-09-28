using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private void Update()
    {

        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

        if (InputBlocker.Instance != null && InputBlocker.Instance.IsInputBlocked())
            return;


        foreach (char c in Input.inputString)
        {
            if (!char.IsControl(c))
            {
                WordManager.Instance.ProcessKey(c);
            }
        }
    }
}
