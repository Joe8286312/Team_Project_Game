using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public TextMeshProUGUI textTimeButton;
    public GameObject pauseMenu;

    // --- 修改点 1: 移除对GameManager和GameTimer的直接公共引用 ---
    // public GameManager gameManager;
    // public GameTimer gameTimer;

    private void Awake()
    {
        // 游戏开始时，暂停菜单默认是关闭的
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    /// <summary>
    /// 更新暂停菜单中的时间显示
    /// </summary>
    public void UpdateTime(string time)
    {
        if (textTimeButton != null)
            textTimeButton.text = time;
    }

    // "继续游戏"按钮的响应方法
    public void OnContinueButtonClicked()
    {
        // --- 修改点 2: 通过GameManager单例来控制暂停 ---
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }
    }

    // "返回菜单"按钮的响应方法
    public void OnMenuButtonClicked()
    {
        // --- 修改点 3: 在返回菜单前，确保时间恢复正常 ---
        // GameManager的OnSceneLoaded会处理菜单场景的光标和时间暂停
        SceneManager.LoadScene("DemoMenu");
    }

    // "消除异常"按钮的响应方法
    public void OnTimeButtonClicked()
    {
        // --- 修改点 4: 通过GameTimer单例来重置异常状态 ---
        if (GameTimer.Instance != null && GameTimer.Instance.currentException != GameTimer.TimeExceptionType.None)
        {
            GameTimer.TimeExceptionType previousException = GameTimer.Instance.currentException;
            GameTimer.Instance.currentException = GameTimer.TimeExceptionType.None;
            LevelManager.Instance.RecordExceptionDiscovered();
            Debug.Log($"异常已手动清除。原异常: {previousException}");
        }
    }
}