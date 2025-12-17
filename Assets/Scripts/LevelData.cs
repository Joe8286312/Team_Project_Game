using UnityEngine;

public enum LevelDifficulty { Easy, Medium, Hard }

[System.Serializable]
public class LevelData
{
    public string levelName = "新关卡"; // 仅用于开发者辨识
    public string sceneName;         // 必须准确匹配场景文件名
    public LevelDifficulty difficulty;
    public int totalExceptions = 0;  // 该关卡包含的异常数量
}