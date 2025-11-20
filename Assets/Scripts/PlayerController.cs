using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    private float verticalRotation = 0f;
    private CharacterController characterController;

    private GameObject currentException = null; // 记录当前触发的异常物体
    private List<GameObject> recordedExceptions = new List<GameObject>(); // 玩家已记录的异常对象

    // 观察异常相关变量
    private GameObject currentlySeenException = null;
    private float seeTimer = 0f;
    private float seeDuration = 3f;
    private List<GameObject> seenExceptions = new List<GameObject>();

    private GameObject listeningException = null;
    private float listenTimer = 0f;
    private float listenDuration = 3f;
    private List<GameObject> listenedExceptions = new List<GameObject>();



    // 新增：分身相关变量
    public GameObject playerClonePrefab; // 在Inspector中指定分身预制体
    private float cloneSpawnInterval = 10f; // 每10秒生成一次
    private float cloneSpawnTimer = 0f;

    // 新增：位置历史记录
    private Queue<Vector3> positionHistory = new Queue<Vector3>();
    private float positionRecordInterval = 0.1f; // 每0.1秒记录一次位置
    private float recordTimer = 0f;
    private const int historySeconds = 3; // 记录时长
    private int maxHistorySize;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        // 在Start中提前计算队列最大尺寸，避免在Update中重复计算
        maxHistorySize = Mathf.CeilToInt(historySeconds / positionRecordInterval);
    }

    void Update()
    {
        if (GameManager.IsGamePaused) return; // 如果游戏已暂停，则直接退出 Update()

        // 视角
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0, mouseX, 0);
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

        // 移动
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        characterController.Move(move * speed * Time.deltaTime);

        // 简单重力处理
        if (!characterController.isGrounded)
            characterController.Move(Vector3.down * 9.8f * Time.deltaTime);

        // 按E键交互异常
        if (currentException != null && Input.GetKeyDown(KeyCode.E))
        {
            if (!recordedExceptions.Contains(currentException))
            {
                recordedExceptions.Add(currentException);
                Debug.Log("已记录异常: " + currentException.name);
                // 这里还可以做进一步反馈（显示UI、加分等）
            }
        }

        // 观察异常检测（射线检测视野中心）
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10f))
        {
            if (hit.collider.CompareTag("Exception_See3s"))
            {
                if (currentlySeenException == hit.collider.gameObject)
                {
                    seeTimer += Time.deltaTime;
                    if (seeTimer >= seeDuration && !seenExceptions.Contains(currentlySeenException))
                    {
                        seenExceptions.Add(currentlySeenException);
                        Debug.Log("记录观察异常: " + currentlySeenException.name);
                    }
                }
                else     // << 进入新的异常物体视野
                {
                    currentlySeenException = hit.collider.gameObject;
                    seeTimer = 0f;
                    Debug.Log("开始观察异常物体: " + currentlySeenException.name);
                }
            }
            else   // << 离开观察异常物体
            {
                if (currentlySeenException != null)
                {
                    Debug.Log("离开观察异常物体: " + currentlySeenException.name);
                    currentlySeenException = null;
                    seeTimer = 0f;
                }
            }
        }
        else
        {
            if (currentlySeenException != null)
            {
                Debug.Log("离开观察异常物体: " + currentlySeenException.name);
                currentlySeenException = null;
                seeTimer = 0f;
            }
        }

        // 检测场景内所有 Exception_Listen3s 类型物体
        GameObject[] listenObjects = GameObject.FindGameObjectsWithTag("Exception_Listen3s");
        bool found = false;
        foreach (var obj in listenObjects)
        {
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist <= 3f)
            {
                found = true;
                if (listeningException == obj)
                {
                    listenTimer += Time.deltaTime;
                    if (listenTimer >= listenDuration && !listenedExceptions.Contains(listeningException))
                    {
                        listenedExceptions.Add(listeningException);
                        Debug.Log("记录聆听异常: " + listeningException.name);
                    }
                }
                else      // << 进入新的监听异常
                {
                    listeningException = obj;
                    listenTimer = 0f;
                    Debug.Log("开始聆听异常物体: " + listeningException.name);
                }
                break;
            }
        }
        if (!found)
        {
            if (listeningException != null)   // << 离开监听异常
            {
                Debug.Log("离开聆听异常物体: " + listeningException.name);
                listeningException = null;
                listenTimer = 0f;
            }
        }

        

        RecordPosition();
        HandleCloneSpawning(); // 新增：处理分身生成
    }

    // 新增：记录玩家位置的方法
    void RecordPosition()
    {
        recordTimer += Time.deltaTime;
        if (recordTimer >= positionRecordInterval)
        {
            positionHistory.Enqueue(transform.position);
            recordTimer -= positionRecordInterval; // 使用减法更精确，防止时间漂移

            // 如果队列超出最大尺寸，只移除一个最旧的元素
            if (positionHistory.Count > maxHistorySize)
            {
                positionHistory.Dequeue();
            }
        }
    }

    // 新增：处理分身生成的方法
    //void HandleCloneSpawning()
    //{
    //    // 检查当前是否为分身异常，如果不是则直接返回
    //    if (GameTimer.TimeExceptionType.SpawnClone != FindObjectOfType<GameTimer>().currentException)
    //    {
    //        return;
    //    }

    //    cloneSpawnTimer += Time.deltaTime;
    //    if (cloneSpawnTimer >= cloneSpawnInterval)
    //    {
    //        cloneSpawnTimer = 0; // 重置计时器

    //        // 确保历史记录中有足够的数据
    //        if (positionHistory.Count > 0)
    //        {
    //            // 取出队列头部的元素，即3秒前的位置
    //            Vector3 spawnPosition = positionHistory.Peek();

    //            // 解决原地生成问题
    //            if (Vector3.Distance(spawnPosition, transform.position) < 0.5f) // 如果距离小于0.5米
    //            {
    //                // 在玩家身后1米处生成
    //                spawnPosition = transform.position - transform.forward * 1.0f;
    //                Debug.Log("玩家原地未动，分身已在身后生成。");
    //            }

    //            Instantiate(playerClonePrefab, spawnPosition, transform.rotation); // 使用玩家当前朝向
    //            Debug.Log($"在 {spawnPosition} 位置生成了一个分身。");
    //        }
    //        else
    //        {
    //            Debug.Log("位置历史记录不足，无法生成分身。");
    //        }
    //    }
    //}

    void HandleCloneSpawning()
    {
        GameTimer gameTimer = FindObjectOfType<GameTimer>();
        if (gameTimer == null || gameTimer.currentException != GameTimer.TimeExceptionType.SpawnClone)
        {
            return;
        }

        cloneSpawnTimer += Time.deltaTime;
        if (cloneSpawnTimer >= cloneSpawnInterval)
        {
            cloneSpawnTimer = 0;

            // --- 新增的安全检查 ---
            if (playerClonePrefab == null)
            {
                Debug.LogError("错误：PlayerClone Prefab 未在Inspector中指定或已丢失！");
                return; // 终止执行以避免异常
            }
            // --- 检查结束 ---

            if (positionHistory.Count > 0)
            {
                GameObject existingClone = GameObject.FindWithTag("Clone");
                if (existingClone != null)
                {
                    Destroy(existingClone);
                }

                Vector3 spawnPosition = positionHistory.Peek();

                if (Vector3.Distance(spawnPosition, transform.position) < 0.5f)
                {
                    spawnPosition = transform.position - transform.forward * 1.0f;
                }

                // 在实例化之后，再给新生成的对象打上Tag
                GameObject newClone = Instantiate(playerClonePrefab, spawnPosition, transform.rotation);
                newClone.tag = "Clone"; // 确保新实例有正确的Tag

                Debug.Log($"在 {spawnPosition} 位置生成了一个新的分身。");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TeleportWall"))
        {
            TeleportThroughWall();
        }

        if (other.CompareTag("Exception_E"))
        {
            currentException = other.gameObject;
            Debug.Log("可以与异常物体交互: " + currentException.name);
        }
    }

    public void TeleportThroughWall()
    {
        Vector3 oldPos = transform.position;
        Debug.Log("旧的玩家位置：" + oldPos);

        Vector3 newPos = oldPos;
        newPos.x = -oldPos.x;
        newPos.z = oldPos.z - 0.5f;

        characterController.enabled = false; // 用于直接设置位置（推荐）
        transform.position = newPos;
        characterController.enabled = true;

        Debug.Log("新的玩家位置：" + transform.position);

        // 旋转180度
        Vector3 playerEuler = transform.eulerAngles;
        playerEuler.y += 180f;
        transform.rotation = Quaternion.Euler(playerEuler);
    }

    // 检测离开异常物体范围
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Exception_E"))
        {
            if (other.gameObject == currentException)
            {
                currentException = null;
                Debug.Log("离开异常物体交互范围");
            }
        }
    }
}