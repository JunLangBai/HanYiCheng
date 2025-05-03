using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Android;

namespace Rokid.UXR.Demo
{
    public class MainSceneSample : MonoBehaviour
    {
        [SerializeField] private GameObject ui;

        private bool loadUI;

        private void Awake()
        {
            RKLog.Info("====MainScene==== Awake");

#if UNITY_ANDROID && !UNITY_EDITOR
             if(!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite)){
                  RKLog.Info("====MainScene==== no permission: WRITE_EXTERNAL_STORAGE, request");
                  Permission.RequestUserPermission(Permission.ExternalStorageWrite);
             }
#endif
        }


        private void Start()
        {
#if UNITY_EDITOR
            ui.gameObject.SetActive(true);
#endif
        }

        private void Update()
        {
#if !UNITY_EDITOR
            if (!loadUI && NativeInterface.NativeAPI.GetHeadTrackingStatus() == HeadTrackingStatus.Tracking)
            {
                loadUI = true;
                ui.gameObject.SetActive(true);
            }

            if (RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_RESET)) //长按HOME键/键盘CTRL+BACK组合
            {
                RKLog.Info("UXR-UNITY::KEY_RESET");
                NativeInterface.NativeAPI.Recenter();
            }
#endif
        }

        private void OnDestroy()
        {
        }


        public void LoadScene(string sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
    }
}