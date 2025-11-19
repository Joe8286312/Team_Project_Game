using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int collection = 0;
    public int gameLevel = 1;

    public UIManager ui;
    public GameTimer gameTimer;

    private float deltaTime = 0.0f;
    private float fpsUpdateInterval = 0.5f;
    private float fpsTimer = 0.0f;

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

        UpdateFPS();
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


}
