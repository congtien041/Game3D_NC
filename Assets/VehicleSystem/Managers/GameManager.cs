using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VehicleSystem.Core;
using TMPro; // Dùng để gọi sang CarControllerVipro

namespace VehicleSystem.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Header("UI Settings")]
        public TextMeshProUGUI countdownText;

        [Header("Player Settings")]
        public CarControllerVipro playerCar; // Kéo chiếc xe vào đây
        public CameraCinematic mainCamera;
        private void Start()
        {
            // Vừa vào game là bắt đầu đếm ngược luôn
            StartCoroutine(CountdownRoutine());
        }

        private IEnumerator CountdownRoutine()
        {
            if (mainCamera != null) mainCamera.isCinematic = true;
            if (playerCar != null)
            {
                playerCar.isEngineOn = false;
            }

            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                
                countdownText.text = "3";
                yield return new WaitForSeconds(2f);
                
                countdownText.text = "2";
                yield return new WaitForSeconds(2f);
                
                countdownText.text = "1";
                yield return new WaitForSeconds(2f);
                
                countdownText.text = "GO!";
                
                if (playerCar != null)
                {
                    playerCar.isEngineOn = true;
                }
                if (mainCamera != null) mainCamera.StartGameplay();
                
                yield return new WaitForSeconds(1f);
                countdownText.gameObject.SetActive(false);
            }
        }
    }
}