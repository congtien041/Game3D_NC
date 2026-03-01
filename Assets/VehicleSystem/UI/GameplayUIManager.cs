using UnityEngine;
using TMPro;
using VehicleSystem.Managers;

namespace VehicleSystem.UI
{
    public class GameplayUIManager : MonoBehaviour
    {
        // [Header("--- PANELS ---")]
        // public GameObject countdownPanel;
        // public GameObject racingPanel;
        // public GameObject finishedPanel;

        [Header("--- TEXT REFERENCES ---")]
        public TextMeshProUGUI countdownText;
        public TextMeshProUGUI mainInfoText; // <--- CHỈ CẦN 1 TEXT NÀY DÙNG CHUNG CHO MỌI MODE

        private void OnEnable()
        {
            // GameManager.OnStateChanged += HandleStateChanged;
            GameManager.OnCountdownUpdate += UpdateCountdownText;
        }

        private void OnDisable()
        {
            // GameManager.OnStateChanged -= HandleStateChanged;
            GameManager.OnCountdownUpdate -= UpdateCountdownText;
        }

        // private void HandleStateChanged(GameState state)
        // {
        //     countdownPanel.SetActive(false);
        //     racingPanel.SetActive(false);
        //     finishedPanel.SetActive(false);

        //     switch (state)
        //     {
        //         case GameState.Countdown:
        //             countdownPanel.SetActive(true);
        //             break;
        //         case GameState.Racing:
        //             racingPanel.SetActive(true);
        //             break;
        //         case GameState.Finished:
        //             finishedPanel.SetActive(true);
        //             break;
        //     }
        // }

        private void UpdateCountdownText(string text)
        {
            if (countdownText != null) countdownText.text = text;
        }

        // Nhận chuỗi string đã được chuẩn bị sẵn từ RaceModeManager và in ra màn hình
        public void UpdateMainInfoText(string infoString)
        {
            if (mainInfoText != null)
            {
                mainInfoText.text = infoString;
            }
        }
    }
}