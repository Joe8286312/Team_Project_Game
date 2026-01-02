using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuController : MonoBehaviour
{
    public Slider sliderBGM;
    public Slider sliderSFX;

    void OnEnable()
    {
        // 每次设置界面被激活（显示）自动同步音量到滑块
        if (AudioManager.Instance != null)
        {
            sliderBGM.value = AudioManager.Instance.bgmPercent;
            sliderSFX.value = AudioManager.Instance.sfxPercent;
        }
    }

    // 若滑块支持实时调节，方法如下
    public void OnBgmSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBgmPercent(value);
    }
    public void OnSfxSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSfxPercent(value);
    }
}