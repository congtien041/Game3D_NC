using System;
using UnityEngine;
using VehicleSystem.Core;

namespace VehicleSystem.Modules
{
    [RequireComponent(typeof(CarControllerVipro))]
    public class CarNitroSystem : MonoBehaviour
    {
        public static event Action<CarNitroSystem> OnPlayerNitroSpawned;
        
        [HideInInspector] public float maxNitro = 100f;              
        [HideInInspector] public float currentNitro = 0f;          
        private float nitroDrainRate = 30f;     
        private float passiveRegenRate = 2f;        
        private float driftRegenRate = 15f;         
        private float spinNitroBonus = 30f;    
        private float nitroThrustForce = 20f;             
        public bool IsUsingNitro { get; private set; } = false;
        public ParticleSystem Nitro;
        private float minEmissionRate = 20f;
        private float maxEmissionRate = 150f;
        private float speedForMaxEmission = 150f; 

        private CarControllerVipro carController;
        private Rigidbody rb;

        private void Start()
        {
            carController = GetComponent<CarControllerVipro>();
            OnPlayerNitroSpawned?.Invoke(this); 
            rb = GetComponent<Rigidbody>();

            if (Nitro != null) Nitro.Stop(); 
        }

        private void Update()
        {
            if (carController == null || carController.isCountdown || !carController.isEngineOn) 
            {
                if (Nitro != null && Nitro.isPlaying) Nitro.Stop();
                return;
            }

            HandleNitroLogic();
            HandleParticleEffects(); 
        }

        private void HandleNitroLogic()
        {
            bool isBraking = Input.GetKey(KeyCode.LeftControl);

            if (Input.GetKey(KeyCode.LeftShift) && !isBraking && currentNitro > 0)
            {
                IsUsingNitro = true;
                currentNitro -= nitroDrainRate * Time.deltaTime;
                if (currentNitro < 0) currentNitro = 0; 
            }
            else
            {
                IsUsingNitro = false;
            }

            if (!IsUsingNitro)
            {
                WheelHit hitL, hitR;
                carController.rearLeftCollider.GetGroundHit(out hitL);
                carController.rearRightCollider.GetGroundHit(out hitR);

                bool isDrifting = (Mathf.Abs(hitL.sidewaysSlip) > 0.3f || Mathf.Abs(hitR.sidewaysSlip) > 0.3f);
                float currentSpeed = rb.linearVelocity.magnitude * 3.6f;

                if (isDrifting)
                {
                    currentNitro += driftRegenRate * Time.deltaTime; 
                }
                else if (currentSpeed > 5f) 
                {
                    currentNitro += passiveRegenRate * Time.deltaTime; 
                }

                if (currentNitro > maxNitro) currentNitro = maxNitro;
            }

            if (IsUsingNitro)
            {
                carController.externalSpeedMultiplier = 1.5f; 
                carController.externalTorqueMultiplier = 3f;
                rb.AddForce(transform.forward * nitroThrustForce * Time.deltaTime, ForceMode.VelocityChange);
            }
            else
            {
                carController.externalSpeedMultiplier = 1f; 
                carController.externalTorqueMultiplier = 1f;
            }
        }

        private void HandleParticleEffects()
        {
            if (Nitro == null) return;

            if (IsUsingNitro)
            {
                if (!Nitro.isPlaying) Nitro.Play();
                float currentSpeedKmH = rb.linearVelocity.magnitude * 3.6f;
                float speedRatio = Mathf.Clamp01(currentSpeedKmH / speedForMaxEmission);
                var emission = Nitro.emission;
                emission.rateOverTime = Mathf.Lerp(minEmissionRate, maxEmissionRate, speedRatio);
            }
            else
            {
                if (Nitro.isPlaying) Nitro.Stop();
            }
        }

        public void AddSpinBonus()
        {
            currentNitro += spinNitroBonus;
            if (currentNitro > maxNitro) currentNitro = maxNitro;
            Debug.Log("<color=cyan>Xoay 1 vòng! Thưởng ngay " + spinNitroBonus + " Nitro!</color>");
        }
    }
}