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
    private Coroutine jumpRoutine; // 新增：Jump 协程

    public GameObject clonePrefab; // 拖到 Inspector

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
        if (jumpRoutine != null) { StopCoroutine(jumpRoutine); jumpRoutine = null; } // 新增

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
                // 新增：冻结玩家
                if (HasOption(LevelSceneSettings.AnomalyOptions.PlayerBody))
                    if (currentPlayer != null) currentPlayer.SetPlayerFrozen(true);
                break;

            case GameTimer.TimeExceptionType.DoubleSpeed:
                // 加速：分模块处理
                if (HasOption(LevelSceneSettings.AnomalyOptions.Audio))
                    if (currentAudio != null) currentAudio.pitch = 2.0f;

                if (HasOption(LevelSceneSettings.AnomalyOptions.PlayerBody))
                    if (currentPlayer != null) currentPlayer.SetSpeedMultiplier(2.0f);

                if (HasOption(LevelSceneSettings.AnomalyOptions.SceneObjects))
                    if (currentAnimators != null)
                        foreach (var anim in currentAnimators) 
                            if (anim != null) anim.speed = 2.0f;
                break;

            case GameTimer.TimeExceptionType.Reverse:
                // 倒流
                if (HasOption(LevelSceneSettings.AnomalyOptions.Audio))
                    if (currentAudio != null) currentAudio.pitch = -0.8f;

                if (HasOption(LevelSceneSettings.AnomalyOptions.PlayerInput))
                    if (currentPlayer != null) currentPlayer.SetControlsInverted(true);
                break;

            case GameTimer.TimeExceptionType.Jump:
                // --- 修改：启动持续跳跃协程 ---
                if (HasOption(LevelSceneSettings.AnomalyOptions.PlayerBody))
                {
                    if (currentPlayer != null)
                    {
                        // 立即执行第一次跳跃
                        currentPlayer.SafeTeleportForward(10f);
                        // 启动持续跳跃协程
                        jumpRoutine = StartCoroutine(JumpRoutine());
                    }
                }
                break;

            case GameTimer.TimeExceptionType.Loop:
                // 循环 (音乐部分)
                if (HasOption(LevelSceneSettings.AnomalyOptions.Audio))
                {
                    if (currentAudio != null)
                    {
                        // 音频循环逻辑
                        float loopPoint = currentAudio.time;
                        currentAudio.loop = true;
                    }
                }

                // --- 核心修改：启动玩家的 Loop 循环系统 ---
                if (HasOption(LevelSceneSettings.AnomalyOptions.PlayerBody))
                    if (currentPlayer != null)
                    {
                        // 传入循环周期（可以从 LevelSceneSettings 配置）
                        float loopDuration = 5f; // 默认5秒循环
                        currentPlayer.StartLoopAnomaly(loopDuration);
                        Debug.Log("玩家 Loop 循环已启动");
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

    // --- 新增：Jump 持续跳跃协程 ---
    private IEnumerator JumpRoutine()
    {
        // 配置参数
        float jumpInterval = 5f;      // 每5秒跳跃一次
        float jumpDistance = 10f;     // 每次跳跃10米

        while (isAnomalyActive && currentType == GameTimer.TimeExceptionType.Jump)
        {
            yield return new WaitForSeconds(jumpInterval);

            // 执行跳跃
            if (currentPlayer != null)
            {
                currentPlayer.SafeTeleportForward(jumpDistance);
                Debug.Log($"时间跳跃：玩家前进 {jumpDistance} 米");
            }
        }
    }

    // --- 找回：分身生成逻辑 ---
    private IEnumerator SpawnCloneRoutine()
    {
        float interval = 10f; // 每10秒生成一个分身
        while (isAnomalyActive && currentType == GameTimer.TimeExceptionType.SpawnClone)
        {
            if (currentPlayer != null && clonePrefab != null)
            {
                // 生成分身
                Debug.Log("游戏分身生成");
                GameObject clone = Instantiate(
                    clonePrefab,
                    currentPlayer.transform.position,
                    currentPlayer.transform.rotation
                );
                // 可选：让分身半透明、自动销毁
                Destroy(clone, 8f); // 8秒后自动消失
            }
            yield return new WaitForSeconds(interval);
        }
    }

    // --- 3. 统一的清除特效方法 ---
    public void ResolveEffect()
    {
        Debug.Log("TimeAnomalyManager: 开始清理异常特效...");

        // 停止所有协程
        if (anomalyRoutine != null) 
        { 
            StopCoroutine(anomalyRoutine); 
            anomalyRoutine = null; 
        }
        if (cloneSpawnRoutine != null) 
        { 
            StopCoroutine(cloneSpawnRoutine); 
            cloneSpawnRoutine = null; 
        }
        if (jumpRoutine != null)
        { 
            StopCoroutine(jumpRoutine); 
            jumpRoutine = null; 
        }

        // 清除所有特效
        if (isAnomalyActive)
        {
            ClearAllEffects();
        }

        // 显示奖励物品
        foreach (var obj in postAnomalyObjects)
        {
            if (obj != null) obj.SetActive(true);
        }

        // --- 新增：清空引用，防止残留 ---
        currentAudio = null;
        currentPlayer = null;
        currentAnimators.Clear();
        postAnomalyObjects.Clear();
        
        Debug.Log("TimeAnomalyManager: 异常特效清理完成");
    }

    // --- 4. 清除所有特效（内部方法）---
    private void ClearAllEffects()
    {
        // 音频复原
        if (currentAudio != null)
        {
            currentAudio.UnPause();
            currentAudio.pitch = 1.0f;
            currentAudio.loop = false;
        }

        // 玩家状态复原
        if (currentPlayer != null)
        {
            currentPlayer.SetSpeedMultiplier(1.0f);
            currentPlayer.SetControlsInverted(false);

            // 新增：解除冻结
            currentPlayer.SetPlayerFrozen(false);

            // 如果是 Loop 异常，调用专门的停止方法
            if (currentType == GameTimer.TimeExceptionType.Loop)
            {
                currentPlayer.StopLoopAnomaly();
            }
        }

        // 场景物体复原
        if (currentAnimators != null)
        {
            foreach (var anim in currentAnimators)
            {
                if (anim != null) anim.speed = 1.0f;
            }
        }

        isAnomalyActive = false;
        currentType = GameTimer.TimeExceptionType.None;
        
        // Debug.Log("所有时间异常特效已清除");
    }

    void Update()
    {
        if (isAnomalyActive)
        {
            // Loop 异常的持续性逻辑（音频循环）
            if (currentType == GameTimer.TimeExceptionType.Loop)
            {
                if (HasOption(LevelSceneSettings.AnomalyOptions.Audio))
                {
                    if (currentAudio != null && currentAudio.time > 5.0f)
                    {
                        currentAudio.time = 0f;
                    }
                }
            }
        }
    }
}