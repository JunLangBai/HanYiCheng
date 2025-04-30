using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalVolumeControl : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;
    GameData gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");
    public CanvasGroup fill;
    

    void Start()
    {
        
        //读取
        // 初始化滑块值
        volumeSlider.value = gameData.volume;
        AudioListener.volume = volumeSlider.value;
        
        // 绑定事件
        volumeSlider.onValueChanged.AddListener(volume => {
            if (volume == 0)
            {
                fill.alpha = 0;
            }
            else
            {
                fill.alpha = 1;
            }

            AudioListener.volume = volume;
            gameData.volume = volumeSlider.value;
            // 保存修改后的数据
            JsonFileManager.SaveToJson(gameData, "GameData.json");
        });
    }
}
