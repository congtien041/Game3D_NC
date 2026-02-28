using UnityEngine;
using VehicleSystem.Core;
using VehicleSystem.Managers;

namespace VehicleSystem.Modules
{
    [RequireComponent(typeof(CarControllerVipro))]
    public class CarRaceListener : MonoBehaviour
    {
        private CarControllerVipro carController;
        private CameraCinematic cameraCinematic;
        private void Awake()
        {
            carController = GetComponent<CarControllerVipro>();
            
            if (carController != null)
            {
                carController.isEngineOn = false;
                carController.isCountdown = true;
            }
        }
        void Start()
        {
            cameraCinematic = FindAnyObjectByType<CameraCinematic>();
        }
        private void OnEnable()
        {
            GameManager.OnRaceStart += StartEngine;
        }

        private void OnDisable()
        {
            GameManager.OnRaceStart -= StartEngine;
        }

        private void StartEngine()
        {
            if (carController != null)
            {
                carController.isEngineOn = true;
                carController.isCountdown = false;
                cameraCinematic.isCinematic = false;
                Debug.Log("<color=green>XE ĐÃ NỔ MÁY VÀ SẴN SÀNG CHẠY!</color>");
            }
        }
    }
}