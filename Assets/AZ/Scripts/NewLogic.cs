using TMPro;
using UnityEngine;

public class FPS : MonoBehaviour
{
    //record frame count
    private int _count;
    //record time span
    private float _time;
    
    //averaging time
    [SerializeField][Range(1f,30f)]
    public float averageTime = 1f;
    
    //UI text element for displaying
    [SerializeField]
    public TextMeshProUGUI fpsText;
    // Start is called before the first frame update
    private void Start()
    {
        _count = 0;
        _time = 0f;
    }

    // Update is called once per frame
    private void Update()
    {
        var delta = Time.smoothDeltaTime;
        _time += delta;
        _count++;
        if (!(_time >= averageTime)) return;
        // Calculate and display current FPS
        fpsText.text = $"FPS: {(_count / _time):F2}";
        // Reset time and frame count
        _time = 0;
        _count = 0;
    }
}