// 新建文件 PlayerCloneFade.cs
using UnityEngine;

public class PlayerCloneFade : MonoBehaviour
{
    public float fadeDuration = 2f;
    private float timer = 0f;
    private Renderer rend;

    void Start()
    {
        rend = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > fadeDuration && rend != null)
        {
            float alpha = Mathf.Lerp(1f, 0f, (timer - fadeDuration) / fadeDuration);
            Color c = rend.material.color;
            c.a = alpha;
            rend.material.color = c;
        }
    }
}