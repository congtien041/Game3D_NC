using UnityEngine;
using TMPro; 
using VehicleSystem.Managers; 

namespace VehicleSystem.UI
{
    public class RaceUIManager : MonoBehaviour
    {
        public static RaceUIManager Instance { get; private set; } 
        public TextMeshProUGUI countdownText;
        public TextMeshProUGUI warningText; 

        private void Awake()
        {
            // Thiết lập Singleton
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            GameManager.OnCountdownUpdate += UpdateCountdownText;
            GameManager.OnRaceStart += HandleRaceStart;
        }

        private void OnDisable()
        {
            GameManager.OnCountdownUpdate -= UpdateCountdownText;
            GameManager.OnRaceStart -= HandleRaceStart;
        }

        // ==========================================
        // 1. CHỨC NĂNG CẢNH BÁO (MỚI THÊM)
        // ==========================================
        public void ShowWarning(string message)
        {
            if (warningText != null)
            {
                warningText.gameObject.SetActive(true);
                warningText.text = message;
            }
        }

        public void HideWarning()
        {
            if (warningText != null)
            {
                warningText.gameObject.SetActive(false);
            }
        }

        // ==========================================
        // 2. CHỨC NĂNG ĐẾM NGƯỢC (NHƯ CŨ)
        // ==========================================
        private void UpdateCountdownText(string text)
        {
            if (countdownText != null)
            {
                if (string.IsNullOrEmpty(text))
                {
                    countdownText.gameObject.SetActive(false);
                }
                else
                {
                    countdownText.gameObject.SetActive(true);
                    countdownText.text = text;
                }
            }
        }

        private void HandleRaceStart()
        {
            if (countdownText != null) 
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = "GO!";
                Invoke(nameof(HideText), 1f);
            }
        }

        private void HideText()
        {
            if(countdownText != null) 
            {
                countdownText.gameObject.SetActive(false);
            }
        }
    }
}