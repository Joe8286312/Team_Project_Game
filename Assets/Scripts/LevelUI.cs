using UnityEngine;
using TMPro;

public class LevelUI : MonoBehaviour
{
    private TextMeshProUGUI levelText;

    void Start()
    {
        levelText = GetComponent<TextMeshProUGUI>();
        UpdateUI();
    }

    void UpdateUI()
    {
        if (LevelManager.Instance != null && levelText != null)
        {
            // 显示 "Floor 1" 或 "Exit"
            string floorName = LevelManager.Instance.GetCurrentFloorText();
            levelText.text = floorName;

            // 如果想显示调试信息，可以取消注释：
            // levelText.text += $"\nAnomalies: {LevelManager.Instance.GetCurrentAnomaliesFound()}";
        }
    }
}