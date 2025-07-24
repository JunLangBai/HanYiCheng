using Rokid.UXR.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FistDetection : MonoBehaviour
{
    private static FistDetection _instance;
    public static FistDetection Instance => _instance;

    private GestureType prevLeftState = GestureType.None;
    private GestureType prevRightState = GestureType.None;

    [SerializeField] private float zOffset = 0.6f; // 专门用于Z轴偏移
    private GameObject pointableUI; // A物体
    private GameObject cameraRig;   // B物体

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
        FindSceneObjects();
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
        
        if (currentState == GestureType.Grip && prevState != GestureType.Grip)
        {
            Debug.Log($"{handType} 握拳手势触发，移动A物体到B物体的Z轴+{zOffset}位置");
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
        Vector3 newPosition = bPosition + bRotation * Vector3.forward * zOffset;
        
        // 设置A物体的位置
        pointableUI.transform.position = newPosition;
        
        // 使A物体朝向B物体（看向B物体）
        pointableUI.transform.LookAt(bPosition);
        
        // 调整A物体的旋转使其正面朝向B物体
        // 因为LookAt会使物体Z轴指向目标，但UI正面是Z轴正方向
        // 所以需要额外旋转180度使正面朝向目标
        pointableUI.transform.Rotate(0, 180f, 0);
    }
}