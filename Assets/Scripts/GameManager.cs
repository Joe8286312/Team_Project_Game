using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int collection = 0;
    public int gameLevel = 1;
    public TextMeshProUGUI textTimeButton;

    //public Button ButtonTime; // 对应 ButtonTime 按钮

    public UIManager ui;
    public GameTimer gameTimer;

    private float deltaTime = 0.0f;
    private float fpsUpdateInterval = 0.5f;
    private float fpsTimer = 0.0f;

    public GameObject pauseMenu; // 通过 Inspector 绑定菜单界面

    private bool isPaused = false;

    public static bool IsGamePaused { get; private set; } = false; // 静态变量供其他脚本访问

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 初始隐藏菜单界面
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        SceneManager.sceneLoaded += OnSceneLoaded;

        //// 绑定 TimeButton 的点击事件
        //if (ButtonTime != null)
        //    ButtonTime.onClick.AddListener(OnTimeButtonClicked);
    }
    void Start()
    {
        if (ui != null)
        {
            ui.UpdateTime(0); // 初始化UI计时器显示
        }
        StartTime();
    }

    void Update()
    {
        // 如果计时器正在运行，则持续更新UI上的时间显示
        if (gameTimer != null && gameTimer.IsRunning())
        {
            ui.UpdateTime(gameTimer.GetElapsedTime());
        }

        if (Input.GetKeyDown(KeyCode.Escape)) // 按下 Esc 呼出菜单
        {
            TogglePause();
        }

        // 如果计时器正在运行，更新按钮上的时间文本
        if (textTimeButton != null && gameTimer != null)
        {
            string formattedTime = GameTimer.FormatTime(gameTimer.GetElapsedTime());
            textTimeButton.text = $"{formattedTime}";
        }

        UpdateFPS();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 每次场景切换时都重新查找并绑定 UIManager 和 GameTimer
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ui = FindObjectOfType<UIManager>();
        gameTimer = FindObjectOfType<GameTimer>();
    }

    void StartTime()
    {
        // 重置并开始计时
        if (gameTimer != null)
        {
            gameTimer.ResetAndStart();
        }
    }

    // 新增：FPS计算和更新方法
    private void UpdateFPS()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        fpsTimer += Time.deltaTime;

        if (fpsTimer >= fpsUpdateInterval)
        {
            int fps = Mathf.CeilToInt(1.0f / deltaTime);
            if (ui != null)
            {
                ui.UpdateFPS(fps);
            }
            fpsTimer = 0.0f;
        }
    }

    // 将 TogglePause 方法的访问修饰符从 private 改为 public
    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        IsGamePaused = isPaused; // 更新全局暂停状态

        if (isPaused)
        {
            Time.timeScale = 0f; // 游戏逻辑暂停
            Cursor.lockState = CursorLockMode.None; // 解锁鼠标
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f; // 恢复游戏逻辑
            Cursor.lockState = CursorLockMode.Locked; // 隐藏鼠标
            Cursor.visible = false;
        }
    }

    // 点击 TimeButton 的回调方法
    //void OnTimeButtonClicked()
    //{
    //    if (gameTimer != null)
    //    {
    //        GameTimer.TimeExceptionType previousException = gameTimer.currentException;
    //        if (gameTimer.currentException != GameTimer.TimeExceptionType.None)
    //        {
    //            gameTimer.currentException = GameTimer.TimeExceptionType.None; // 将异常类型设置为 None
    //            Debug.Log($"解决时间异常，原异常类型: {previousException}");
    //        }
            
    //    }
    //}
}
