using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manu : MonoBehaviour
{
    // 开始游戏按钮
    public void StartGame()
    {
        //// “Demo”为游戏场景名，根据实际情况更换
        //SceneManager.LoadScene("Demo");

        // 进入场景前手动重置，不推荐长期用
        var gm = FindObjectOfType<GameManager>();
        if (gm != null && gm.gameTimer != null)
        {
            gm.gameTimer.ResetAndStart();
            gm.gameTimer.currentException = GameTimer.TimeExceptionType.None;
        }
        SceneManager.LoadScene("Demo");
    }

    // 退出游戏按钮
    public void QuitGame()
    {
        // 在编辑器中用 Application.Quit() 不会生效，可以加一个debug
        Application.Quit();
        Debug.Log("退出游戏");
    }
}
