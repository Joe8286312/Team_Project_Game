using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("基础设置")]
    public float baseSpeed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    private float currentSpeedMultiplier = 1f;
    private bool controlsInverted = false; // 控制反转
    private CharacterController characterController;
    private float verticalRotation = 0f;

    // 新增：玩家冻结标志
    private bool isPlayerFrozen = false;

    // --- 优化后的时间循环系统 (仅用于 Loop 异常) ---
    private struct PlayerState
    {
        public Vector3 position;
        public Quaternion rotation;
        public Quaternion camRotation;
    }
    
    private List<PlayerState> loopRecords = new List<PlayerState>(); // 改名更明确
    private bool isLoopActive = false; // 是否处于循环异常
    private bool isRewinding = false;  // 是否正在回滚
    private int maxLoopFrames = 300;   // Loop 专用，约5秒 (60fps)
    private float loopDuration = 5f;   // 循环周期（秒）
    private float loopTimer = 0f;      // 循环计时器

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (GameManager.IsGamePaused) return;

        // 新增：如果冻结，禁止一切输入
        if (isPlayerFrozen) return;

        // 如果正在回滚，不接受玩家输入，只播放历史
        if (isRewinding)
        {
            ExecuteRewind();
            return;
        }

        // 正常游戏逻辑
        HandleMouseLook();
        HandleMovement();
        HandleGravity();

        // Loop 循环计时器
        if (isLoopActive)
        {
            loopTimer += Time.deltaTime;
            if (loopTimer >= loopDuration)
            {
                // 时间到，触发回滚
                StartRewind();
            }
        }
    }

    void FixedUpdate()
    {
        // 新增：冻结时不记录Loop
        if (isPlayerFrozen) return;

        // 仅在 Loop 异常激活时记录状态
        if (isLoopActive && !isRewinding && !GameManager.IsGamePaused)
        {
            RecordLoopState();
        }
    }

    // --- 核心移动逻辑 ---

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 控制反转逻辑 (时间倒流/混乱时可能用到)
        if (controlsInverted)
        {
            mouseX = -mouseX;
            mouseY = -mouseY;
        }

        transform.Rotate(0, mouseX, 0);
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        if (controlsInverted)
        {
            moveX = -moveX;
            moveZ = -moveZ;
        }

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        // 应用速度倍率
        characterController.Move(move * (baseSpeed * currentSpeedMultiplier) * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (!characterController.isGrounded)
        {
            characterController.Move(Vector3.down * 9.8f * Time.deltaTime);
        }
    }

    // --- 异常效果接口 ---

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = multiplier;
    }

    public void SetControlsInverted(bool inverted)
    {
        controlsInverted = inverted;
    }

    // 新增：设置玩家冻结状态
    public void SetPlayerFrozen(bool frozen)
    {
        isPlayerFrozen = frozen;
    }

    // --- 优化后的 Loop 循环系统 ---

    /// <summary>
    /// 启动 Loop 异常（由 TimeAnomalyManager 调用）
    /// </summary>
    public void StartLoopAnomaly(float duration = 5f)
    {
        isLoopActive = true;
        loopDuration = duration;
        loopTimer = 0f;
        loopRecords.Clear();
        
        // 根据循环时长动态调整记录帧数
        maxLoopFrames = Mathf.CeilToInt(duration * 60); // 假设60fps
        
        Debug.Log($"Loop 异常启动，循环周期: {duration}秒，最大记录帧数: {maxLoopFrames}");
    }

    /// <summary>
    /// 停止 Loop 异常（解决异常时调用）
    /// </summary>
    public void StopLoopAnomaly()
    {
        // --- 修改：添加状态检查，防止重复清理 ---
        if (!isLoopActive && !isRewinding)
        {
            return; // 如果已经是正常状态，不需要重复清理
        }

        isLoopActive = false;
        isRewinding = false;
        loopTimer = 0f;
        loopRecords.Clear();
        
        // 确保恢复控制
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        Debug.Log("Loop 异常已解决");
    }

    /// <summary>
    /// 记录循环状态（仅在 Loop 激活时调用）
    /// </summary>
    void RecordLoopState()
    {
        // 如果超过最大帧数，移除最早的记录
        if (loopRecords.Count >= maxLoopFrames)
        {
            loopRecords.RemoveAt(0);
        }

        loopRecords.Add(new PlayerState
        {
            position = transform.position,
            rotation = transform.rotation,
            camRotation = cameraTransform.localRotation
        });
    }

    /// <summary>
    /// 开始回滚（时间到达循环终点时调用）
    /// </summary>
    void StartRewind()
    {
        if (loopRecords.Count == 0)
        {
            Debug.LogWarning("没有可回滚的记录，重置计时器");
            loopTimer = 0f;
            return;
        }

        isRewinding = true;
        characterController.enabled = false; // 回滚时禁用碰撞器
        Debug.Log($"开始回滚，共 {loopRecords.Count} 帧记录");

        // 暂停游戏时间
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.PauseForRewind();
        }
    }

    /// <summary>
    /// 执行回滚逻辑（每帧调用）
    /// </summary>
    void ExecuteRewind()
    {
        if (loopRecords.Count > 0)
        {
            // 取出最后一帧（倒序播放）
            int index = loopRecords.Count - 1;
            PlayerState state = loopRecords[index];

            // 应用位置和旋转
            transform.position = state.position;
            transform.rotation = state.rotation;
            cameraTransform.localRotation = state.camRotation;

            loopRecords.RemoveAt(index);
        }
        else
        {
            // 回滚完成，回到循环起点
            FinishRewind();
        }
    }

    /// <summary>
    /// 回滚完成，恢复玩家控制
    /// </summary>
    void FinishRewind()
    {
        isRewinding = false;
        loopTimer = 0f; // 重置循环计时器
        loopRecords.Clear(); // 清空记录，重新开始记录
        characterController.enabled = true; // 恢复玩家控制

        // 恢复游戏时间
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.ResumeFromRewind();
        }
        
        Debug.Log("回滚完成，玩家可以继续操作");
    }

    // --- 安全传送 (用于 Jump 异常) ---
    public void SafeTeleportForward(float distance)
    {
        // Vector3 origin = transform.position + Vector3.up; // 稍微抬高一点检测
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 forward = transform.forward;
        RaycastHit hit;

        float targetDist = distance;

        // 射线检测前方是否有墙
        if (Physics.Raycast(origin, forward, out hit, distance))
        {
            // 如果有墙，传送到墙前 0.5 米处
            targetDist = hit.distance - 0.5f;
        }

        if (targetDist > 0.5f)
        {
            characterController.enabled = false;
            transform.position += forward * targetDist;
            characterController.enabled = true;
            Debug.Log($"时间跳跃: 前进了 {targetDist} 米");
        }
    }
}