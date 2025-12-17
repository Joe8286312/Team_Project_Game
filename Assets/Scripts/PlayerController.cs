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

    // --- 时间回滚系统 ---
    private struct PlayerState
    {
        public Vector3 position;
        public Quaternion rotation;
        public Quaternion camRotation;
    }
    private List<PlayerState> historyRecords = new List<PlayerState>();
    private bool isRewinding = false;
    private int maxHistoryFrames = 600; // 约10秒 (60fps)

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        // 可以在这里查找 TimeAnomalyManager 并注册自己，或者让 Manager 来找它
    }

    void Update()
    {
        if (GameManager.IsGamePaused) return;

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
    }

    void FixedUpdate()
    {
        if (!isRewinding && !GameManager.IsGamePaused)
        {
            RecordState();
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

    // --- 回滚系统实现 ---

    void RecordState()
    {
        if (historyRecords.Count >= maxHistoryFrames)
        {
            historyRecords.RemoveAt(0);
        }
        historyRecords.Add(new PlayerState
        {
            position = transform.position,
            rotation = transform.rotation,
            camRotation = cameraTransform.localRotation
        });
    }

    public void StartRewind()
    {
        isRewinding = true;
        characterController.enabled = false; // 禁用碰撞器以免回滚时卡住
    }

    public void StopRewind()
    {
        isRewinding = false;
        characterController.enabled = true;
        historyRecords.Clear(); // 回滚结束清空历史，防止重复
    }

    void ExecuteRewind()
    {
        if (historyRecords.Count > 0)
        {
            // 取出最后一帧
            int index = historyRecords.Count - 1;
            PlayerState state = historyRecords[index];

            transform.position = state.position;
            transform.rotation = state.rotation;
            cameraTransform.localRotation = state.camRotation;

            historyRecords.RemoveAt(index);
        }
        else
        {
            // 历史记录播放完毕，停止回滚或保持不动
            // 这里可以选择自动停止，也可以等异常解决
        }
    }

    // --- 安全传送 (用于时间跳跃) ---
    public void SafeTeleportForward(float distance)
    {
        Vector3 forward = transform.forward;
        Vector3 origin = transform.position + Vector3.up; // 稍微抬高一点检测

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