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


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
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