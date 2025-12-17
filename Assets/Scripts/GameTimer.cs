using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    [Header("游戏规则")]
    [Tooltip("游戏最大限时（秒），超过此时间判定失败")]
    public float maxGameTime = 600f;

    // --- 真实时间系统 (累加模式，用于判定游戏结束) ---
    private float realTimeElapsed = 0f;
    private bool isRealTimeRunning = false;

    // --- 虚假时间系统 (UI显示用) ---
    private float displayElapsedTime = 0f;

    // --- 异常定义 ---
    public enum TimeExceptionType
    {
        None, Stop, Reverse, Jump, Loop, DoubleSpeed, SpawnClone
    }

    [Header("当前异常状态")]
    public TimeExceptionType currentException = TimeExceptionType.None;
    public bool hasActiveAnomaly { get; private set; } = false;

    // 异常参数
    private float jumpInterval = 5f;
    private float lastJumpTime = 0f;

    // --- 找回：Loop 异常参数 ---
    private float loopStartVal = 0f; // 记录进入循环时的时间点
    private float loopDuration = 5f; // UI 视觉上的循环周期

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // --- 核心：初始化 ---
    public void FullReset()
    {
        realTimeElapsed = 0f;
        isRealTimeRunning = true;
        displayElapsedTime = 0f;
        currentException = TimeExceptionType.None;
        hasActiveAnomaly = false;
        Debug.Log($"GameTimer: 游戏重置，限时 {maxGameTime} 秒");
    }

    // --- 注入异常 ---
    public void InjectLevelException(TimeExceptionType type)
    {
        currentException = type;

        if (type != TimeExceptionType.None)
        {
            hasActiveAnomaly = true;

            // --- 找回：初始化 Loop 参数 ---
            if (type == TimeExceptionType.Loop)
            {
                loopStartVal = displayElapsedTime; // 锁死当前时间点作为循环起点
                Debug.Log($"GameTimer: 时间循环起点设为 {loopStartVal}");
            }
        }
        else
        {
            hasActiveAnomaly = false;
            SyncDisplayToReal(); // 恢复正常时同步时间
        }
    }

    // --- 解决异常 ---
    public void ResolveAnomaly()
    {
        if (currentException != TimeExceptionType.None)
        {
            Debug.Log($"GameTimer: 正在清除异常 {currentException}");

            // 1. 状态复原
            currentException = TimeExceptionType.None;

            // 2. 通知 Manager 停止特效
            if (TimeAnomalyManager.Instance != null)
                TimeAnomalyManager.Instance.ResolveEffect();

            // 3. 计分（只在主动解决时计分）
            if (hasActiveAnomaly)
            {
                hasActiveAnomaly = false;
                if (LevelManager.Instance != null)
                    LevelManager.Instance.ReportAnomalyFound();
            }

            // 4. 强制同步时间 (防止漂移)
            SyncDisplayToReal();
            
            // --- 新增：清除回滚暂停状态 ---
            isPausedForRewind = false;
        }
        // --- 新增：即使没有异常也要确保状态复原 ---
        else
        {
            hasActiveAnomaly = false;
            isPausedForRewind = false;
            SyncDisplayToReal();
        }
    }

    private void SyncDisplayToReal()
    {
        displayElapsedTime = realTimeElapsed;
    }

    void Update()
    {
        // 暂停时全停
        if (GameManager.IsGamePaused) return;
        if (!isRealTimeRunning) return;

        // 1. 真实时间累加
        realTimeElapsed += Time.unscaledDeltaTime;

        // 检查超时
        if (realTimeElapsed >= maxGameTime)
        {
            HandleTimeOut();
            return;
        }

        // 2. UI 时间更新 (表演层)
        UpdateDisplayTime();
    }

    private bool isPausedForRewind = false; // 新增标记

    private void UpdateDisplayTime()
    {
        if (!isRealTimeRunning || isPausedForRewind) return; // 回滚时暂停

        switch (currentException)
        {
            case TimeExceptionType.None:
                // 锁死真实时间，防止浮点漂移
                displayElapsedTime = realTimeElapsed;
                break;

            case TimeExceptionType.Stop:
                // 停止：UI 不动
                break;

            case TimeExceptionType.Reverse:
                // 倒流
                displayElapsedTime -= Time.deltaTime * 1.0f;
                if (displayElapsedTime < 0) displayElapsedTime = 0;
                break;

            case TimeExceptionType.Jump:
                displayElapsedTime += Time.deltaTime;
                if (Time.time - lastJumpTime >= jumpInterval)
                {
                    // displayElapsedTime += Random.Range(10f, 60f);
                    displayElapsedTime += 5.0f;
                    lastJumpTime = Time.time;
                }
                break;

            // --- 找回：Loop UI 逻辑 ---
            case TimeExceptionType.Loop:
                displayElapsedTime += Time.deltaTime;
                // 如果 UI 时间超过了 (起点 + 5秒)，强行跳回起点
                // 这会让玩家看到时间一直在 00:10 -> 00:15 之间鬼畜
                if (displayElapsedTime > loopStartVal + loopDuration)
                {
                    displayElapsedTime = loopStartVal;
                }
                break;

            case TimeExceptionType.DoubleSpeed:
                displayElapsedTime += Time.deltaTime * 5f; // UI 跑得飞快
                break;

            // --- 找回：SpawnClone 逻辑 ---
            case TimeExceptionType.SpawnClone:
                // 在 UI 上，时间是正常流逝的，异常体现在场景里出现了克隆体
                displayElapsedTime += Time.deltaTime;
                break;

            default:
                displayElapsedTime = realTimeElapsed;
                break;
        }
    }

    private void HandleTimeOut()
    {
        isRealTimeRunning = false;
        SceneManager.LoadScene("Demo_Death");
    }

    public float GetDisplayTime() => displayElapsedTime;
    public float GetRealTimeElapsed() => realTimeElapsed;

    public static string FormatTime(float time)
    {
        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // 新增：供 PlayerController 调用
    public void PauseForRewind()
    {
        isPausedForRewind = true;
    }

    public void ResumeFromRewind()
    {
        isPausedForRewind = false;
    }
}