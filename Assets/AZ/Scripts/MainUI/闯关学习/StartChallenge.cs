using UnityEngine;
using UnityEngine.SceneManagement;

public class StartChallenge : MonoBehaviour
{
    public void StartStudy()
    {
        SceneManager.LoadScene("LevelSelection1");
    }
}