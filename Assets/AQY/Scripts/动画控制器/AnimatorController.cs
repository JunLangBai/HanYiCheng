using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void PlayAnimation(string animName)
    {
        animator.SetTrigger(animName);
    }
}
