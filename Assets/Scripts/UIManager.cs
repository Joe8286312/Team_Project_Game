using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI textInfo;
    public TextMeshProUGUI textTime;
    public TextMeshProUGUI textFPS;

    private void Awake()
    {
        if (textInfo != null)
            textInfo.gameObject.SetActive(false);
        if (textTime != null)
            textTime.text = "00:00";
    }

    // --- 这个方法现在应该不再报错了，因为 GameTimer.FormatTime 已经存在 ---
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
            if (fps >= 60) textFPS.color = Color.green;
            else if (fps >= 30) textFPS.color = Color.yellow;
            else textFPS.color = Color.red;
        }
    }
}