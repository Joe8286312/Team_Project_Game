using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI textInfo;
    public TextMeshProUGUI textTime;
    public TextMeshProUGUI textFPS;

    // --- 修改点: GameTimer引用不再需要公开 ---
    // public GameTimer gameTimer;

    private void Awake()
    {
        if (textInfo != null)
            textInfo.gameObject.SetActive(false);
        if (textTime != null)
            textTime.text = "00:00";
    }

    // --- 修改点: 此方法由GameManager在Update中调用 ---
    public void UpdateTime(float time)
    {
        if (textTime != null)
            textTime.text = GameTimer.FormatTime(time);
    }

    // --- 修改点: 此方法由GameManager在Update中调用 ---
    public void UpdateFPS(int fps)
    {
        if (textFPS != null)
        {
            textFPS.text = $"FPS: {fps}";
            if (fps >= 60) textFPS.color = Color.green;
            else if (fps >= 30) textFPS.color = Color.yellow;
            else textFPS.color = Color.red;
        }
    }
}