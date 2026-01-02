using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    private Dictionary<string, AudioClip> bgmClips = new();
    private Dictionary<string, AudioClip> sfxClips = new();
    private Dictionary<string, float> bgmBaseVolumes = new();
    private Dictionary<string, float> sfxBaseVolumes = new();

    // ==== 玩家调节的全局百分比 [0, 1f] ====
    public float bgmPercent { get; private set; } = 1f;
    public float sfxPercent { get; private set; } = 1f;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        foreach (var clip in Resources.LoadAll<AudioClip>("Audios/BGM"))
        {
            bgmClips[clip.name] = clip;
            bgmBaseVolumes[clip.name] = 1f; // 默认值
            Debug.Log("加载BGM资源：" + string.Join(",", bgmClips.Keys));
        }
        foreach (var clip in Resources.LoadAll<AudioClip>("Audios/SFX"))
        {
            sfxClips[clip.name] = clip;
            sfxBaseVolumes[clip.name] = 1f; // 默认值
            Debug.Log("加载SFX资源：" + string.Join(",", sfxClips.Keys));
        }

        // 在这里你可以初始化部分资源的基础音量值
        // bgmBaseVolumes["BGM-nop"] = 0.5f;
        // ...

        LoadVolumeSettings();
    }

    // ========== 新增，支持自定义音量 ===========
    public void SetBgmBaseVolume(string name, float baseVol)
    {
        if (bgmBaseVolumes.ContainsKey(name))
            bgmBaseVolumes[name] = Mathf.Clamp01(baseVol);
    }
    public void SetSfxBaseVolume(string name, float baseVol)
    {
        if (sfxBaseVolumes.ContainsKey(name))
            sfxBaseVolumes[name] = Mathf.Clamp01(baseVol);
    }

    // ================= 全局百分比接口 ===============
    public void SetBgmPercent(float percent)
    {
        bgmPercent = Mathf.Clamp01(percent);
        if (bgmSource != null) bgmSource.volume = GetCurrentBgmVolume();
        PlayerPrefs.SetFloat("BGMPercent", bgmPercent);
    }
    public void SetSfxPercent(float percent)
    {
        sfxPercent = Mathf.Clamp01(percent);
        PlayerPrefs.SetFloat("SFXPercent", sfxPercent);
    }
    private void LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey("BGMPercent")) bgmPercent = PlayerPrefs.GetFloat("BGMPercent");
        if (PlayerPrefs.HasKey("SFXPercent")) sfxPercent = PlayerPrefs.GetFloat("SFXPercent");
    }
    // =========== 播放BGM  支持开发者指定基础音量 ===========
    public void PlayBGM(string name, float? customBaseVol = null, bool loop = true)
    {
        if (!bgmClips.TryGetValue(name, out var clip)) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;

        float baseVol = customBaseVol.HasValue
            ? Mathf.Clamp01(customBaseVol.Value)
            : (bgmBaseVolumes.ContainsKey(name) ? bgmBaseVolumes[name] : 1f);

        bgmSource.volume = baseVol * bgmPercent;
        bgmSource.Play();
    }
    public void StopBGM() => bgmSource.Stop();
    // ========== 播放SFX ==============
    public void PlaySFX(string name, float? overrideBaseVol = null, float volumeMul = 1f)
    {
        if (!sfxClips.TryGetValue(name, out var clip)) return;
        float baseVol = overrideBaseVol.HasValue
            ? Mathf.Clamp01(overrideBaseVol.Value)
            : (sfxBaseVolumes.ContainsKey(name) ? sfxBaseVolumes[name] : 1f);
        sfxSource.PlayOneShot(clip, baseVol * sfxPercent * volumeMul);
    }

    // 当前BGM最终音量（暴露给设置界面用滑块同步显示）
    public float GetCurrentBgmVolume()
    {
        if (bgmSource.clip == null) return 0f;
        string name = bgmSource.clip.name;
        float baseVol = bgmBaseVolumes.ContainsKey(name) ? bgmBaseVolumes[name] : 1f;
        return baseVol * bgmPercent;
    }
}