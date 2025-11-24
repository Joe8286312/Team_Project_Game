using UnityEngine;

public class GameTimer : MonoBehaviour
{
    // --- 修改点 1: 添加单例实例 ---
    public static GameTimer Instance { get; private set; }

    private float elapsedTime = 0f;
    private bool isRunning = false;

    // 时间异常类型定义
    public enum TimeExceptionType
    {
        None, Stop, Reverse, Jump, Loop, DoubleSpeed, SpawnClone
    }
    public TimeExceptionType currentException = TimeExceptionType.None;

    // 异常效果参数
    private float jumpInterval = 5f;
    private float jumpAmount = 3f;
    private float lastJumpTime = 0f;
    private float loopStart = 0f;
    private float loopTime = 5f;

    private TimeExceptionType lastLoggedException = TimeExceptionType.None;

    // --- 修改点 2: 实现单例模式的 Awake 方法 ---
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 设置此对象在场景切换时不被销毁
        }
        else
        {
            Destroy(gameObject); // 如果已有实例，则销毁此重复对象
        }
    }

    void Update()
    {
        if (!isRunning) return;

        // 仅在异常类型变化时记录日志，避免刷屏
        if (currentException != lastLoggedException)
        {
            Debug.Log($"触发时间异常: {currentException}");
            lastLoggedException = currentException;
        }

        // 根据当前异常类型更新时间
        switch (currentException)
        {
            case TimeExceptionType.None:
                elapsedTime += Time.deltaTime;
                break;
            case TimeExceptionType.Stop:
                // 时间停止，不执行任何操作
                break;
            case TimeExceptionType.Reverse:
                elapsedTime -= Time.deltaTime;
                if (elapsedTime < 0) elapsedTime = 0;
                break;
            case TimeExceptionType.Jump:
                elapsedTime += Time.deltaTime;
                if (Time.time - lastJumpTime >= jumpInterval) // 使用Time.time来判断真实时间间隔
                {
                    elapsedTime += jumpAmount;
                    lastJumpTime = Time.time;
                }
                break;
            case TimeExceptionType.Loop:
                if (loopStart == 0f) loopStart = elapsedTime;
                elapsedTime += Time.deltaTime;
                if (elapsedTime > loopStart + loopTime) elapsedTime = loopStart;
                break;
            case TimeExceptionType.DoubleSpeed:
                elapsedTime += Time.deltaTime * 2;
                break;
            case TimeExceptionType.SpawnClone:
                elapsedTime += Time.deltaTime;
                break;
        }
    }

    // --- 修改点 3: 不需要 StartTimer 方法，由 ResetAndStart 控制 ---
    // public void StartTimer() { isRunning = true; }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetAndStart()
    {
        elapsedTime = 0f;
        isRunning = true;
        currentException = TimeExceptionType.None; // 重置时清除异常状态
        Debug.Log("计时器已重置并启动。");
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public static string FormatTime(float time)
    {
        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}