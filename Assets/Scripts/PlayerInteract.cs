using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("交互设置")]
    public float interactDistance = 3.0f;
    public LayerMask interactLayer; // 确保包含交互物品的Layer
    public Transform cameraTransform;

    // 本地缓存，防止重复点击同一个异常加分
    private HashSet<GameObject> interactedAnomalies = new HashSet<GameObject>();

    void Update()
    {
        if (GameManager.IsGamePaused) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckInteraction();
        }
    }

    void CheckInteraction()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            GameObject hitObj = hit.collider.gameObject;

            // 1. 常规过关出口 (进入下一层)
            if (hitObj.CompareTag("E") || hitObj.CompareTag("NextLevel"))
            {
                Debug.Log("尝试通关前往下一层...");
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.AttemptTransition();
                }
            }

            // 2. 特殊门 (Exception_E) - 你的新需求
            else if (hitObj.CompareTag("Exception_E"))
            {
                Debug.Log("与特殊门交互...");
                if (LevelManager.Instance != null)
                {
                    //LevelManager.Instance.HandleSpecialDoorInteraction();
                }
            }

            //// 3. 相片碎片道具 (新增 Tag: "PhotoFragment")
            //else if (hitObj.CompareTag("PhotoFragment"))
            //{
            //    Debug.Log("捡起相片碎片");
            //    if (LevelManager.Instance != null)
            //    {
            //        //LevelManager.Instance.CollectPhotoFragment();
            //    }
            //    Destroy(hitObj); // 收集后销毁物体
            //}

            //// 4. 其他普通交互型异常 (比如墙上的画歪了，如果你还有其他交互异常)
            //// 你可能需要为这些定义一个新的 Tag，例如 "Anomaly_Interact"
            //// 只有这些才算作“发现异常”
            //else if (hitObj.CompareTag("Anomaly_Interact"))
            //{
            //    if (!interactedAnomalies.Contains(hitObj))
            //    {
            //        interactedAnomalies.Add(hitObj);
            //        Debug.Log($"手动消除/发现异常: {hitObj.name}");

            //        if (LevelManager.Instance != null)
            //        {
            //            LevelManager.Instance.ReportAnomalyFound();
            //        }
            //        // Destroy(hitObj); // 如果需要消除
            //    }
            //}
        }
    }

    // 切换场景时调用此方法清空记录
    public void ClearRecords()
    {
        interactedAnomalies.Clear();
    }
}