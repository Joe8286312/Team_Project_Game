using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float mouseSensitivity = 100f;
    public Animator animator;
    public Transform cameraTransform; // 拖你摄像机进来

    float cameraPitch = 0f;

    void Update()
    {
        // 1. 键盘移动
        float moveZ = Input.GetKey(KeyCode.W) ? 1 : (Input.GetKey(KeyCode.S) ? -1 : 0);
        float moveX = Input.GetKey(KeyCode.D) ? 1 : (Input.GetKey(KeyCode.A) ? -1 : 0);
        Vector3 move = transform.forward * moveZ + transform.right * moveX;
        move.Normalize();

        // 2. 动画控制
        bool walking = moveZ != 0 || moveX != 0;
        animator.SetBool("isWalking", walking);
        animator.SetBool("isWalkingBack", moveZ < 0);

        // 3. 实际移动
        transform.position += move * moveSpeed * Time.deltaTime;

        // 4. 鼠标控制视角
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Y轴旋转角色
        transform.Rotate(Vector3.up * mouseX);

        // X轴旋转摄像机（上下视角，限制俯仰角度）
        cameraPitch -= mouseY; // 注意减号
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f); // 限制角度
        cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0, 0);
    }
}