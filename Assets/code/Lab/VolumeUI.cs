using UnityEngine;
using UnityEngine.UI; 
public class VolumeUI : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        volumeSlider.value = GameDataManager.instance.musicVolume;

        volumeSlider.onValueChanged.AddListener(delegate { OnSliderChanged(); });
    }

    public void OnSliderChanged()
    {
        GameDataManager.instance.musicVolume = volumeSlider.value;
        
    }
}