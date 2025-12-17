using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeAnomalyManager : MonoBehaviour
{
    public static TimeAnomalyManager Instance { get; private set; }

    // --- 运行时动态引用的物体 (不再自动 Find，完全由配置注入) ---
    private AudioSource currentAudio;
    private PlayerController currentPlayer;
    private List<Animator> currentAnimators = new List<Animator>();

    // --- 当前关卡的参数 ---
    private float delayedTriggerTime = 0f;
    private List<GameObject> postAnomalyObjects = new List<GameObject>();
    private LevelSceneSettings.AnomalyOptions currentOptions; // 存储当前的选项组合

    // --- 状态控制 ---
    private bool isAnomalyActive = false;
    private GameTimer.TimeExceptionType currentType = GameTimer.TimeExceptionType.None;
    private Coroutine anomalyRoutine; // 用于延迟触发
    private Coroutine cloneSpawnRoutine; // 用于分身生成

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- 1. 初始化 (接收所有指定的物体和选项) ---
    public void InitializeAnomaly(
        GameTimer.TimeExceptionType type,
        float delay,
        List<GameObject> hiddenItems,
        LevelSceneSettings.AnomalyOptions options,
        AudioSource audioRef,
        PlayerController playerRef,
        List<Animator> animatorsRef
    )
    {
        // 安全清理：如果有正在运行的协程或特效，先停止
        if (anomalyRoutine != null) { StopCoroutine(anomalyRoutine); anomalyRoutine = null; }
        if (cloneSpawnRoutine != null) { StopCoroutine(cloneSpawnRoutine); cloneSpawnRoutine = null; }

        // 如果上一关的特效还在，先强制复原
        if (isAnomalyActive) ResolveEffect();

        // --- 赋值数据 ---
        currentType = type;
        delayedTriggerTime = delay;
        postAnomalyObjects = hiddenItems;
        currentOptions = options; // 记录你勾选了哪些特效

        // --- 赋值引用 ---
        currentAudio = audioRef;
        currentPlayer = playerRef;
        if (currentPlayer == null) currentPlayer = FindObjectOfType<PlayerController>(); // 唯一保留的保底逻辑
        currentAnimators = animatorsRef;

        // 隐藏奖励道具
        foreach (var obj in postAnomalyObjects)
        {
            if (obj != null) obj.SetActive(false);
        }

        // 启动流程
        if (currentType != GameTimer.TimeExceptionType.None)
        {
            anomalyRoutine = StartCoroutine(TriggerAnomalyRoutine());
        }
        else
        {
            // 如果是正常关卡，直接告诉 Timer 同步时间
            if (GameTimer.Instance != null) GameTimer.Instance.InjectLevelException(GameTimer.TimeExceptionType.None);
        }
    }

    private IEnumerator TriggerAnomalyRoutine()
    {
        // 等待延迟
        if (delayedTriggerTime > 0) yield return new WaitForSeconds(delayedTriggerTime);

        // 激活物理特效 (声音/玩家/灯光)
        ActivateEffect();

        // 激活 UI 特效 (这一步永远执行，不受 Options 限制，保证左上角时间一定会乱)
        if (GameTimer.Instance != null) GameTimer.Instance.InjectLevelException(currentType);

        anomalyRoutine = null;
    }

    // 辅助判断：当前是否勾选了某个选项
    private bool HasOption(LevelSceneSettings.AnomalyOptions option)
    {
        return (currentOptions & option) != 0;
    }

    // --- 2. 激活特效 (根据选项执行) ---
    private void ActivateEffect()
    {
        isAnomalyActive = true;
        Debug.Log($"TimeAnomalyManager: 激活 -> {currentType} | 组合: {currentOptions}");

        switch (currentType)
        {
            case GameTimer.TimeExceptionType.Stop:
                // 停止：只在勾选 Audio 时暂停音乐
                if (HasOption(LevelSceneSettings.AnomalyOptions.Audio))
                    if (currentAudio != null) currentAudio.Pause();
                break;

            case GameTimer.TimeExceptionType.DoubleSpeed:
                // 加速：分模块处理
                if (HasOption(LevelSceneSettings.AnomalyOptions.Audio))
                    if (currentAudio != null) currentAudio.pitch = 2.0f;

                if (HasOption(LevelSceneSettings.AnomalyOptions.PlayerBody))
                    if (currentPlayer != null) currentPlayer.SetSpeedMultiplier(2.0f);

                if (HasOption(LevelSceneSettings.AnomalyOptions.SceneObjects))
                {
                    if (currentAnimators != null)
                        foreach (var anim in currentAnimators) if (anim != null) anim.speed = 2.0f;
                }
                break;

            case GameTimer.TimeExceptionType.Reverse:
                // 倒流
                if (HasOption(LevelSceneSettings.AnomalyOptions.Audio))
                    if (currentAudio != null) currentAudio.pitch = -0.8f;

                if (HasOption(LevelSceneSettings.AnomalyOptions.PlayerBody))
                    if (currentPlayer != null) currentPlayer.StartRewind();

                if (HasOption(LevelSceneSettings.AnomalyOptions.PlayerInput))
                    if (currentPlayer != null) currentPlayer.SetControlsInverted(true);
                break;

            case GameTimer.TimeExceptionType.Jump:
                // 跳跃 (属于身体移动)
                if (HasOption(LevelSceneSettings.AnomalyOptions.PlayerBody))
                    if (currentPlayer != null) currentPlayer.SafeTeleportForward(10f);
                break;

            case GameTimer.TimeExceptionType.Loop:
                // 循环 (音乐部分)
                if (HasOption(LevelSceneSettings.AnomalyOptions.Audio))
                {
                    if (currentAudio != null)
                    {
                        currentAudio.time = 0;
                        currentAudio.loop = true;
                    }
                }
                break;

            case GameTimer.TimeExceptionType.SpawnClone:
                // 分身 (找回的功能)
                if (HasOption(LevelSceneSettings.AnomalyOptions.PlayerBody))
                {
                    // 启动生成克隆的协程
                    cloneSpawnRoutine = StartCoroutine(SpawnCloneRoutine());
                }
                break;
        }
    }

    // --- 找回：分身生成逻辑 ---
    private IEnumerator SpawnCloneRoutine()
    {
        while (isAnomalyActive && currentType == GameTimer.TimeExceptionType.SpawnClone)
        {
            // 这里调用你的生成逻辑，或者在 PlayerController 里有一个 SpawnClone() 方法
            Debug.Log("TimeAnomalyManager: 生成了一个时间残影 (Clone)!");

            // 示例：如果有预制体，可以在这里 Instantiate
            // Instantiate(clonePrefab, currentPlayer.transform.position, Quaternion.identity);

            yield return new WaitForSeconds(10f); // 每10秒生成一个
        }
    }

    // --- 3. 清除特效 ---
    public void ResolveEffect()
    {
        // 停止所有协程
        if (anomalyRoutine != null) { StopCoroutine(anomalyRoutine); anomalyRoutine = null; }
        if (cloneSpawnRoutine != null) { StopCoroutine(cloneSpawnRoutine); cloneSpawnRoutine = null; }

        if (isAnomalyActive)
        {
            isAnomalyActive = false;

            // 无论之前选了什么，这里都尝试复原所有状态，防止残留
            if (currentAudio != null)
            {
                currentAudio.UnPause();
                currentAudio.pitch = 1.0f;
            }

            if (currentPlayer != null)
            {
                currentPlayer.SetSpeedMultiplier(1.0f);
                currentPlayer.StopRewind();
                currentPlayer.SetControlsInverted(false);
            }

            if (currentAnimators != null)
            {
                foreach (var anim in currentAnimators) if (anim != null) anim.speed = 1.0f;
            }
        }

        // 总是显示奖励
        foreach (var obj in postAnomalyObjects)
        {
            if (obj != null) obj.SetActive(true);
        }
    }

    void Update()
    {
        if (isAnomalyActive)
        {
            // 持续性逻辑
            if (currentType == GameTimer.TimeExceptionType.Loop)
            {
                if (HasOption(LevelSceneSettings.AnomalyOptions.Audio))
                    if (currentAudio != null && currentAudio.time > 5.0f) currentAudio.time = 0f;
            }
        }
    }
}