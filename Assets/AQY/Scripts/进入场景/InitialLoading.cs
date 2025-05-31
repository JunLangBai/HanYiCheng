using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialLoading : MonoBehaviour
{
    private void Start()
    {
        if (JsonManager.Instance.gameData.placementClear == false &&
            JsonManager.Instance.gameData.tutorialClear == false)
            SceneManager.LoadScene("PlacementUI");
        else if (JsonManager.Instance.gameData.placementClear && JsonManager.Instance.gameData.tutorialClear == false)
            SceneManager.LoadScene("Tutorial");
        else
            SceneManager.LoadScene("MainUI");
    }
}