using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenu;         // 关联PauseMenu对象
    public GameManager gameManager;      // 可选，如果需要
    public GameTimer gameTimer;          // 可选，如果需要

    // “继续游戏”按钮功能（ButtonContinue）
    public void OnContinueButtonClicked()
    {
        // 直接调用你 GameManager 的 TogglePause 或更直接的恢复方法
        if (gameManager != null)
        {
            gameManager.TogglePause();
        }
        else
        {
            //// 手动恢复状态（保险方案，不推荐长期用）
            //Time.timeScale = 1f;
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
            //if (pauseMenu != null) pauseMenu.SetActive(false);
            //GameManager.IsGamePaused = false;
        }
    }

    // “返回菜单”按钮（ButtonMenu）
    public void OnMenuButtonClicked()
    {
        Time.timeScale = 1f; // 回到菜单前确保时间恢复
        SceneManager.LoadScene("DemoMenu"); // 你的菜单场景名，根据实际情况调整
    }

    // “异常解除”功能（ButtonTime）原本在GameManager里，提取到这里
    public void OnTimeButtonClicked()
    {
        if (gameTimer != null)
        {
            GameTimer.TimeExceptionType previousException = gameTimer.currentException;
            if (gameTimer.currentException != GameTimer.TimeExceptionType.None)
            {
                gameTimer.currentException = GameTimer.TimeExceptionType.None; // 解除异常
                Debug.Log($"异常解除，原异常类型: {previousException}");
            }
        }
    }
}
