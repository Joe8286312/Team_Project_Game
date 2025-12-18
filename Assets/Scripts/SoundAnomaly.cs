using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundAnomaly : MonoBehaviour
{
    public float detectRadius = 3f; // 玩家靠近多少米内才播放
    private AudioSource audioSource;
    private Transform player;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D音效
        audioSource.playOnAwake = false;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= detectRadius)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}