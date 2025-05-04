using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartChallenge : MonoBehaviour
{
   public void StartStudy()
   {
      SceneManager.LoadScene("LevelSelection");
   }
}
