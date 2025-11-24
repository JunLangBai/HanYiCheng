// 引入必要的命名空间
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // 用于场景管理
using System.IO;                   // 用于文件读写
using System.Collections.Generic;    // 用于使用List
using System.Linq;                 // 用于方便地查询List

// --- 用于JSON序列化的数据结构 ---

// 单个用户的数据
[System.Serializable]
public class UserData
{
    public string username;
    public string password;
}

// 包含所有用户的数据库，这是JSON文件的根对象
[System.Serializable]
public class UserDatabase
{
    public List<UserData> users = new List<UserData>();
}


public class LoginManager : MonoBehaviour
{
    [Header("场景加载接口")]
    [Tooltip("登录成功后要加载的场景名称")]
    [SerializeField] private string sceneNameToLoad;

    [Header("面板切换组件")]
    [SerializeField] private CanvasGroup loginCanvasGroup;
    [SerializeField] private CanvasGroup registerCanvasGroup;

    [Header("登录界面UI")]
    [SerializeField] private TMP_InputField loginUsernameInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button showRegisterPanelButton;

    [Header("注册界面UI")]
    [SerializeField] private TMP_InputField registerUsernameInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private TMP_InputField registerPasswordConfirmInput;
    [SerializeField] private Button finalRegisterButton;
    [SerializeField] private Button backToLoginButton;

    private UserDatabase userDatabase = new UserDatabase();
    private string userDataPath;

    void Awake()
    {
        // 设置用户数据的保存路径
        userDataPath = Path.Combine(Application.persistentDataPath, "users.json");
        // 程序启动时加载一次用户数据
        LoadUsers();
    }

    void Start()
    {
        ShowLoginPanel();
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        showRegisterPanelButton.onClick.AddListener(ShowRegisterPanel);
        finalRegisterButton.onClick.AddListener(OnFinalRegisterButtonClicked);
        backToLoginButton.onClick.AddListener(ShowLoginPanel);
    }
    
    // --- 面板切换逻辑 ---
    private void TogglePanel(CanvasGroup groupToShow, CanvasGroup groupToHide)
    {
        groupToShow.alpha = 1f;
        groupToShow.interactable = true;
        groupToShow.blocksRaycasts = true;

        groupToHide.alpha = 0f;
        groupToHide.interactable = false;
        groupToHide.blocksRaycasts = false;
    }

    public void ShowLoginPanel() => TogglePanel(loginCanvasGroup, registerCanvasGroup);
    public void ShowRegisterPanel() => TogglePanel(registerCanvasGroup, loginCanvasGroup);

    // --- 核心逻辑：登录与注册 ---

    private void OnLoginButtonClicked()
    {
        string username = loginUsernameInput.text;
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("用户名或密码不能为空！");
            return;
        }

        // 验证用户
        if (ValidateUser(username, password))
        {
            Debug.Log($"登录成功！欢迎, {username}。");
            
            // 检查场景名称是否已设置
            if (string.IsNullOrEmpty(sceneNameToLoad))
            {
                Debug.LogError("登录成功，但未在Inspector中设置要加载的场景名称！");
            }
            else
            {
                Debug.Log($"正在加载场景: {sceneNameToLoad}");
                SceneManager.LoadScene(sceneNameToLoad);
            }
        }
        else
        {
            Debug.LogError("登录失败！用户名或密码错误。");
            // 在这里可以添加UI提示，例如一个红色的错误文本
        }
    }

    private void OnFinalRegisterButtonClicked()
    {
        string username = registerUsernameInput.text;
        string password = registerPasswordInput.text;
        string confirmPassword = registerPasswordConfirmInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("注册的用户名或密码不能为空！");
            return;
        }

        if (password != confirmPassword)
        {
            Debug.LogWarning("两次输入的密码不一致！");
            return;
        }

        // 检查用户名是否已存在
        if (userDatabase.users.Any(user => user.username == username))
        {
            Debug.LogWarning($"用户名 '{username}' 已被注册！");
            return;
        }

        // 创建新用户并添加到数据库
        UserData newUser = new UserData { username = username, password = password };
        userDatabase.users.Add(newUser);
        
        // 保存更新后的数据库到JSON文件
        SaveUsers();

        Debug.Log($"用户 '{username}' 注册成功！");
        ShowLoginPanel();
    }

    // --- 数据处理：读写JSON文件 ---

    private void LoadUsers()
    {
        if (File.Exists(userDataPath))
        {
            string json = File.ReadAllText(userDataPath);
            userDatabase = JsonUtility.FromJson<UserDatabase>(json);
            Debug.Log($"已从 {userDataPath} 加载 {userDatabase.users.Count} 个用户。");
        }
        else
        {
            Debug.Log("未找到用户数据文件，将创建一个新的。");
            userDatabase = new UserDatabase();
        }
    }

    private void SaveUsers()
    {
        string json = JsonUtility.ToJson(userDatabase, true); // 'true' for pretty print
        File.WriteAllText(userDataPath, json);
        Debug.Log($"用户数据已保存到 {userDataPath}");
    }

    private bool ValidateUser(string username, string password)
    {
        // 在加载的用户数据中查找匹配的用户名和密码
        UserData user = userDatabase.users.FirstOrDefault(u => u.username == username && u.password == password);
        return user != null;
    }


    void OnDestroy()
    {
        // 省略移除监听的代码，与之前版本相同
    }
}
