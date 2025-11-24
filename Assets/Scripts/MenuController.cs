using UnityEngine;
using UnityEngine.SceneManagement;

public class Manu : MonoBehaviour
{
    // 开始游戏按钮
    public void StartGame()
    {
        // --- 修改点 1: 逻辑简化 ---
        // 直接加载第一个游戏场景，GameManager会在场景加载后处理计时器重置
        SceneManager.LoadScene("Demo");
    }

    // 退出游戏按钮
    public void QuitGame()
    {
        // 在编辑器模式下，Application.Quit()不起作用，因此添加日志以供调试
        Debug.Log("退出游戏");
        Application.Quit();
    }
}