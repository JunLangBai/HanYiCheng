using UnityEngine;
using UnityEngine.SceneManagement;

public class TotalLevelSelection : MonoBehaviour
{
    public void LevelSelection1()
    {
        SceneManager.LoadScene("LevelSelection1");
    }

    public void LevelSelection2()
    {
        SceneManager.LoadScene("LevelSelection2");
    }
    
      public void LevelSelection3()
        {
            SceneManager.LoadScene("LevelSelection3");
        }
}