using System;
using UnityEngine;
using VehicleSystem.Core;

namespace VehicleSystem.Modules
{
    [RequireComponent(typeof(CarControllerVipro))]
    public class CarNitroSystem : MonoBehaviour
    {
        // Event báo cho UI biết xe đã xuất hiện (Tối ưu hóa)
        public static event Action<CarNitroSystem> OnPlayerNitroSpawned;

        [Header("--- THÔNG SỐ BÌNH NITRO ---")]
        public float maxNitro = 100f;              
        public float currentNitro = 30f;          
        [Tooltip("Số Nitro tốn cho 1 lần bấm bứt tốc")]
        public float nitroCostPerBoost = 30f;     
        
        [Header("--- SỨC MẠNH BURST NITRO ---")]
        [Tooltip("Thời gian bứt tốc kéo dài bao nhiêu giây?")]
        public float boostDuration = 2f; 
        [Tooltip("Lực đẩy nhân lên cực mạnh (VD: 2.5 lần)")]
        public float nitroTorqueMultiplier = 2.5f; 
        [Tooltip("Giới hạn tốc độ mở rộng (VD: 1.6 lần)")]
        public float nitroSpeedMultiplier = 1.6f;  

        [Header("--- HỒI NITRO (REGEN) ---")]
        public float passiveRegenRate = 2f;        
        public float driftRegenRate = 15f;         
        
        public bool IsUsingNitro { get; private set; } = false;

        private CarControllerVipro carController;
        private bool isDrifting = false;
        private float boostTimer = 0f; // Đồng hồ đếm ngược thời gian bứt tốc
        private Rigidbody rb;
        private void Start()
        {
            OnPlayerNitroSpawned?.Invoke(this); 
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (carController == null || carController.isCountdown || !carController.isEngineOn) return;

            HandleNitroBurstInput();
            CheckDriftAndRegen();
            ApplyMultipliersToCar();
        }

        private void HandleNitroBurstInput()
        {
            if (IsUsingNitro)
            {
                boostTimer -= Time.deltaTime;
                if (boostTimer <= 0f)
                {
                    IsUsingNitro = false; 
                }
                return; 
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && currentNitro >= nitroCostPerBoost)
            {
                IsUsingNitro = true;
                boostTimer = boostDuration;       
                currentNitro -= nitroCostPerBoost; 
                
                rb.AddForce(carController.transform.forward * 10f, ForceMode.VelocityChange);
            }
        }

        private void CheckDriftAndRegen()
        {
            WheelHit hitL, hitR;
            carController.rearLeftCollider.GetGroundHit(out hitL);
            carController.rearRightCollider.GetGroundHit(out hitR);

            isDrifting = (Mathf.Abs(hitL.sidewaysSlip) > 0.3f || Mathf.Abs(hitR.sidewaysSlip) > 0.3f);

            if (!IsUsingNitro)
            {
                if (isDrifting) currentNitro += driftRegenRate * Time.deltaTime; 
                else currentNitro += passiveRegenRate * Time.deltaTime; 

                if (currentNitro > maxNitro) currentNitro = maxNitro;
            }
        }

        private void ApplyMultipliersToCar()
        {
            if (IsUsingNitro)
            {
                carController.externalSpeedMultiplier = nitroSpeedMultiplier;
                carController.externalTorqueMultiplier = nitroTorqueMultiplier;
            }
            else
            {
                carController.externalSpeedMultiplier = 1f;
                carController.externalTorqueMultiplier = 1f;
            }
        }

        public void AddNitro(float amount)
        {
            currentNitro += amount;
            if (currentNitro > maxNitro) currentNitro = maxNitro;
        }
    }
}