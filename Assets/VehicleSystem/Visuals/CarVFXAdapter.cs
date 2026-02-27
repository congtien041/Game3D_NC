using UnityEngine;
using VehicleSystem.Core;

namespace VehicleSystem.Visuals
{
    [RequireComponent(typeof(CarControllerVipro))]
    public class CarVFXAdapter : MonoBehaviour
    {
        private float slipThreshold = 0.4f;
        private float minSpeedForVFX = 10f;
        [Header("PARTICLE SYSTEMS")]
        public ParticleSystem rearLeftSmoke;
        public ParticleSystem rearRightSmoke;

        [Header("TRAIL RENDERERS")]
        public TrailRenderer rearLeftSkid;
        public TrailRenderer rearRightSkid;
        private CarControllerVipro carController;

        private void Start()
        {
            carController = GetComponent<CarControllerVipro>();
            StopEffects();
        }

        private void Update()
        {
            if (carController == null) return;

            HandleWheelVFX(carController.rearLeftCollider, rearLeftSmoke, rearLeftSkid);
            HandleWheelVFX(carController.rearRightCollider, rearRightSmoke, rearRightSkid);
        }

        private void HandleWheelVFX(WheelCollider wheel, ParticleSystem smoke, TrailRenderer skid)
        {
            WheelHit hit;
            bool isSlipping = false;
            if (wheel.GetGroundHit(out hit))
            {
                Mathf.Abs(hit.sidewaysSlip);
                Mathf.Abs(hit.forwardSlip);
                float currentSlip = Mathf.Abs(hit.sidewaysSlip) + Mathf.Abs(hit.forwardSlip);
                bool isBraking = Input.GetKey(KeyCode.Space);
                bool fastEnough = carController.currentspeed > minSpeedForVFX;

                if ((currentSlip > slipThreshold || isBraking) && fastEnough)
                {
                    isSlipping = true;
                }
            }
            if (smoke != null)
            {
                if (isSlipping)
                {
                    if (!smoke.isPlaying) smoke.Play();
                }
                else
                {
                    if (smoke.isPlaying) smoke.Stop();
                }
            }

            if (skid != null)
            {
                skid.emitting = isSlipping && wheel.isGrounded;
            }
        }

        private void StopEffects()
        {
            if (rearLeftSmoke) rearLeftSmoke.Stop();
            if (rearRightSmoke) rearRightSmoke.Stop();
            if (rearLeftSkid) rearLeftSkid.emitting = false;
            if (rearRightSkid) rearRightSkid.emitting = false;
        }
    }
}