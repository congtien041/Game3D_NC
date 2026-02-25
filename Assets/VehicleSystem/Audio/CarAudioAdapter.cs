using UnityEngine;
using VehicleSystem.Core;

namespace VehicleSystem.Audio
{
    [RequireComponent(typeof(CarControllerVipro))]
    public class CarAudioAdapter : MonoBehaviour
    {
        private float minPitch = 0.8f; 
        private float maxPitch = 2.5f; 

        private CarControllerVipro carController;
        private GlobalAudio audioSystem;

        private void Start()
        {
            carController = GetComponent<CarControllerVipro>();
            
            if (GlobalAudio.Instance != null)
            {
                audioSystem = GlobalAudio.Instance;
                
                audioSystem.StartEngineSound();
            }
        }

        private void Update()
        {
            if (audioSystem == null || carController == null) return;

            HandleEngineSound();
            HandleNitroSound();
            HandleBrakeAndDriftSound();
        }

        private void HandleEngineSound()
        {
            if (audioSystem.engineSource == null) return;

            float speedRatio = carController.currentspeed / carController.maxSpeed;
            speedRatio = Mathf.Clamp01(speedRatio);

            audioSystem.engineSource.pitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);
        }

        private void HandleNitroSound()
        {
            bool isNitroKey = Input.GetKey(KeyCode.LeftShift) && carController.currentspeed > 1f;
            
            audioSystem.PlayNitroSound(isNitroKey);
        }

        private void HandleBrakeAndDriftSound()
        {
            bool isBraking = Input.GetKey(KeyCode.Space);
            
            bool isDrifting = CheckIsDrifting();

            if (isBraking || isDrifting)
            {
                audioSystem.StartBrakeLoop();
            }
            else
            {
                audioSystem.StopBrakeLoop();
            }
        }

        private bool CheckIsDrifting()
        {
            WheelHit hit;
            float slipThreshold = 0.4f; 

            if (carController.rearLeftCollider.GetGroundHit(out hit))
                if (Mathf.Abs(hit.sidewaysSlip) > slipThreshold) return true;

            if (carController.rearRightCollider.GetGroundHit(out hit))
                if (Mathf.Abs(hit.sidewaysSlip) > slipThreshold) return true;

            return false;
        }
        
        private void OnDisable()
        {
            if (audioSystem != null && audioSystem.engineSource != null)
            {
                audioSystem.engineSource.Stop();
                audioSystem.PlayNitroSound(false);
                audioSystem.StopBrakeLoop();
            }
        }
    }
}