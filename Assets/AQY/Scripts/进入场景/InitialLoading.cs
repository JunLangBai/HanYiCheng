using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialLoading : MonoBehaviour
{
    public float time;
    public float musicTime;
    public AudioSource audio;
    
    private void Start()
    {
        Invoke("SoundLoading",musicTime);
        
        if (JsonManager.Instance.gameData.placementClear == false &&
            JsonManager.Instance.gameData.tutorialClear == false)
        {
            Invoke("LoadPlacementUI", time);
        }
        else if (JsonManager.Instance.gameData.placementClear && 
                 JsonManager.Instance.gameData.tutorialClear == false)
        {
            Invoke("LoadTutorial", time);
        }
        else
        {
            Invoke("LoadMainUI", time);
        }
    }

    private void SoundLoading()
    {
        audio.Play();
    }

    private void LoadPlacementUI()
    {
        SceneManager.LoadScene("PlacementUI");
    }

    private void LoadTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    private void LoadMainUI()
    {
        SceneManager.LoadScene("MainUI");
    }
}