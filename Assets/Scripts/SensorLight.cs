using UnityEngine;

public class SensorLight : MonoBehaviour
{
    public Light lightSource;       // 你的灯光组件（拖到Inspector）
    public float targetIntensity = 3f; // 点亮时的强度
    public float fadeSpeed = 2f;       // 渐变速度

    private float offIntensity = 0f;
    private bool playerNearby = false;

    void Update()
    {
        // 实现柔和渐亮/渐灭
        float target = playerNearby ? targetIntensity : offIntensity;
        lightSource.intensity = Mathf.Lerp(lightSource.intensity, target, Time.deltaTime * fadeSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // 你的角色Tag需为"Player"
            playerNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }
}