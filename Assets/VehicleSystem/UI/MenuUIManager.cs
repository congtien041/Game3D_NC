using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VehicleSystem.Core;
using VehicleSystem.UI;

namespace VehicleSystem.Data
{
    public class MenuUIManager : MonoBehaviour
    {
        public CarDataSO[] allCars; 
        private int currentIndex = 0; 
        private float maxSliderValue = 800f; 
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

            // 1. TẢI DỮ LIỆU NÂNG CẤP CỦA XE NÀY
            CarUpgradeSave save = LoadCarSave(carData.carID);

            // 2. TÍNH TOÁN CHỈ SỐ CUỐI CÙNG (GỐC + NÂNG CẤP)
            CarStatsData finalStats = carData.CalculateFinalStats(save);

            // 3. HIỂN THỊ CHỈ SỐ ĐÃ TÍNH TOÁN (Thay vì baseTopSpeed)
            stat1Slider.value = finalStats.uiTopSpeed;
            stat2Slider.value = finalStats.uiAcceleration;
            stat3Slider.value = finalStats.uiHandling;
            stat4Slider.value = finalStats.uiNitro;

            stat1Text.text = finalStats.uiTopSpeed.ToString("0");
            stat2Text.text = finalStats.uiAcceleration.ToString("0");
            stat3Text.text = finalStats.uiHandling.ToString("0");
            stat4Text.text = finalStats.uiNitro.ToString("0");

            if (CarUpgradeManager.Instance != null)
            {
                CarUpgradeManager.Instance.SetCurrentCar(carData);
            }
        }

        // Hàm hỗ trợ tự động tải file Save dựa vào ID của xe
        private CarUpgradeSave LoadCarSave(string carID)
        {
            string saveKey = "CarSave_" + carID;
            if (PlayerPrefs.HasKey(saveKey))
            {
                string json = PlayerPrefs.GetString(saveKey);
                return JsonUtility.FromJson<CarUpgradeSave>(json);
            }
            return new CarUpgradeSave { carID = carID }; // Trả về level 0 nếu xe chưa từng được nâng cấp
        }
    }
}