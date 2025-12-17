using System.Collections.Generic;
using UnityEngine;

public class PlayerAnomalyDetector : MonoBehaviour
{
    [Header("通用设置")]
    public Transform cameraTransform;

    [Header("注视异常 (See)")]
    public float checkDistance = 10f;
    public float seeDuration = 3f;
    private float seeTimer = 0f;
    private GameObject currentGazingObj = null;

    [Header("聆听异常 (Listen)")]
    public float listenDistance = 3f;
    public float listenDuration = 3f;
    private float listenTimer = 0f;
    private GameObject currentListeningObj = null;

    // 本地缓存已发现的异常
    private HashSet<GameObject> discoveredAnomalies = new HashSet<GameObject>();

    void Update()
    {
        if (GameManager.IsGamePaused) return;

        HandleSeeAnomaly();
        HandleListenAnomaly();
    }

    // 处理 "盯着看" 的异常 (Tag: Exception_See3s)
    private void HandleSeeAnomaly()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, checkDistance) && hit.collider.CompareTag("Exception_See3s"))
        {
            GameObject target = hit.collider.gameObject;

            if (discoveredAnomalies.Contains(target)) return;

            if (currentGazingObj != target)
            {
                currentGazingObj = target;
                seeTimer = 0f;
            }

            seeTimer += Time.deltaTime;
            if (seeTimer >= seeDuration)
            {
                ReportDiscovery(target);
            }
        }
        else
        {
            currentGazingObj = null;
            seeTimer = 0f;
        }
    }

    // 处理 "靠近听" 的异常 (Tag: Exception_Listen3s)
    private void HandleListenAnomaly()
    {
        // 使用 OverlapSphere 检测周围
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, listenDistance);
        GameObject bestCandidate = null;

        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Exception_Listen3s"))
            {
                bestCandidate = hit.gameObject;
                break;
            }
        }

        if (bestCandidate != null && !discoveredAnomalies.Contains(bestCandidate))
        {
            if (currentListeningObj != bestCandidate)
            {
                currentListeningObj = bestCandidate;
                listenTimer = 0f;
            }

            listenTimer += Time.deltaTime;
            if (listenTimer >= listenDuration)
            {
                ReportDiscovery(bestCandidate);
            }
        }
        else
        {
            currentListeningObj = null;
            listenTimer = 0f;
        }
    }

    private void ReportDiscovery(GameObject anomaly)
    {
        if (!discoveredAnomalies.Contains(anomaly))
        {
            discoveredAnomalies.Add(anomaly);
            Debug.Log($"成功发现被动异常: {anomaly.name}");

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ReportAnomalyFound();
            }
        }
    }

    public void ClearRecords()
    {
        discoveredAnomalies.Clear();
    }
}