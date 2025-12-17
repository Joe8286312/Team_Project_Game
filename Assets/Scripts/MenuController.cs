using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        // 1. 重置时间管理器
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.FullReset();
        }

        // 2. 启动关卡管理器
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("场景中未找到LevelManager！正在尝试直接加载教程...");
            SceneManager.LoadScene("Demo_Tutorial");
        }
    }

    public void QuitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }
}