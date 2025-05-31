using UnityEngine;
using UnityEngine.SceneManagement;

public class FingerReaderSceneLoader : MonoBehaviour
{
    public string sceneName;

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}