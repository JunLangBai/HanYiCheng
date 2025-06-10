using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void PlayAnimation(string animName)
    {
        animator.SetTrigger(animName);
    }

    public void BackMainUI()
    {
        SceneManager.LoadScene("MainUI");
    }

    public void RestartMockTalk()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);
    }
}
