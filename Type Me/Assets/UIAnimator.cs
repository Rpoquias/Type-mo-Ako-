using UnityEngine;

public class UIAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void PlayAnimation()
    {
        animator.SetTrigger("PlayOpen");
    }


}