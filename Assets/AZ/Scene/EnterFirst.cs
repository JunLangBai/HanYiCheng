using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterFirst : MonoBehaviour
{
    
    public float time;
    public float musicTime;
    public AudioSource audio;
    public AudioClip audioClip;
    
    private void Start()
    {
        Invoke("SoundLoading",musicTime);
        
        
        Invoke("LoadPlacementUI", time);
    }

    private void SoundLoading()
    {
        audio.clip = audioClip;
        audio.Play();
    }

    private void LoadPlacementUI()
    {
        SceneManager.LoadScene("Login");
    }

}
