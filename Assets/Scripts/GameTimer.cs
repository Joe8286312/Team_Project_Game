using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    //public static GameTimer Instance { get; private set; }

    private float elapsedTime = 0f;
    private bool isRunning = false;

    // 时间异常类型
    public enum TimeExceptionType
    {
        None,          // 正常计时
        Stop,          // 停止计时
        Reverse,       // 时间反向
        Jump,          // 跳动计时
        Loop,          // 循环计时
        DoubleSpeed,   // 时间流逝速度x2
        SpawnClone     // 生成分身
    }

    public TimeExceptionType currentException = TimeExceptionType.None;

    // 跳动异常参数
    private float jumpInterval = 5f; // 每 5 秒跳一次
    private float jumpAmount = 3f;   // 跳动 3 秒
    private float lastJumpTime = 0f;

    // 循环异常参数
    private float loopStart = 0f;
    private float loopTime = 5f;

    // 标志变量，记录当前异常类型是否已记录
    private TimeExceptionType lastLoggedException = TimeExceptionType.None;

    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        DontDestroyOnLoad(gameObject);
    //    }
    //    else if (Instance != this)
    //    {
    //        Destroy(gameObject); // 防止重复实例
    //        return;
    //    }
    //}

    // 在每一帧更新时间（如果正在运行）
    void Update()
    {
        //if (isRunning)
        //{
        //    elapsedTime += Time.deltaTime;
        //}

        if (!isRunning) return;

        

        if (currentException != TimeExceptionType.None && currentException != lastLoggedException)
        {
            // 如果当前异常与上一次记录的不一样，则输出日志并更新 lastLoggedException
            Debug.Log($"检测到时间异常：{currentException}");
            lastLoggedException = currentException;
        }

        switch (currentException)
        {
            case TimeExceptionType.None:
                elapsedTime += Time.deltaTime;
                break;

            case TimeExceptionType.Stop:
                //Debug.Log("出现时间异常(停止)");

                // 时间停止，不做任何操作
                break;

            case TimeExceptionType.Reverse:
                //Debug.Log("出现时间异常(倒退)");

                elapsedTime -= Time.deltaTime; // 时间倒退
                if (elapsedTime < 0) elapsedTime = 0; // 防止时间为负数
                break;

            case TimeExceptionType.Jump:
                //Debug.Log("出现时间异常(跳动)");

                elapsedTime += Time.deltaTime;
                if (elapsedTime - lastJumpTime >= jumpInterval)
                {
                    elapsedTime += jumpAmount; // 跳动时间
                    lastJumpTime = elapsedTime;
                }
                break;

            case TimeExceptionType.Loop:
                //Debug.Log("出现时间异常(循环)");
                
                // 首次进入循环异常时，设置循环的起始时间
                if (loopStart == 0f)
                {
                    loopStart = elapsedTime;
                }

                // 计算循环区间
                float loopEnd = loopStart + loopTime;

                elapsedTime += Time.deltaTime;
                if (elapsedTime > loopEnd) elapsedTime = loopStart; // 循环时间
                break;

            case TimeExceptionType.DoubleSpeed:
                elapsedTime += Time.deltaTime * 2; // 时间流逝速度加倍
                break;

            // 为SpawnClone添加一个case，它本身不改变时间流速，但可以保持异常状态
            case TimeExceptionType.SpawnClone:
                elapsedTime += Time.deltaTime;
                break;
        }
    }

    /// <summary>
    /// 开始或继续计时。
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
    }

    /// <summary>
    /// 停止计时。
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
    }

    /// <summary>
    /// 将计时器重置为0并开始计时。
    /// </summary>
    public void ResetAndStart()
    {
        elapsedTime = 0f;
        isRunning = true;
    }

    /// <summary>
    /// 获取当前已流逝的时间。
    /// </summary>
    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    /// <summary>
    /// 检查计时器当前是否正在运行。
    /// </summary>
    public bool IsRunning()
    {
        return isRunning;
    }

    /// <summary>
    /// (静态方法) 将秒数格式化为 "分:秒.百分之一秒" 的字符串。
    /// </summary>
    public static string FormatTime(float time)
    {
        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
