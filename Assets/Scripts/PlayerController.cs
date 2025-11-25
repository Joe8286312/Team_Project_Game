using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;
    public GameObject playerClonePrefab; // 在Inspector中指定预制体

    private CharacterController characterController;
    private float verticalRotation = 0f;

    // 异常相关
    private GameObject currentException = null;
    private List<GameObject> recordedExceptions = new List<GameObject>();
    private GameObject currentlySeenException = null;
    private float seeTimer = 0f;
    private float seeDuration = 3f;
    private List<GameObject> seenExceptions = new List<GameObject>();
    private GameObject listeningException = null;
    private float listenTimer = 0f;
    private float listenDuration = 3f;
    private List<GameObject> listenedExceptions = new List<GameObject>();

    // 克隆相关
    private float cloneSpawnInterval = 10f;
    private float cloneSpawnTimer = 0f;

    // 位置历史记录
    private Queue<Vector3> positionHistory = new Queue<Vector3>();
    private float positionRecordInterval = 0.1f;
    private float recordTimer = 0f;
    private const int historySeconds = 3;
    private int maxHistorySize;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        maxHistorySize = Mathf.CeilToInt(historySeconds / positionRecordInterval);

        // --- 修改点: 移动到GameManager的ResumeGame中 ---
        // Cursor.lockState = CursorLockMode.Locked; 
    }

    void Update()
    {
        // --- 修改点: 已有逻辑，保持不变 ---
        // 通过静态变量检查游戏是否暂停，这是很好的做法
        if (GameManager.IsGamePaused) return;

        HandleMouseLook();
        HandleMovement();
        HandleGravity();
        HandleExceptionInteraction();
        RecordPosition();
        HandleCloneSpawning();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0, mouseX, 0);
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        characterController.Move(move * speed * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (!characterController.isGrounded)
        {
            characterController.Move(Vector3.down * 9.8f * Time.deltaTime);
        }
    }

    // 将所有异常处理逻辑整合到一个方法中
    private void HandleExceptionInteraction()
    {
        //// E键交互
        //if (currentException != null && Input.GetKeyDown(KeyCode.E) && !recordedExceptions.Contains(currentException))
        //{
        //    recordedExceptions.Add(currentException);
        //    Debug.Log("已记录异常: " + currentException.name);
        //}

        // E键交互 - 改为关卡交互
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnInteractKeyPressed();
            }
        }

        //// E键交互
        //if (currentException != null && Input.GetKeyDown(KeyCode.E) && !recordedExceptions.Contains(currentException))
        //{
        //    recordedExceptions.Add(currentException);
        //    Debug.Log("已记录异常: " + currentException.name);

        //    // 添加安全检查
        //    if (LevelManager.Instance != null)
        //    {
        //        LevelManager.Instance.RecordExceptionDiscovered();
        //    }
        //    else
        //    {
        //        Debug.LogWarning("LevelManager 实例未找到，无法记录异常。");
        //    }
        //}

        // 视觉检测
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 10f) && hit.collider.CompareTag("Exception_See3s"))
        {
            if (currentlySeenException != hit.collider.gameObject)
            {
                currentlySeenException = hit.collider.gameObject;
                seeTimer = 0f;
                Debug.Log("开始观察异常: " + currentlySeenException.name);
            }
            seeTimer += Time.deltaTime;
            if (seeTimer >= seeDuration && !seenExceptions.Contains(currentlySeenException))
            {
                seenExceptions.Add(currentlySeenException);
                LevelManager.Instance.RecordExceptionDiscovered();
                Debug.Log("记录视觉异常: " + currentlySeenException.name);
            }
        }
        else if (currentlySeenException != null)
        {
            currentlySeenException = null;
            seeTimer = 0f;
        }

        // 听觉检测
        bool foundListener = false;
        foreach (var obj in GameObject.FindGameObjectsWithTag("Exception_Listen3s"))
        {
            if (Vector3.Distance(transform.position, obj.transform.position) <= 3f)
            {
                foundListener = true;
                if (listeningException != obj)
                {
                    listeningException = obj;
                    listenTimer = 0f;
                    Debug.Log("开始聆听异常: " + listeningException.name);
                }
                listenTimer += Time.deltaTime;
                if (listenTimer >= listenDuration && !listenedExceptions.Contains(listeningException))
                {
                    listenedExceptions.Add(listeningException);
                    LevelManager.Instance.RecordExceptionDiscovered();
                    Debug.Log("记录听觉异常: " + listeningException.name);
                }
                break;
            }
        }
        if (!foundListener && listeningException != null)
        {
            listeningException = null;
            listenTimer = 0f;
        }
    }

    void RecordPosition()
    {
        recordTimer += Time.deltaTime;
        if (recordTimer >= positionRecordInterval)
        {
            positionHistory.Enqueue(transform.position);
            recordTimer = 0f;
            if (positionHistory.Count > maxHistorySize)
            {
                positionHistory.Dequeue();
            }
        }
    }

    void HandleCloneSpawning()
    {
        // --- 修改点: 通过单例访问 GameTimer ---
        if (GameTimer.Instance == null || GameTimer.Instance.currentException != GameTimer.TimeExceptionType.SpawnClone)
        {
            return;
        }

        cloneSpawnTimer += Time.deltaTime;
        if (cloneSpawnTimer >= cloneSpawnInterval)
        {
            cloneSpawnTimer = 0;
            if (playerClonePrefab == null)
            {
                Debug.LogError("PlayerClone Prefab未在Inspector中指定！");
                return;
            }

            // 销毁已存在的克隆体
            GameObject existingClone = GameObject.FindWithTag("Clone");
            if (existingClone != null)
            {
                Destroy(existingClone);
            }

            if (positionHistory.Count > 0)
            {
                Vector3 spawnPosition = positionHistory.Peek();
                if (Vector3.Distance(spawnPosition, transform.position) < 0.5f)
                {
                    spawnPosition = transform.position - transform.forward * 2.0f; // 增加距离避免立即重叠
                }
                GameObject newClone = Instantiate(playerClonePrefab, spawnPosition, transform.rotation);
                newClone.tag = "Clone"; // 确保新克隆体有正确的Tag
                Debug.Log($"在 {spawnPosition} 位置生成了一个新的克隆体。");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TeleportWall"))
        {
            TeleportThroughWall();
        }
        else if (other.CompareTag("Exception_E"))
        {
            currentException = other.gameObject;
            Debug.Log("进入异常交互区: " + currentException.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Exception_E") && other.gameObject == currentException)
        {
            currentException = null;
            Debug.Log("离开异常交互区。");
        }
    }

    // --- 修改点: 恢复你原来的传送逻辑 ---
    public void TeleportThroughWall()
    {
        Vector3 oldPos = transform.position;
        Debug.Log("传送前位置：" + oldPos);

        Vector3 newPos = oldPos;
        newPos.x = -oldPos.x;
        newPos.z = oldPos.z - 0.5f;

        characterController.enabled = false;
        transform.position = newPos;
        characterController.enabled = true;

        Debug.Log("传送后位置：" + transform.position);

        // 旋转180度
        Vector3 playerEuler = transform.eulerAngles;
        playerEuler.y += 180f;
        transform.rotation = Quaternion.Euler(playerEuler);
    }
}