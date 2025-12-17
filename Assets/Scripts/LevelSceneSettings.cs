using System.Collections.Generic;
using UnityEngine;

public class LevelSceneSettings : MonoBehaviour
{
    [Header("=== 基础配置 ===")]
    [Tooltip("本关发生的时间异常类型 (例如 Reverse 倒流)")]
    public GameTimer.TimeExceptionType timeExceptionType = GameTimer.TimeExceptionType.None;

    // --- 核心功能：多选开关 ---
    [System.Flags] // 允许在 Inspector 面板中多选
    public enum AnomalyOptions
    {
        None = 0,
        Audio = 1 << 0,       // 1: 允许影响 BGM (变速/倒放/暂停)
        PlayerBody = 1 << 1,  // 2: 允许影响玩家物理 (速度/瞬移/位置回溯/分身)
        PlayerInput = 1 << 2, // 4: 允许干扰玩家输入 (反向操作)
        SceneObjects = 1 << 3 // 8: 允许影响场景物体 (如灯光闪烁速度)
    }

    [Header("=== 效果选择 (多选) ===")]
    [Tooltip("勾选你希望本关生效的物理特效组合。\n如果不勾选任何选项，则只有左上角的UI时间数字会发生异常。")]
    public AnomalyOptions activeOptions = AnomalyOptions.Audio | AnomalyOptions.PlayerBody | AnomalyOptions.PlayerInput | AnomalyOptions.SceneObjects;

    [Header("=== 指定受影响的物体 (控制权下放) ===")]
    [Tooltip("如果不填，本关将不会有 BGM 异常。请拖入场景中你想要影响的那个 AudioSource")]
    public AudioSource specificAudio;

    [Tooltip("如果不填，将尝试自动寻找玩家。如果你有多个角色或特殊需求，请拖入当前使用的那个")]
    public PlayerController specificPlayer;

    [Tooltip("在此处拖入场景中控制灯光闪烁、风扇旋转的 Animator 组件。\n在【时间加速】异常时，这些物体的播放速度也会加倍！")]
    public List<Animator> accelerateAnimators = new List<Animator>();

    [Header("=== 触发流程 ===")]
    [Tooltip("进入场景后多少秒才触发异常")]
    public float startDelay = 0f;

    [Tooltip("异常被解决后才会出现的物体（例如：解决异常后出现照片碎片）")]
    public List<GameObject> appearOnResolve = new List<GameObject>();

    void Start()
    {
        // 优先使用全局管理器初始化，因为它可以处理物理特效
        if (TimeAnomalyManager.Instance != null)
        {
            TimeAnomalyManager.Instance.InitializeAnomaly(
                timeExceptionType,
                startDelay,
                appearOnResolve,
                activeOptions,      // 传递你的选择
                specificAudio,      // 传递指定的音频
                specificPlayer,     // 传递指定的玩家
                accelerateAnimators // 传递指定的灯光/动画
            );
        }
        else
        {
            // 容错：如果没有管理器（比如单独测试场景），直接通知 Timer 进行简单的 UI 异常
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.InjectLevelException(timeExceptionType);
            }
        }
    }
}