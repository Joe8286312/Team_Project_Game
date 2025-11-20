using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI textInfo;
    public TextMeshProUGUI textTime;
    public TextMeshProUGUI textFPS;

    public GameTimer gameTimer;

    private void Start()
    {
        gameTimer = FindObjectOfType<GameTimer>();
    }

    private void Awake()
    {
        // 初始隐藏收集信息
        if (textInfo != null)
            textInfo.gameObject.SetActive(false);
        
        // 初始化时间显示
        if (textTime != null)
            textTime.text = "00:00";

    }

    // 更新时间显示
    public void UpdateTime(float time)
    {
        if (textTime != null)
            textTime.text = GameTimer.FormatTime(time);
    }

    public void UpdateFPS(int fps)
    {
        if (textFPS != null)
        {
            textFPS.text = $"FPS: {fps}";

            // 根据帧率改变颜色
            if (fps >= 60)
                textFPS.color = Color.green;
            else if (fps >= 30)
                textFPS.color = Color.yellow;
            else
                textFPS.color = Color.red;
        }
    }

    //// 鼠标点击时间文本触发异常
    //public void OnTimeTextClicked()
    //{
    //    if (gameTimer != null)
    //    {
    //        // 循环切换时间异常状态
    //        int currentType = (int)gameTimer.currentException;
    //        currentType = (currentType + 1) % System.Enum.GetValues(typeof(GameTimer.TimeExceptionType)).Length;
    //        gameTimer.currentException = (GameTimer.TimeExceptionType)currentType;

    //        Debug.Log($"当前时间异常：{gameTimer.currentException}");
    //    }
    //}

}
