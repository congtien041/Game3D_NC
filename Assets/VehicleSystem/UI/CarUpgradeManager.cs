using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VehicleSystem.Data;
using VehicleSystem.Core;

namespace VehicleSystem.UI
{
    public class CarUpgradeManager : MonoBehaviour
    {
        [Header("Data References")]
        public CarDataSO currentCar;       // Dữ liệu gốc của xe đang hiển thị
        public CarUpgradeSave currentSave; // Dữ liệu lưu trữ cấp độ hiện tại của xe

        [Header("Player Resources")]
        public int playerMoney = 15000;    // Tiền hiện có (để test)
        public int playerBlueprints = 10;  // Số lượng blueprint hiện có

        [Header("Resource UI")]
        public TextMeshProUGUI moneyText;
        public TextMeshProUGUI blueprintText; // Hiển thị blueprint bên dưới tên xe

        [Header("UI - Top Speed")]
        public TextMeshProUGUI topSpeedText;
        public Image topSpeedFill;
        public Button topSpeedBtn;
        public TextMeshProUGUI topSpeedCostText; // Text hiển thị giá tiền trên nút

        [Header("UI - Acceleration")]
        public TextMeshProUGUI accelText;
        public Image accelFill;
        public Button accelBtn;
        public TextMeshProUGUI accelCostText;

        [Header("UI - Handling")]
        public TextMeshProUGUI handlingText;
        public Image handlingFill;
        public Button handlingBtn;
        public TextMeshProUGUI handlingCostText;

        [Header("UI - Nitro")]
        public TextMeshProUGUI nitroText;
        public Image nitroFill;
        public Button nitroBtn;
        public TextMeshProUGUI nitroCostText;

        private void Start()
        {
            // Khởi tạo save rỗng để test nếu chưa có hệ thống Load/Save
            if (currentSave == null && currentCar != null)
            {
                currentSave = new CarUpgradeSave { carID = currentCar.carID };
            }
            
            RefreshUI();
        }

        // Hàm này gọi để làm mới toàn bộ số liệu trên màn hình
        public void RefreshUI()
        {
            if (currentCar == null || currentSave == null) return;

            // 1. Cập nhật tài nguyên người chơi
            if (moneyText != null) moneyText.text = playerMoney.ToString();
            if (blueprintText != null) blueprintText.text = playerBlueprints.ToString() + "/50";

            // 2. Lấy chỉ số thực tế sau khi đã cộng dồn level
            CarStatsData currentStats = currentCar.CalculateFinalStats(currentSave);

            // 3. Cập nhật Text chỉ số (Format "F1" lấy 1 số thập phân, "F2" lấy 2 số)
            topSpeedText.text = currentStats.uiTopSpeed.ToString("F1");
            accelText.text = currentStats.uiAcceleration.ToString("F2");
            handlingText.text = currentStats.uiHandling.ToString("F2");
            nitroText.text = currentStats.uiNitro.ToString("F2");

            // 4. Cập nhật thanh màu xanh ngọc (Fill Amount 0 -> 1)
            topSpeedFill.fillAmount = (float)currentSave.topSpeedLevel / currentCar.maxUpgradeLevel;
            accelFill.fillAmount = (float)currentSave.accelerationLevel / currentCar.maxUpgradeLevel;
            handlingFill.fillAmount = (float)currentSave.handlingLevel / currentCar.maxUpgradeLevel;
            nitroFill.fillAmount = (float)currentSave.nitroLevel / currentCar.maxUpgradeLevel;

            // 5. Cập nhật trạng thái Nút và Giá tiền
            UpdateUpgradeButton(topSpeedBtn, topSpeedCostText, currentSave.topSpeedLevel);
            UpdateUpgradeButton(accelBtn, accelCostText, currentSave.accelerationLevel);
            UpdateUpgradeButton(handlingBtn, handlingCostText, currentSave.handlingLevel);
            UpdateUpgradeButton(nitroBtn, nitroCostText, currentSave.nitroLevel);
        }

        // Hàm hỗ trợ kiểm tra nút có bấm được không và hiển thị giá
        private void UpdateUpgradeButton(Button btn, TextMeshProUGUI costText, int currentLevel)
        {
            if (currentLevel >= currentCar.maxUpgradeLevel)
            {
                if (costText != null) costText.text = "MAX";
                btn.interactable = false;
            }
            else
            {
                int cost = currentCar.GetUpgradeCost(currentLevel);
                if (costText != null) costText.text = cost.ToString();
                
                // Nút chỉ bấm được khi đủ tiền
                btn.interactable = (playerMoney >= cost);
            }
        }

        // --- CÁC HÀM GẮN VÀO SỰ KIỆN ONCLICK() CỦA NÚT ---

        public void UpgradeTopSpeed()
        {
            int cost = currentCar.GetUpgradeCost(currentSave.topSpeedLevel);
            if (playerMoney >= cost && currentSave.topSpeedLevel < currentCar.maxUpgradeLevel)
            {
                playerMoney -= cost;
                currentSave.topSpeedLevel++;
                RefreshUI();
            }
        }

        public void UpgradeAcceleration()
        {
            int cost = currentCar.GetUpgradeCost(currentSave.accelerationLevel);
            if (playerMoney >= cost && currentSave.accelerationLevel < currentCar.maxUpgradeLevel)
            {
                playerMoney -= cost;
                currentSave.accelerationLevel++;
                RefreshUI();
            }
        }

        public void UpgradeHandling()
        {
            int cost = currentCar.GetUpgradeCost(currentSave.handlingLevel);
            if (playerMoney >= cost && currentSave.handlingLevel < currentCar.maxUpgradeLevel)
            {
                playerMoney -= cost;
                currentSave.handlingLevel++;
                RefreshUI();
            }
        }

        public void UpgradeNitro()
        {
            int cost = currentCar.GetUpgradeCost(currentSave.nitroLevel);
            if (playerMoney >= cost && currentSave.nitroLevel < currentCar.maxUpgradeLevel)
            {
                playerMoney -= cost;
                currentSave.nitroLevel++;
                RefreshUI();
            }
        }

        // Hàm dùng để đổi xe khác trên UI
        public void LoadNewCarData(CarDataSO newCar, CarUpgradeSave newSave)
        {
            currentCar = newCar;
            currentSave = newSave;
            RefreshUI();
        }
    }
}