using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  UnityEngine.SceneManagement;

public class BackSceneMgr : MonoBehaviour
{
    public string SceneName;

    public void BackScene()
    {
        SceneManager.LoadScene(SceneName);
    }
}
