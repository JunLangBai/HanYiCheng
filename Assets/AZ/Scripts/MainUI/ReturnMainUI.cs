using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnMainUI : MonoBehaviour
{
    public void ReturnToMainUI()
    {
        SceneManager.LoadScene("MainUI");
    }
}
