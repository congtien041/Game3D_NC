using System;
using UnityEngine;
using VehicleSystem.Core;

namespace VehicleSystem.Modules
{
    [RequireComponent(typeof(CarControllerVipro))]
    public class CarNitroSystem : MonoBehaviour
    {
        // Event báo cho UI biết để tự động kết nối thanh Slider
        public static event Action<CarNitroSystem> OnPlayerNitroSpawned;

        [Header("--- THÔNG SỐ BÌNH NITRO ---")]
        public float maxNitro = 100f;              
        public float currentNitro = 0f;          
        [Tooltip("Số lượng Nitro tiêu hao mỗi giây khi bấm Shift")]
        public float nitroDrainRate = 30f;     

        [Header("--- SỨC MẠNH NITRO (+30% Tốc độ) ---")]
        [Tooltip("Tăng 30% giới hạn tốc độ tối đa")]
        public float nitroSpeedMultiplier = 1.3f;  
        [Tooltip("Tăng lực đẩy động cơ để vọt lên nhanh hơn")]
        public float nitroTorqueMultiplier = 1.5f; 

        [Header("--- HỒI NITRO (REGEN) ---")]
        [Tooltip("Hồi Nitro khi chạy bình thường")]
        public float passiveRegenRate = 2f;        
        [Tooltip("Hồi Nitro nhanh khi đang Drift")]
        public float driftRegenRate = 15f;         
        [Tooltip("Lượng Nitro được thưởng ngay lập tức khi xoay 1 vòng")]
        public float spinNitroBonus = 30f;         
        
        public bool IsUsingNitro { get; private set; } = false;

        private CarControllerVipro carController;
        private Rigidbody rb;
        private void Start()
        {
            carController = GetComponent<CarControllerVipro>();
            // Báo hiệu cho UI biết bình Nitro đã sẵn sàng
            OnPlayerNitroSpawned?.Invoke(this); 
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (carController == null || carController.isCountdown || !carController.isEngineOn) return;

            HandleNitroLogic();
        }

        private void HandleNitroLogic()
        {
            bool isPressingGas = Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;
            bool isBraking = Input.GetKey(KeyCode.Space);

            if (Input.GetKey(KeyCode.LeftShift) && !isBraking)
            {
                IsUsingNitro = true;
                currentNitro -= nitroDrainRate * Time.deltaTime;
                if (currentNitro < 0) currentNitro = 0; 
            }
            else
            {
                IsUsingNitro = false;
            }

            // 2. TÍCH LŨY NITRO KHI KHÔNG SỬ DỤNG
            if (!IsUsingNitro)
            {
                // Lấy thông số trượt của bánh sau để kiểm tra Drift
                WheelHit hitL, hitR;
                carController.rearLeftCollider.GetGroundHit(out hitL);
                carController.rearRightCollider.GetGroundHit(out hitR);

                bool isDrifting = (Mathf.Abs(hitL.sidewaysSlip) > 0.3f || Mathf.Abs(hitR.sidewaysSlip) > 0.3f);
                float currentSpeed = rb.linearVelocity.magnitude * 3.6f;

                if (isDrifting)
                {
                    // Đang Drift -> Tích siêu nhanh
                    currentNitro += driftRegenRate * Time.deltaTime; 
                }
                else if (currentSpeed > 5f) 
                {
                    // Xe đang chạy thẳng bình thường (tốc độ > 5km/h) -> Tích chậm
                    currentNitro += passiveRegenRate * Time.deltaTime; 
                }

                if (currentNitro > maxNitro) currentNitro = maxNitro;
            }

            // 3. TRUYỀN SỨC MẠNH XUỐNG BÁNH XE
            if (IsUsingNitro)
            {
                carController.externalSpeedMultiplier = nitroSpeedMultiplier; // Tăng 30% tốc độ
                carController.externalTorqueMultiplier = nitroTorqueMultiplier;
            }
            else
            {
                carController.externalSpeedMultiplier = 1f; // Trả về bình thường
                carController.externalTorqueMultiplier = 1f;
            }
        }

        // Module Xoay xe (Trick) sẽ gọi hàm này để nạp điểm thưởng
        public void AddSpinBonus()
        {
            currentNitro += spinNitroBonus;
            if (currentNitro > maxNitro) currentNitro = maxNitro;
            Debug.Log("<color=cyan>Xoay 1 vòng! Thưởng ngay " + spinNitroBonus + " Nitro!</color>");
        }
    }
}