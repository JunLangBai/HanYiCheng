using UnityEngine;
using System.Collections.Generic;

public class PrefabGenerator : MonoBehaviour
{
    public enum GenerationMode
    {
        Sequential,
        Random
    }

    [Header("Base Settings")]
    public Transform parentObject;     // 父物体
    public GameObject prefab;         // 预制体

    [Header("Resources")]
    public List<Sprite> sprites = new List<Sprite>();
    public List<AudioClip> audioClips = new List<AudioClip>();

    [Header("Audio")]
    public AudioSource audioPlayer;   // 指定的音频播放器

    [Header("Generation Mode")]
    public GenerationMode mode = GenerationMode.Sequential;

    private int currentIndex;

    public void GeneratePrefab()
    {
        if (!ValidateComponents()) return;

        int index = GetNextIndex();
        GameObject instance = CreateInstance();
        SetupSprite(instance, index);
        PlayAudio(index);
    }

    private bool ValidateComponents()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is not assigned!");
            return false;
        }

        if (parentObject == null)
        {
            Debug.LogError("Parent object is not assigned!");
            return false;
        }

        if (sprites.Count == 0 || audioClips.Count == 0)
        {
            Debug.LogError("Sprite or AudioClip list is empty!");
            return false;
        }

        return true;
    }

    private int GetNextIndex()
    {
        int maxIndex = Mathf.Min(sprites.Count, audioClips.Count);
        
        if (mode == GenerationMode.Sequential)
        {
            int index = currentIndex % maxIndex;
            currentIndex = (currentIndex + 1) % maxIndex;
            return index;
        }
        
        return Random.Range(0, maxIndex);
    }

    private GameObject CreateInstance()
    {
        GameObject instance = Instantiate(prefab, parentObject);
        instance.transform.localPosition = Vector3.zero;
        return instance;
    }

    private void SetupSprite(GameObject instance, int index)
    {
        SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = sprites[index];
        }
        else
        {
            Debug.LogWarning("Prefab missing SpriteRenderer component");
        }
    }

    private void PlayAudio(int index)
    {
        if (audioPlayer != null)
        {
            audioPlayer.PlayOneShot(audioClips[index]);
        }
        else
        {
            Debug.LogWarning("AudioPlayer is not assigned");
        }
    }
}