using UnityEngine;
using UnityEngine.Android;
using Rokid.UXR.Module;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Rokid.UXR.Demo
{
    public class UXRVoiceSample : MonoBehaviour
    {
        private bool isInit = false;
        public Renderer m_Render;

        public Text tipText;

        public Text infoText;


        private void Awake()
        {
            //Permission.RequestUserPermission("android.permission.RECORD_AUDIO");

            if (!Permission.HasUserAuthorizedPermission("android.permission.RECORD_AUDIO"))
            {
                Permission.RequestUserPermission("android.permission.RECORD_AUDIO");
            }

            tipText.text = "请说出“变成蓝色”或者“变成绿色”来控制物体的颜色. 点击返回按键或者说”回到上一级“回到首页";
        }

        void Start()
        {
            if (!Permission.HasUserAuthorizedPermission("android.permission.RECORD_AUDIO"))
            {
                Debug.LogError("-RKX- UXR-Sample:: no permission RECORD_AUDIO, will return!!! ");
                SceneManager.LoadScene(0);//WA
                return;
            }

            // Should acquire permission RECORD_AUDIO before initVoice to avoid a voice recog bug
            initVoice();
            OfflineVoiceModule.Instance.AddInstruct(LANGUAGE.ENGLISH, "Show blue", "show blue", this.gameObject.name, "OnReceive");
            OfflineVoiceModule.Instance.AddInstruct(LANGUAGE.ENGLISH, "Show green", "show green", this.gameObject.name, "OnReceive");
            OfflineVoiceModule.Instance.AddInstruct(LANGUAGE.ENGLISH, "Go back", "go back", this.gameObject.name, "OnReceive");

            OfflineVoiceModule.Instance.AddInstruct(LANGUAGE.CHINESE, "回到上一级", "hui dao shang yi ji", this.gameObject.name, "OnReceive");

            OfflineVoiceModule.Instance.AddInstruct(LANGUAGE.CHINESE, "变成蓝色", "bian cheng lan se", this.gameObject.name, "OnReceive");
            OfflineVoiceModule.Instance.AddInstruct(LANGUAGE.CHINESE, "变成绿色", "bian cheng lv se", this.gameObject.name, "OnReceive");
            OfflineVoiceModule.Instance.Commit();
        }

        private void OnDestroy()
        {
            Debug.Log("-RKX- UXR-Sample:: On Voice OnDestroy ");


            OfflineVoiceModule.Instance.ClearAllInstruct();


            OfflineVoiceModule.Instance.Commit();
        }


        void OnReceive(string msg)
        {
            Debug.Log("-RKX- UXR-Sample:: On Voice Response received : " + msg);

            if (string.Equals("回到上一级", msg) || string.Equals("Go back", msg))
            {
                back();
            }
            else if (string.Equals("变成蓝色", msg) || string.Equals("Show blue", msg))
            {
                m_Render.material.color = Color.blue;
            }
            else if (string.Equals("变成绿色", msg) || string.Equals("Show green", msg))
            {
                m_Render.material.color = Color.green;
            }
            else
            {
                Debug.Log("voice OnResponse: " + msg);
            }
        }

        /// <summary>
        /// 初始化声音模块 
        /// </summary>
        private void initVoice()
        {
            // Start plugin `VoiceControlFragment` , init once.
            if (!isInit)
            {
                Debug.Log("-RKX- UXR-Sample start init voice.");
                ModuleManager.Instance.RegistModule("com.rokid.voicecommand.VoiceCommandHelper", false);
              
                //Should choose one of the language to use
                OfflineVoiceModule.Instance.ChangeVoiceCommandLanguage(LANGUAGE.CHINESE); //Support for CHINESE.
                //OfflineVoiceModule.Instance.ChangeVoiceCommandLanguage(LANGUAGE.ENGLISH); //Support for ENGLISH.

                isInit = true;
            }
        }

        private void back()
        {
            SceneManager.LoadScene(0);
        }
    }
}
