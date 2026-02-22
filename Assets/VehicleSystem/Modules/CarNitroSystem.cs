using UnityEngine;
using VehicleSystem.Core;

namespace VehicleSystem.Modules
{
    [RequireComponent(typeof(CarControllerVipro))]
    public class CarNitroSystem : MonoBehaviour
    {
        [Header("--- THÔNG SỐ BÌNH NITRO ---")]
        public float maxNitro = 100f;              
        public float currentNitro = 100f;          
        public float nitroDrainRate = 25f;         
        
        [Header("--- SỨC MẠNH NITRO ---")]
        public float nitroTorqueMultiplier = 1.5f; 
        public float nitroSpeedMultiplier = 1.3f;  

        [Header("--- HỒI NITRO (REGEN) ---")]
        public float passiveRegenRate = 2f;        
        public float driftRegenRate = 15f;         
        
        public bool IsUsingNitro { get; private set; } = false;

        private CarControllerVipro carController;
        private bool isDrifting = false;

        private void Start()
        {
            carController = GetComponent<CarControllerVipro>();
            currentNitro = maxNitro; 
        }

        private void Update()
        {
            if (carController == null || carController.isCountdown || !carController.isEngineOn) return;

            HandleNitroInput();
            CheckDriftAndRegen();
            ApplyMultipliersToCar();
        }

        private void HandleNitroInput()
        {
            float verticalInput = Input.GetAxis("Vertical");
            bool isPressingGas = verticalInput > 0.1f;
            bool isBraking = Input.GetKey(KeyCode.Space);

            // Bấm Shift + Bấm Ga + Không đạp phanh + Còn Nitro
            if (Input.GetKey(KeyCode.LeftShift) && isPressingGas && !isBraking && currentNitro > 0)
            {
                IsUsingNitro = true;
                currentNitro -= nitroDrainRate * Time.deltaTime;
                if (currentNitro < 0) currentNitro = 0; 
            }
            else
            {
                IsUsingNitro = false;
            }
        }

        private void CheckDriftAndRegen()
        {
            // Kiểm tra Drift thông qua bánh sau
            WheelHit hitL, hitR;
            carController.rearLeftCollider.GetGroundHit(out hitL);
            carController.rearRightCollider.GetGroundHit(out hitR);

            isDrifting = (Mathf.Abs(hitL.sidewaysSlip) > 0.3f || Mathf.Abs(hitR.sidewaysSlip) > 0.3f);

            // Chỉ hồi Nitro khi không sử dụng
            if (!IsUsingNitro)
            {
                if (isDrifting) currentNitro += driftRegenRate * Time.deltaTime; 
                else currentNitro += passiveRegenRate * Time.deltaTime; 

                if (currentNitro > maxNitro) currentNitro = maxNitro;
            }
        }

        private void ApplyMultipliersToCar()
        {
            // Truyền thông số sang Core
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

        // Các Script khác (như đồ nhặt trên map) sẽ gọi hàm này
        public void AddNitro(float amount)
        {
            currentNitro += amount;
            if (currentNitro > maxNitro) currentNitro = maxNitro;
        }
    }
}