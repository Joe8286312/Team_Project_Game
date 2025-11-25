using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("关卡配置")]
    public List<LevelData> allLevels = new List<LevelData>();
    public int tutorialLevelIndex = 0;
    public int endingLevelIndex = 8;

    [Header("UI引用")]
    public Text levelDisplayText;

    // 关卡列表
    private List<LevelData> totalLevelList1 = new List<LevelData>();
    private List<LevelData> totalLevelList2 = new List<LevelData>();
    private List<LevelData> easyLevels = new List<LevelData>();
    private List<LevelData> mediumLevels = new List<LevelData>();
    private List<LevelData> hardLevels = new List<LevelData>();

    // 当前关卡状态
    private int currentLevelIndex = -1;
    private LevelData currentLevelData;
    private int discoveredExceptions = 0;
    private bool skipTutorial = false;

    //void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        DontDestroyOnLoad(gameObject);
    //        InitializeLevelSystem();
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLevelSystem();

            // 设置默认关卡（如果需要）
            if (allLevels.Count > 0 && currentLevelData == null)
            {
                currentLevelData = allLevels[0];
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateLevelDisplay();
    }

    /// <summary>
    /// 初始化关卡系统
    /// </summary>
    private void InitializeLevelSystem()
    {
        // 清空所有列表
        totalLevelList1.Clear();
        totalLevelList2.Clear();
        easyLevels.Clear();
        mediumLevels.Clear();
        hardLevels.Clear();

        // 添加所有关卡到总列表1
        totalLevelList1.AddRange(allLevels);

        // 按难度分类
        foreach (LevelData level in allLevels)
        {
            switch (level.difficulty)
            {
                case LevelDifficulty.Easy:
                    easyLevels.Add(level);
                    break;
                case LevelDifficulty.Medium:
                    mediumLevels.Add(level);
                    break;
                case LevelDifficulty.Hard:
                    hardLevels.Add(level);
                    break;
            }
        }

        // 对各个难度列表进行乱序
        ShuffleList(easyLevels);
        ShuffleList(mediumLevels);
        ShuffleList(hardLevels);

        Debug.Log($"关卡系统初始化完成 - 易: {easyLevels.Count}关, 中: {mediumLevels.Count}关, 难: {hardLevels.Count}关");
    }

    /// <summary>
    /// 处理E键交互，判断进入哪一关
    /// </summary>
    public void OnInteractKeyPressed()
    {
        if (currentLevelIndex == -1)
        {
            // 首次进入游戏
            if (skipTutorial)
            {
                EnterLevel(GetNextLevelIndex());
            }
            else
            {
                EnterLevel(tutorialLevelIndex);
            }
        }
        else
        {
            // 判断是否所有异常都已发现
            if (currentLevelData != null && discoveredExceptions >= currentLevelData.totalExceptions)
            {
                // 所有异常已发现，进入下一关
                int nextLevel = GetNextLevelIndex();
                EnterLevel(nextLevel);
            }
            else
            {
                // 未发现所有异常，进入第2关重头开始
                EnterLevel(2);
            }
        }
    }

    ///// <summary>
    ///// 进入指定关卡
    ///// </summary>
    //public void EnterLevel(int levelIndex)
    //{
    //    if (levelIndex < 0 || levelIndex >= allLevels.Count)
    //    {
    //        Debug.LogError($"无效的关卡索引: {levelIndex}");
    //        return;
    //    }

    //    currentLevelIndex = levelIndex;
    //    currentLevelData = allLevels[levelIndex];
    //    discoveredExceptions = 0;

    //    // 从相应列表中移除当前关卡（如果存在）
    //    RemoveLevelFromLists(currentLevelData);

    //    // 加载场景
    //    SceneManager.LoadScene(currentLevelData.sceneName);

    //    // 更新UI显示
    //    UpdateLevelDisplay();

    //    Debug.Log($"进入关卡: {currentLevelData.levelName} {currentLevelData.sceneName} (难度: {currentLevelData.difficulty})");
    //}

    /// <summary>
    /// 进入指定关卡
    /// </summary>
    public void EnterLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= allLevels.Count)
        {
            Debug.LogError($"无效的关卡索引: {levelIndex}");
            return;
        }

        currentLevelIndex = levelIndex;
        currentLevelData = allLevels[levelIndex];
        discoveredExceptions = 0;

        // 从相应列表中移除当前关卡（如果存在）
        RemoveLevelFromLists(currentLevelData);

        // 根据场景名称加载场景
        if (!string.IsNullOrEmpty(currentLevelData.sceneName))
        {
            SceneManager.LoadScene(currentLevelData.sceneName);
        }
        else
        {
            Debug.LogError($"关卡 {currentLevelData.levelName} 的场景名称为空！");
            return;
        }

        // 更新UI显示
        UpdateLevelDisplay();

        Debug.Log($"进入关卡: {currentLevelData.levelName} (场景: {currentLevelData.sceneName}, 难度: {currentLevelData.difficulty})");
    }

    /// <summary>
    /// 通过场景名称进入关卡
    /// </summary>
    public void EnterLevel(string sceneName)
    {
        // 查找对应场景名称的关卡数据
        LevelData targetLevel = allLevels.Find(level => level.sceneName == sceneName);

        if (targetLevel != null)
        {
            int levelIndex = allLevels.IndexOf(targetLevel);
            EnterLevel(levelIndex);
        }
        else
        {
            Debug.LogError($"未找到场景名为 {sceneName} 的关卡数据！");
        }
    }

    /// <summary>
    /// 获取下一个关卡的索引
    /// </summary>
    private int GetNextLevelIndex()
    {
        // 如果当前是教程关，进入正式关卡选择
        if (currentLevelIndex == tutorialLevelIndex)
        {
            return GetLevelByDifficultyRules(2); // 从第2关开始
        }

        // 如果当前是结局关，回到菜单或重新开始
        if (currentLevelIndex == endingLevelIndex)
        {
            // 这里可以返回到主菜单或重新开始游戏
            return tutorialLevelIndex;
        }

        // 根据难度规则获取下一关
        int nextLevel = GetLevelByDifficultyRules(currentLevelIndex + 1);

        return nextLevel;
    }

    /// <summary>
    /// 根据难度规则获取关卡
    /// </summary>
    private int GetLevelByDifficultyRules(int expectedLevel)
    {
        // 如果所有关卡都已经体验过一遍
        if (totalLevelList1.Count == 0 && totalLevelList2.Count > 0)
        {
            // 对总列表2进行乱序
            if (totalLevelList2.Count > 1)
            {
                ShuffleList(totalLevelList2);
            }

            // 从总列表2中获取关卡
            if (totalLevelList2.Count > 0)
            {
                LevelData nextLevel = totalLevelList2[0];
                totalLevelList2.RemoveAt(0);
                totalLevelList1.Add(nextLevel);
                return allLevels.IndexOf(nextLevel);
            }
        }

        // 根据预期关卡决定难度
        LevelDifficulty targetDifficulty = GetDifficultyByLevel(expectedLevel);

        // 尝试从对应难度列表中获取关卡
        List<LevelData> targetList = GetLevelListByDifficulty(targetDifficulty);

        if (targetList.Count > 0)
        {
            LevelData nextLevel = targetList[0];
            targetList.RemoveAt(0);
            totalLevelList2.Add(nextLevel);
            return allLevels.IndexOf(nextLevel);
        }

        // 如果目标难度列表为空，尝试其他难度
        foreach (LevelDifficulty difficulty in System.Enum.GetValues(typeof(LevelDifficulty)))
        {
            if (difficulty != targetDifficulty)
            {
                List<LevelData> fallbackList = GetLevelListByDifficulty(difficulty);
                if (fallbackList.Count > 0)
                {
                    LevelData nextLevel = fallbackList[0];
                    fallbackList.RemoveAt(0);
                    totalLevelList2.Add(nextLevel);
                    return allLevels.IndexOf(nextLevel);
                }
            }
        }

        // 如果所有列表都为空，回到教程关
        Debug.LogWarning("所有关卡都已体验，回到教程关");
        return tutorialLevelIndex;
    }

    /// <summary>
    /// 根据关卡索引获取难度
    /// </summary>
    private LevelDifficulty GetDifficultyByLevel(int levelIndex)
    {
        if (levelIndex >= 2 && levelIndex <= 3) return LevelDifficulty.Easy;
        if (levelIndex >= 4 && levelIndex <= 5) return LevelDifficulty.Medium;
        if (levelIndex >= 6 && levelIndex <= 7) return LevelDifficulty.Hard;

        return LevelDifficulty.Easy; // 默认
    }

    /// <summary>
    /// 根据难度获取对应的关卡列表
    /// </summary>
    private List<LevelData> GetLevelListByDifficulty(LevelDifficulty difficulty)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Easy: return easyLevels;
            case LevelDifficulty.Medium: return mediumLevels;
            case LevelDifficulty.Hard: return hardLevels;
            default: return easyLevels;
        }
    }

    /// <summary>
    /// 从所有列表中移除指定关卡
    /// </summary>
    private void RemoveLevelFromLists(LevelData level)
    {
        totalLevelList1.Remove(level);
        easyLevels.Remove(level);
        mediumLevels.Remove(level);
        hardLevels.Remove(level);
    }

    ///// <summary>
    ///// 记录发现的异常
    ///// </summary>
    //public void RecordExceptionDiscovered()
    //{
    //    discoveredExceptions++;
    //    Debug.Log($"发现异常: {discoveredExceptions}/{currentLevelData.totalExceptions}");

    //    // 可以在这里触发UI更新或其他逻辑
    //}

    /// <summary>
    /// 记录发现的异常
    /// </summary>
    public void RecordExceptionDiscovered()
    {
        // 添加安全检查
        if (currentLevelData == null)
        {
            Debug.LogWarning("尝试记录异常，但当前没有激活的关卡数据。");
            return;
        }

        discoveredExceptions++;
        Debug.Log($"发现异常: {discoveredExceptions}/{currentLevelData.totalExceptions}");

        // 更新UI显示
        UpdateLevelDisplay();

        // 可以在这里触发其他逻辑，比如检查是否发现所有异常
        CheckAllExceptionsDiscovered();
    }

    /// <summary>
    /// 检查是否发现了所有异常
    /// </summary>
    private void CheckAllExceptionsDiscovered()
    {
        if (currentLevelData != null && discoveredExceptions >= currentLevelData.totalExceptions)
        {
            Debug.Log($"已发现所有 {currentLevelData.totalExceptions} 个异常！");
            // 可以在这里触发完成关卡的逻辑
        }
    }

    /// <summary>
    /// 设置跳过教程
    /// </summary>
    public void SetSkipTutorial(bool skip)
    {
        skipTutorial = skip;
        Debug.Log(skip ? "已设置跳过教程关" : "将播放教程关");
    }

    ///// <summary>
    ///// 更新关卡显示文本
    ///// </summary>
    //private void UpdateLevelDisplay()
    //{
    //    if (levelDisplayText != null && currentLevelData != null)
    //    {
    //        levelDisplayText.text = $"当前关卡: {currentLevelData.levelName}\n难度: {currentLevelData.difficulty}\n异常: {discoveredExceptions}/{currentLevelData.totalExceptions}";
    //    }
    //}

    /// <summary>
    /// 更新关卡显示文本
    /// </summary>
    private void UpdateLevelDisplay()
    {
        if (levelDisplayText != null)
        {
            if (currentLevelData != null)
            {
                levelDisplayText.text = $"当前关卡: {currentLevelData.levelName}\n难度: {currentLevelData.difficulty}\n异常: {discoveredExceptions}/{currentLevelData.totalExceptions}";
            }
            else
            {
                levelDisplayText.text = "当前无激活关卡";
            }
        }
    }

    /// <summary>
    /// 列表乱序排序
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    /// <summary>
    /// 获取当前关卡信息（供其他脚本使用）
    /// </summary>
    public LevelData GetCurrentLevelData()
    {
        return currentLevelData;
    }

    /// <summary>
    /// 获取当前发现的异常数
    /// </summary>
    public int GetDiscoveredExceptions()
    {
        return discoveredExceptions;
    }
}

/// <summary>
/// 关卡难度枚举
/// </summary>
public enum LevelDifficulty
{
    Easy,
    Medium,
    Hard
}

/// <summary>
/// 关卡数据类
/// </summary>
[System.Serializable]
public class LevelData
{
    public string levelName;
    public string sceneName;
    public LevelDifficulty difficulty;
    public int totalExceptions;
}