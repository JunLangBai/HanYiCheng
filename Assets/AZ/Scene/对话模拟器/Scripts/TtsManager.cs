using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace OpenapiDemo
{
    [RequireComponent(typeof(AudioSource))]
    public class TtsManager : MonoBehaviour
    {
        [Header("有道智云API凭证")]
        // ######## 已将您的API凭证更新在此处 ########
        [SerializeField] private string appKey = "5a2f7e95ec88ff0d";
        [SerializeField] private string appSecret = "QagE0IVcZc8OfmCgtzQBi2BppIsesgP9";
        // #########################################

        [Header("TTS参数")]
        [SerializeField] private string voiceName = "piaozhiyou";
        [SerializeField] private string format = "mp3";

        [Tooltip("合成音频的语速，范围 0.5 ~ 2.0，默认为1.0")]
        [SerializeField] private string speed = "1.0";
        [Tooltip("合成音频的音量，范围 0.5 ~ 5.0，默认为1.0")]
        [SerializeField] private string volume = "1.0";

        private AudioSource audioSource;
        private string _audioSavePath;
        private bool isSynthesizing = false; // 防止重复请求

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("TtsManager需要一个AudioSource组件！");
            }

            // 统一使用 persistentDataPath，它在Editor和安卓设备上都能正确工作
            _audioSavePath = Path.Combine(Application.persistentDataPath, "tts_output.mp3");
            Debug.Log($"TTS音频将保存至: {_audioSavePath}");
        }

        /// <summary>
        /// 公共接口：接收文本，开始TTS合成并播放
        /// </summary>
        /// <param name="textToSynthesize">需要转换为语音的文本</param>
        public void SynthesizeAndPlay(string textToSynthesize)
        {
            if (isSynthesizing)
            {
                Debug.LogWarning("正在进行TTS合成，请稍后再试。");
                return;
            }

            if (string.IsNullOrEmpty(textToSynthesize))
            {
                Debug.LogError("传入的文本为空，无法进行TTS合成。");
                return;
            }

            StartCoroutine(RequestTtsCoroutine(textToSynthesize));
        }

        private System.Collections.IEnumerator RequestTtsCoroutine(string text)
        {
            isSynthesizing = true;
            Debug.Log($"开始合成文本: {text}");

            byte[] result = null;
            Exception taskException = null;

            // 在后台线程中执行网络请求，避免阻塞主线程
            Task ttsTask = Task.Run(() =>
            {
                try
                {
                    Dictionary<string, string[]> paramsMap = CreateRequestParams(text);
                    AuthV3Util.addAuthParams(appKey, appSecret, paramsMap);
                    Dictionary<string, string[]> header = new Dictionary<string, string[]>() { { "Content-Type", new string[] { "application/x-www-form-urlencoded" } } };
                    
                    result = HttpUtil.doPost("https://openapi.youdao.com/ttsapi", header, paramsMap, "audio");
                }
                catch (Exception e)
                {
                    taskException = e;
                }
            });

            // 等待后台任务完成
            yield return new WaitUntil(() => ttsTask.IsCompleted);

            if (taskException != null)
            {
                Debug.LogError($"TTS请求任务失败: {taskException.Message}");
                isSynthesizing = false;
                yield break;
            }

            if (result == null || result.Length == 0)
            {
                Debug.LogError("获取音频数据失败，请检查网络、API凭证或参数是否正确。");
                isSynthesizing = false;
                yield break;
            }

            Debug.Log("音频数据获取成功，准备保存和播放...");
            SaveFile(_audioSavePath, result);
            yield return StartCoroutine(PlayAudioCoroutine(_audioSavePath));

            isSynthesizing = false;
        }

        private Dictionary<string, string[]> CreateRequestParams(string text)
        {
            var paramsDict = new Dictionary<string, string[]>() {
                { "q", new string[]{text}},
                {"voiceName", new string[]{voiceName}},
                {"format", new string[]{format}}
            };

            if (!string.IsNullOrEmpty(speed) && speed != "1.0")
            {
                paramsDict.Add("speed", new string[] { speed });
            }
            if (!string.IsNullOrEmpty(volume) && volume != "1.0")
            {
                paramsDict.Add("volume", new string[] { volume });
            }

            return paramsDict;
        }

        private void SaveFile(string path, byte[] data)
        {
            try
            {
                File.WriteAllBytes(path, data);
                Debug.Log("TTS音频文件保存成功! 路径: " + path);
            }
            catch (Exception e)
            {
                Debug.LogError($"保存文件出错: {e.Message}");
            }
        }

        private System.Collections.IEnumerator PlayAudioCoroutine(string filePath)
        {
            // 对于本地文件，需要加上 "file://" 前缀
            string url = "file://" + filePath;
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"加载音频文件失败: {www.error}");
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    if (clip != null)
                    {
                        audioSource.clip = clip;
                        audioSource.Play();
                        Debug.Log("开始播放TTS音频...");
                    }
                    else
                    {
                        Debug.LogError("加载的AudioClip为空，请检查音频文件格式是否正确。");
                    }
                }
            }
        }
    }
}