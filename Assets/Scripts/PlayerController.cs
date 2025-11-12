using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;
    private float verticalRotation = 0f;
    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
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
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TeleportWall"))
        {
            TeleportThroughWall();
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
}