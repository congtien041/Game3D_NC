using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VehicleSystem.Core; 

namespace VehicleSystem.Data
{
    public class MenuUIManager : MonoBehaviour
    {
        public CarDataSO[] allCars; 
        private int currentIndex = 0; 
        private float maxSliderValue = 500f; 
        public Image carIconImage;        
        public TextMeshProUGUI carNameText; 
        public Slider stat1Slider; 
        public Slider stat2Slider;
        public Slider stat3Slider;
        public Slider stat4Slider;
        public TextMeshProUGUI stat1Text; 
        public TextMeshProUGUI stat2Text;
        public TextMeshProUGUI stat3Text;
        public TextMeshProUGUI stat4Text;

        private void Start()
        {
            SetupSlidersLimits();

            if (allCars != null && allCars.Length > 0)
            {
                DisplayCar(allCars[currentIndex]);
            }
        }

        private void SetupSlidersLimits()
        {
            if (stat1Slider) stat1Slider.maxValue = maxSliderValue;
            if (stat2Slider) stat2Slider.maxValue = maxSliderValue;
            if (stat3Slider) stat3Slider.maxValue = maxSliderValue;
            if (stat4Slider) stat4Slider.maxValue = maxSliderValue;
        }

        public void NextCar()
        {
            if (allCars == null || allCars.Length == 0) return;
            
            currentIndex++;
            if (currentIndex >= allCars.Length) currentIndex = 0; 
            
            DisplayCar(allCars[currentIndex]);
        }

        public void PrevCar()
        {
            if (allCars == null || allCars.Length == 0) return;

            currentIndex--;
            if (currentIndex < 0) currentIndex = allCars.Length - 1; 
            
            DisplayCar(allCars[currentIndex]);
        }

        public void DisplayCar(CarDataSO carData) 
        {
            if (carData == null) return;
            carNameText.text = carData.carName;
            if (carIconImage != null && carData.carIcon != null) { carIconImage.sprite = carData.carIcon; }
            stat1Slider.value = carData.baseTopSpeed;
            stat2Slider.value = carData.baseAcceleration;
            stat3Slider.value = carData.baseHandling;
            stat4Slider.value = carData.baseNitro;
            stat1Text.text = carData.baseTopSpeed.ToString("0");
            stat2Text.text = carData.baseAcceleration.ToString("0");
            stat3Text.text = carData.baseHandling.ToString("0");
            stat4Text.text = carData.baseNitro.ToString("0");
        }
    }
}