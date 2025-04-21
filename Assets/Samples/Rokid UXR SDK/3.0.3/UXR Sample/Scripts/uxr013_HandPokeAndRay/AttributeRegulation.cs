using UnityEngine;
using UnityEngine.UI;
using Rokid.UXR.Interaction;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using Rokid.UXR.UI;

namespace Rokid.UXR.Demo
{
    public class AttributeRegulation : AutoInjectBehaviour
    {
        [SerializeField, Autowrited]
        private Text handScaleText;
        [SerializeField, Autowrited]
        private Slider handScaleSlider;

        [SerializeField, Autowrited]
        private Toggle fishEyeToggle;

        [SerializeField, Autowrited]
        private Toggle dspCloseAll;

        [SerializeField, Autowrited]
        private Toggle dspOnlyDetection;
        [SerializeField, Autowrited]
        private Toggle dspOnlyFollow;
        [SerializeField, Autowrited]
        private Toggle dspOpenAll;
        [SerializeField, Autowrited]
        private Toggle logToggle;
        [SerializeField, Autowrited]
        private Toggle headHandToggle;
        [SerializeField, Autowrited]
        private Button gesCalibButton;
        [SerializeField, Autowrited]
        private Button resetGesCalibButton;
        [SerializeField, Autowrited]
        private Button handMeshTestButton;
        [SerializeField, Autowrited]
        private Button forceChangeToGes;
        [SerializeField, Autowrited]
        private Text gesCaliText;

        [SerializeField, Autowrited]
        private Toggle inputStatusChangeLockToggle;

        private bool forceToGes = false;

        private bool lockInteraction = false;


        private void Start()
        {
            handScaleText.text = $"HandScale:{GesEventInput.Instance.GetHandScale()}";
            handScaleSlider.value = GesEventInput.Instance.GetHandScale();
            handScaleSlider?.onValueChanged.AddListener(value =>
            {
                GesEventInput.Instance.SetHandScale(value);
                handScaleText.text = $"HandScale:{GesEventInput.Instance.GetHandScale()}";
            });

            fishEyeToggle?.onValueChanged.AddListener(isOn =>
            {
                SetUseFishEyeDistort(isOn ? 1 : 0);
            });


            dspCloseAll?.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    SetUseDsp(0);
                }
            });

            dspOnlyDetection?.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    SetUseDsp(1);
                }
            });

            dspOnlyFollow?.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    SetUseDsp(2);
                }
            });

            dspOpenAll?.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    SetUseDsp(3);
                }
            });
            if (dspOpenAll)
                dspOpenAll.isOn = true;
            logToggle?.onValueChanged.AddListener(isOn =>
            {
                Debug.unityLogger.logEnabled = isOn;
            });
            headHandToggle.onValueChanged.AddListener(value =>
            {
                if (value)
                {
                    GesEventInput.Instance.ActiveHandOrHeadHand(HandOrHeadHandType.HeadHand);
                }
                else
                {
                    GesEventInput.Instance.ActiveHandOrHeadHand(HandOrHeadHandType.NormalHand);
                }
            });

            gesCalibButton.onClick.AddListener(() =>
            {
                NativeInterface.NativeAPI.BeginGestureCalibrate();
            });

            resetGesCalibButton.onClick.AddListener(() =>
            {
                NativeInterface.NativeAPI.ResetGestureCalibrate();
            });

            NativeInterface.NativeAPI.OnGesCalibStateChange += OnGesCalibStateChange;

            inputStatusChangeLockToggle.onValueChanged.AddListener(value =>
            {
                if (value)
                {
                    InputModuleManager.Instance.LockInputModuleChange();
                }
                else
                {
                    InputModuleManager.Instance.ReleaseInputModuleChange();
                }
            });

            forceChangeToGes.onClick.AddListener(() =>
            {
                forceToGes = true;
                InputModuleManager.Instance.ForceActiveModule(InputModuleType.Gesture);
            });

            GesEventInput.OnTrackedSuccess += OnTrackedSuccess;

            handMeshTestButton.onClick.AddListener(() =>
            {
                var behindMeshs = GesEventInput.Instance.Interactor.GetComponentsInChildren<CustomHandVisualByState>(true);
                var handVisuals = GesEventInput.Instance.Interactor.GetComponentsInChildren<HandVisual>(true);
                for (int i = 0; i < behindMeshs.Length; i++)
                {
                    behindMeshs[i].enabled = false;
                    behindMeshs[i].gameObject.SetActive(true);
                    handVisuals[i].gameObject.SetActive(true);
                }
            });


        }

        private void OnTrackedSuccess(HandType handType)
        {
            if (handType == HandType.RightHand && forceToGes)
            {
                InputModuleManager.Instance.LockInputModuleChange();
            }
        }


        private void OnGesCalibStateChange(int result)
        {
            gesCaliText.text = "GesCaliResult:" + result;
        }


        /// <summary>
        /// 是否使用鱼眼校正,0-否,1-是
        /// </summary>
        public void SetUseFishEyeDistort(int useFishEyeDistort)
        {
            NativeInterface.NativeAPI.SetUseFishEyeDistort(useFishEyeDistort);
        }

        /// <summary>
        /// 0 是 检测 和 跟踪都不开 dsp
        /// 1 是 检测开 跟踪不开 
        /// 2 是 检测不开 跟踪开
        /// 3 是 都开
        /// </summary>
        public void SetUseDsp(int useDsp)
        {
            // RKLog.KeyInfo("SetUseDsp:" + useDsp);
            NativeInterface.NativeAPI.SetUseDsp(useDsp);
        }

        private void OnDestroy()
        {
            NativeInterface.NativeAPI.StopGestureCalibrate();
        }

        private float deltaTime;
        private bool menuLongPress;
        private bool triggerMenuLongPress;

        private void Update()
        {
            menuLongPress = false;
            if (Input.GetKey(KeyCode.Menu))
            {
                if (!triggerMenuLongPress)
                {
                    deltaTime += Time.deltaTime;
                    if (deltaTime > 0.5f)
                    {
                        deltaTime = 0;
                        triggerMenuLongPress = true;
                        menuLongPress = true;
                    }
                }
            }
            else
            {
                deltaTime = 0;
                triggerMenuLongPress = false;
            }

            if (menuLongPress || Input.GetKeyDown(KeyCode.L))
            {
                if (this.lockInteraction == false)
                {
                    InputModuleManager.Instance.LockAndDisableAllInput(InputModuleType.Mouse);
                    UIManager.Instance.CreatePanel<TipPanel>(true).Init("交互锁定", TipLevel.Warning, 3);
                }
                else
                {
                    InputModuleManager.Instance.UnLockAndEnableAllInput();
                    UIManager.Instance.CreatePanel<TipPanel>(true).Init("交互释放", TipLevel.Warning, 3);
                }
                lockInteraction = !lockInteraction;
            }
        }
    }
}
