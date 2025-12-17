using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("场景配置")]
    [Tooltip("所有非教程、非终点的循环关卡数据都放在这里")]
    public List<LevelData> allPuzzleLevels = new List<LevelData>();

    [Tooltip("教程关卡的场景名称")]
    public string tutorialSceneName = "Demo_Tutorial";

    [Tooltip("终点关卡的场景名称")]
    public string endingSceneName = "Demo_Ending";

    // 新增：死亡场景或UI面板
    [Tooltip("死亡场景或UI面板")]
    public string deathSceneName = "Demo_Death";

    [Header("游戏规则")]
    [Tooltip("第几层是终点层（例如8）")]
    public int endingFloorIndex = 8;

    // --- 新增：道具收集系统 ---
    [Header("Collection System")]
    [SerializeField] private int collectedPhotos = 0;
    public int totalPhotosNeeded = 6;

    [Header("当前状态 (只读)")]
    [SerializeField] private int currentFloor = 1; // 1=教程, 2-7=循环, 8=终点
    [SerializeField] private LevelData currentLevelData;
    [SerializeField] private int foundAnomalies = 0;

    // --- 内部类：负责双列表缓冲逻辑 ---
    [System.Serializable]
    private class DifficultyPool
    {
        public List<LevelData> activeList = new List<LevelData>();
        public List<LevelData> reserveList = new List<LevelData>();

        // 初始化：把所有该难度的关卡放入 activeList 并乱序
        public void Initialize(List<LevelData> source)
        {
            activeList = new List<LevelData>(source);
            reserveList.Clear();
            Shuffle(activeList);
        }

        // 核心逻辑：取出一个，存入备用，如果空了则交换
        public LevelData GetNextLevel()
        {
            if (activeList.Count == 0)
            {
                // 如果 active 空了，说明一轮循环结束
                // 将 reserve 里的全部转正，并乱序
                if (reserveList.Count == 0)
                {
                    Debug.LogError("严重错误：没有可用的关卡数据！");
                    return null;
                }

                // 交换列表引用
                activeList = new List<LevelData>(reserveList);
                reserveList.Clear();
                Shuffle(activeList);
                Debug.Log("【关卡池】列表耗尽，已重置并乱序备用列表");
            }

            // 取出第一个
            LevelData selected = activeList[0];
            activeList.RemoveAt(0);

            // 放入备用列表
            reserveList.Add(selected);

            return selected;
        }

        private void Shuffle(List<LevelData> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                LevelData temp = list[i];
                int rnd = Random.Range(i, list.Count);
                list[i] = list[rnd];
                list[rnd] = temp;
            }
        }
    }

    // 实例化三个难度的池子
    private DifficultyPool easyPool = new DifficultyPool();
    private DifficultyPool mediumPool = new DifficultyPool();
    private DifficultyPool hardPool = new DifficultyPool();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGameData();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 每次加载新场景时重置玩家状态
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 清理玩家身上的检测记录
        var detector = FindObjectOfType<PlayerAnomalyDetector>();
        if (detector) detector.ClearRecords();

        var interactor = FindObjectOfType<PlayerInteract>();
        if (interactor) interactor.ClearRecords();
    }

    // 游戏启动时分类并初始化池子
    private void InitializeGameData()
    {
        List<LevelData> allEasy = new List<LevelData>();
        List<LevelData> allMedium = new List<LevelData>();
        List<LevelData> allHard = new List<LevelData>();

        foreach (var level in allPuzzleLevels)
        {
            switch (level.difficulty)
            {
                case LevelDifficulty.Easy: allEasy.Add(level); break;
                case LevelDifficulty.Medium: allMedium.Add(level); break;
                case LevelDifficulty.Hard: allHard.Add(level); break;
            }
        }

        easyPool.Initialize(allEasy);
        mediumPool.Initialize(allMedium);
        hardPool.Initialize(allHard);

        Debug.Log($"初始化完成：易({allEasy.Count}) 中({allMedium.Count}) 难({allHard.Count})");
    }

    // --- 新增：收集道具的方法 ---
    public void CollectPhotoFragment()
    {
        collectedPhotos++;
        Debug.Log($"收集到相片碎片: {collectedPhotos}/{totalPhotosNeeded}");
    }

    // --- 新增：处理特殊门（Exception_E）的交互逻辑 ---
    public void HandleSpecialDoorInteraction()
    {
        // 1. 如果当前是终点关 (Floor 8)
        if (currentFloor >= endingFloorIndex)
        {
            if (collectedPhotos >= totalPhotosNeeded)
            {
                TriggerHappyEnding();
            }
            else
            {
                TriggerBadEnding();
            }
        }
        // 2. 如果是非终点关 (Floor 1-7)
        else
        {
            TriggerDeath();
        }
    }

    private void TriggerHappyEnding()
    {
        Debug.Log("结局：Happy End (相片集齐)");
        // 可以在这里加载特定的Happy End场景或显示UI
        // GameManager.Instance.ShowVictory(); // 或者使用专门的 HappyEnd UI
        SceneManager.LoadScene("Scene_HappyEnd");
    }

    private void TriggerBadEnding()
    {
        Debug.Log("结局：Bad End (相片未集齐)");
        // 可以在这里加载特定的Bad End场景或显示UI
        SceneManager.LoadScene("Scene_BadEnd");
    }

    private void TriggerDeath()
    {
        Debug.Log("触发死亡：在非终点关打开了门");
        // 加载死亡场景，或者调用 GameManager 显示死亡 UI
        SceneManager.LoadScene(deathSceneName);
        // 或者: GameManager.Instance.ShowDeathUI();
    }

    // --- 外部调用入口 ---

    public void StartGame()
    {
        currentFloor = 1; // 教程关
        LoadLevelByFloorIndex();
    }

    /// <summary>
    /// 当玩家发现了异常时调用
    /// </summary>
    public void ReportAnomalyFound()
    {
        //// 终点关不记录异常
        //if (currentFloor >= endingFloorIndex) return;

        foundAnomalies++;
        int total = currentLevelData != null ? currentLevelData.totalExceptions : 0;
        Debug.Log($"异常进度: {foundAnomalies}/{total}");
    }

    /// <summary>
    /// 玩家按下E键且射线检测通过后调用此方法
    /// </summary>
    public void AttemptTransition()
    {
        Debug.Log($"尝试过关检查。当前层: {currentFloor}");

        // 1. 教程关 (Floor 1): 直接通过
        if (currentFloor == 1)
        {
            GoToNextFloor();
            return;
        }

        // 2. 终点关 (Floor >= 8): 按理应该无异常，但是玩家仍交互该按钮，说明认为循环场景消失是异常，失败
        if (currentFloor >= endingFloorIndex)
        {
            Debug.Log($"已达终点关，玩家认为循环场景消失是异常，未交互朋友家门，重置回第2层！");
            ResetToLevel2();
            return;
        }

        // 3. 循环关卡判定 (Floor 2-7)
        // 检查是否找齐了所有异常
        int required = currentLevelData != null ? currentLevelData.totalExceptions : 0;

        if (foundAnomalies >= required)
        {
            // 成功：进入下一层
            Debug.Log("异常全部发现，进入下一层！");
            GoToNextFloor();
        }
        else
        {
            // 失败：重置回第2层
            Debug.Log($"异常未找全 ({foundAnomalies}/{required})，重置回第2层！");
            ResetToLevel2();
        }
    }

    private void GoToNextFloor()
    {
        currentFloor++;
        LoadLevelByFloorIndex();
    }

    private void ResetToLevel2()
    {
        currentFloor = 2; // 回到循环的起点
        LoadLevelByFloorIndex();
    }

    private void LoadLevelByFloorIndex()
    {
        // 重置当前关卡状态
        foundAnomalies = 0;
        currentLevelData = null;

        string sceneToLoad = "";

        // 逻辑判定：第几层去哪个池子拿数据
        if (currentFloor == 1)
        {
            sceneToLoad = tutorialSceneName;
        }
        else if (currentFloor >= endingFloorIndex)
        {
            // 到了终点层，加载终点场景
            sceneToLoad = endingSceneName;
        }
        else
        {
            // 循环关卡逻辑
            if (currentFloor >= 2 && currentFloor <= 3)
            {
                currentLevelData = easyPool.GetNextLevel();
            }
            else if (currentFloor >= 4 && currentFloor <= 5)
            {
                currentLevelData = mediumPool.GetNextLevel();
            }
            else if (currentFloor >= 6 && currentFloor <= 7)
            {
                currentLevelData = hardPool.GetNextLevel();
            }

            if (currentLevelData != null)
            {
                sceneToLoad = currentLevelData.sceneName;
            }
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"正在加载场景: {sceneToLoad} (Floor {currentFloor})");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("无法加载关卡：场景名为空，可能是池子空了或配置错误。");
        }
    }

    // --- 供 UI 使用的 Getter ---

    public string GetCurrentFloorText()
    {
        if (currentFloor == 1) return "Tutorial";
        if (currentFloor >= endingFloorIndex) return "Exit";
        return $"Floor {currentFloor}";
    }

    public int GetCurrentAnomaliesFound() => foundAnomalies;
}