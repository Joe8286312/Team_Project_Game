using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundAnomaly : MonoBehaviour
{
    // public float detectRadius = 3f; // 玩家靠近多少米内才播放
    private AudioSource audioSource;
    private Transform player;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D音效
        audioSource.playOnAwake = false;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void OnEnable()
    {
        GameManager.OnPauseStateChanged += OnPauseChanged;
    }

    void OnDisable()
    {
        GameManager.OnPauseStateChanged -= OnPauseChanged;
    }

    private void OnPauseChanged(bool isPaused)
    {
        if (audioSource == null) return;
        if (isPaused)
            audioSource.Pause();
        else
            audioSource.UnPause();
    }

    void Update()
    {
        // 你的异常音效播放逻辑...
    }
}