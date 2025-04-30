using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialLoading : MonoBehaviour
{
    
    void Start()
    {
        if (JsonManager.Instance.gameData.placementClear == false && JsonManager.Instance.gameData.tutorialClear == false)
        {
            SceneManager.LoadScene("PlacementUI");
        }
        else if (JsonManager.Instance.gameData.placementClear == true && JsonManager.Instance.gameData.tutorialClear == false)
        {
            SceneManager.LoadScene("Tutorial");
        }
        else
        {
            SceneManager.LoadScene("MainUI");
        }
    }
}
