using Rokid.UXR.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class FistDetection : MonoBehaviour
{
    private static FistDetection _instance;
    public static FistDetection Instance => _instance;

    private GestureType prevLeftState = GestureType.None;
    private GestureType prevRightState = GestureType.None;

    [SerializeField] private float defaultZOffset = 0.6f; // 默认偏移量
    private float currentZOffset; // 当前使用的偏移量
    private GameObject pointableUI; // A物体
    private GameObject cameraRig;   // B物体
    private bool isMove = true;

    // 需要特殊偏移量的场景列表
    private List<string> specialScenes = new List<string>
    {
        "InitialEnter", // 替换为您的场景名称
        "MainUI", // 替换为您的场景名称
        "PlacementUI", // 替换为您的场景名称
        "Tutorial", // 替换为您的场景名称
        "ReadAfter",
        "Reader1",
        "Reader2",
        "Reader3",
        "Reader4",
        "Reader5",
        "Reader6"
    };
    
    // 需要特殊偏移量的场景列表
    private List<string> noneScenes = new List<string>
    {
        "MockTalk"
    };

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            currentZOffset = defaultZOffset; // 初始化偏移量
        }
    }

    void Start()
    {
        FindSceneObjects();
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 检查当前场景是否需要特殊偏移量
        if (specialScenes.Contains(scene.name))
        {
            currentZOffset = 1f;
            isMove = true;
            Debug.Log($"进入特殊场景 {scene.name}, 设置偏移量为 1.0");
            FindSceneObjects();
        }
        else if (noneScenes.Contains(scene.name))
        {
            isMove = false;
            Debug.Log("UI不允许被移动");
        }
        else
        {
            isMove = true;
            currentZOffset = defaultZOffset;
            Debug.Log($"进入普通场景 {scene.name}, 设置偏移量为 {defaultZOffset}");
            FindSceneObjects();
        }
    }

    void FindSceneObjects()
    {
        pointableUI = GameObject.Find("PointableUI");
        cameraRig = GameObject.Find("RKCameraRig");
        
        if (pointableUI == null)
        {
            Debug.LogWarning("PointableUI (A物体) 未在场景中找到!");
        }
        if (cameraRig == null)
        {
            Debug.LogWarning("RKCameraRig (B物体) 未在场景中找到!");
        }

        // 只在找到两个物体时移动
        if (pointableUI != null && cameraRig != null && isMove)
        {
            MoveAToB();
        }
    }

    void Update()
    {
        CheckHandGesture(HandType.LeftHand, ref prevLeftState);
        CheckHandGesture(HandType.RightHand, ref prevRightState);
    }

    void CheckHandGesture(HandType handType, ref GestureType prevState)
    {
        if (pointableUI == null || cameraRig == null) return;
        
        GestureType currentState = GesEventInput.Instance?.GetGestureType(handType) ?? GestureType.None;
        
        if (currentState == GestureType.Grip && prevState != GestureType.Grip && isMove)
        {
            Debug.Log($"{handType} 握拳手势触发，移动A物体到B物体的Z轴+{currentZOffset}位置");
            MoveAToB();
        }

        prevState = currentState;
    }

    void MoveAToB()
    {
        // 获取B物体的位置和旋转
        Vector3 bPosition = cameraRig.transform.position;
        Quaternion bRotation = cameraRig.transform.rotation;
        
        // 计算A物体的新位置：在B物体的Z轴正方向偏移指定距离
        Vector3 newPosition = bPosition + bRotation * Vector3.forward * currentZOffset;
        
        // 设置位置
        pointableUI.transform.position = newPosition;
        
        // 使A物体朝向B物体（看向B物体）
        pointableUI.transform.LookAt(bPosition);
        
        // 调整A物体的旋转使其正面朝向B物体
        // 因为LookAt会使物体Z轴指向目标，但UI正面是Z轴正方向
        // 所以需要额外旋转180度使正面朝向目标
        pointableUI.transform.Rotate(0, 180f, 0);
    }
}