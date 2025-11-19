using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    private float elapsedTime = 0f;
    private bool isRunning = false;

    // 在每一帧更新时间（如果正在运行）
    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
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
