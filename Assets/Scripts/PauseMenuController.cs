using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public TextMeshProUGUI textTimeButton;
    public GameObject pauseMenu;

    private void Awake()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    public void UpdateTime(string time)
    {
        if (textTimeButton != null)
            textTimeButton.text = time;
    }

    public void OnContinueButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }
    }

    public void OnMenuButtonClicked()
    {
        Time.timeScale = 1f; // 确保回主菜单时时间恢复
        SceneManager.LoadScene("DemoMenu");
    }

    // "发现异常/修复" 按钮点击响应
    public void OnTimeButtonClicked()
    {
        if (GameTimer.Instance != null)
        {
            // 1. 执行逻辑修复 (数据层面的同步)
            GameTimer.Instance.ResolveAnomaly();

            // 2. --- 核心修正点：立即强制刷新 UI 显示 ---
            // 由于游戏此时处于暂停状态 (IsGamePaused=true)，GameManager 的 Update 循环不会运行，
            // UI 不会自动刷新。必须手动从这里拉取最新时间并赋值给文本，
            // 否则玩家会觉得点击了没有反应。
            float correctTime = GameTimer.Instance.GetDisplayTime();
            if (textTimeButton != null)
            {
                textTimeButton.text = GameTimer.FormatTime(correctTime);
            }

            // Debug.Log($"UI已强制刷新为: {GameTimer.FormatTime(correctTime)}");
        }
    }
}