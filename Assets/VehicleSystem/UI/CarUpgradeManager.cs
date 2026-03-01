using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VehicleSystem.Data;
using VehicleSystem.Core;
using VehicleSystem.Managers;

namespace VehicleSystem.UI
{
    public class CarUpgradeManager : MonoBehaviour
    {
        [Header("Data References")]
        // Đổi thành private, không cần kéo thả trong Editor nữa
        private CarDataSO currentCar; 
        private CarUpgradeSave currentSave;

        [Header("Shared Buttons (Menu & Upgrade)")]
        public Button upgradeBtn;
        public Button garageBtn;
        public Button modeBtn;
        public Button playBtn;
        
        [Header("Extra Buttons")]
        public Button backBtn;

        [Header("Button Text References")]
        public TextMeshProUGUI upgradeBtnText;
        public TextMeshProUGUI garageBtnText;
        public TextMeshProUGUI modeBtnText;
        public TextMeshProUGUI playBtnText;

        [Header("Stats UI")]
        public TextMeshProUGUI topSpeedText;
        public Image topSpeedFill;
        public TextMeshProUGUI accelText;
        public Image accelFill;
        public TextMeshProUGUI handlingText;
        public Image handlingFill;
        public TextMeshProUGUI nitroText;
        public Image nitroFill;

        private bool isUpgradeMode = false;
        public static CarUpgradeManager Instance { get; private set; }
        public bool IsUpgradeMode => isUpgradeMode;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (backBtn != null)
                backBtn.onClick.AddListener(ExitUpgradeMode);

            ShowMenuUI();
        }

        // ---- HÀM MỚI: Nhận xe từ MenuUIManager truyền sang ----
        public void SetCurrentCar(CarDataSO carData)
        {
            currentCar = carData;
            LoadCarData(); // Tải file save của chiếc xe mới này
            
            // Nếu đang mở bảng nâng cấp mà đổi xe thì update luôn UI
            if (isUpgradeMode) 
            {
                RefreshUI();
            }
        }

        public void EnterUpgradeMode()
        {
            isUpgradeMode = true;
            SetupUpgradeButtons();
            
            if (backBtn != null) backBtn.gameObject.SetActive(true); 
        }

        public void ExitUpgradeMode()
        {
            isUpgradeMode = false;
            ShowMenuUI();
            
            if (backBtn != null) backBtn.gameObject.SetActive(false); 
        }

        public void SetupUpgradeButtons()
        {
            ClearAllButtonListeners();

            upgradeBtn.onClick.AddListener(UpgradeTopSpeed);
            garageBtn.onClick.AddListener(UpgradeAcceleration);
            modeBtn.onClick.AddListener(UpgradeHandling);
            playBtn.onClick.AddListener(UpgradeNitro);

            RefreshUI();
        }

        public void ShowMenuUI()
        {
            ClearAllButtonListeners();

            upgradeBtn.onClick.AddListener(EnterUpgradeMode);
            // garageBtn.onClick.AddListener(OpenGarage);
            // modeBtn.onClick.AddListener(OpenMode);
            // playBtn.onClick.AddListener(PlayGame);

            upgradeBtnText.text = "UPGRADE";
            garageBtnText.text = "GARAGE";
            modeBtnText.text = "MODE";
            playBtnText.text = "PLAY";

            upgradeBtn.interactable = true;
            garageBtn.interactable = true;
            modeBtn.interactable = true;
            playBtn.interactable = true;
        }

        private void ClearAllButtonListeners()
        {
            upgradeBtn.onClick.RemoveAllListeners();
            garageBtn.onClick.RemoveAllListeners();
            modeBtn.onClick.RemoveAllListeners();
            playBtn.onClick.RemoveAllListeners();
        }

        public void RefreshUI()
        {
            if (!isUpgradeMode || currentCar == null || currentSave == null) return;

            int playerMoney = MoneyManager.Instance.GetBalance();

            CarStatsData currentStats = currentCar.CalculateFinalStats(currentSave);
            topSpeedText.text = currentStats.uiTopSpeed.ToString("F1");
            accelText.text = currentStats.uiAcceleration.ToString("F2");
            handlingText.text = currentStats.uiHandling.ToString("F2");
            nitroText.text = currentStats.uiNitro.ToString("F2");

            topSpeedFill.fillAmount = (float)currentSave.topSpeedLevel / currentCar.maxUpgradeLevel;
            accelFill.fillAmount = (float)currentSave.accelerationLevel / currentCar.maxUpgradeLevel;
            handlingFill.fillAmount = (float)currentSave.handlingLevel / currentCar.maxUpgradeLevel;
            nitroFill.fillAmount = (float)currentSave.nitroLevel / currentCar.maxUpgradeLevel;

            UpdateButtonState(upgradeBtn, upgradeBtnText, "TOP SPEED", currentSave.topSpeedLevel, playerMoney);
            UpdateButtonState(garageBtn, garageBtnText, "ACCEL", currentSave.accelerationLevel, playerMoney);
            UpdateButtonState(modeBtn, modeBtnText, "HANDLING", currentSave.handlingLevel, playerMoney);
            UpdateButtonState(playBtn, playBtnText, "NITRO", currentSave.nitroLevel, playerMoney);
        }

        private void UpdateButtonState(Button btn, TextMeshProUGUI btnText, string statName, int level, int money)
        {
            if (level >= currentCar.maxUpgradeLevel)
            {
                btnText.text = statName + "\nMAX";
                btn.interactable = false;
            }
            else
            {
                int cost = currentCar.GetUpgradeCost(level);
                btnText.text = $"{statName}\n${cost}";
                btn.interactable = (money >= cost);
            }
        }

        public void UpgradeTopSpeed() => PerformUpgrade(ref currentSave.topSpeedLevel);
        public void UpgradeAcceleration() => PerformUpgrade(ref currentSave.accelerationLevel);
        public void UpgradeHandling() => PerformUpgrade(ref currentSave.handlingLevel);
        public void UpgradeNitro() => PerformUpgrade(ref currentSave.nitroLevel);

        private void LoadCarData()
        {
            if (currentCar == null) return;
            string saveKey = "CarSave_" + currentCar.carID;

            if (PlayerPrefs.HasKey(saveKey))
            {
                string json = PlayerPrefs.GetString(saveKey);
                currentSave = JsonUtility.FromJson<CarUpgradeSave>(json);
            }
            else
            {
                currentSave = new CarUpgradeSave { carID = currentCar.carID };
            }
        }

        private void SaveCarData()
        {
            if (currentSave == null) return;
            string saveKey = "CarSave_" + currentSave.carID;
            string json = JsonUtility.ToJson(currentSave);
            
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
        }

        private void PerformUpgrade(ref int level)
        {
            int cost = currentCar.GetUpgradeCost(level);
            if (MoneyManager.Instance.SpendMoney(cost))
            {
                level++;
                SaveCarData(); 
                RefreshUI();
                
                // ---- DÒNG NÀY ĐỂ BÁO LẠI CHO MENU UPDATE THANH SLIDER BÊN NGOÀI ----
                FindObjectOfType<MenuUIManager>().DisplayCar(currentCar);
            }
        }
    }
}