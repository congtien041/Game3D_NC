using UnityEngine;
using System.Collections;
using VehicleSystem.Core;

namespace VehicleSystem.Modules
{
    [RequireComponent(typeof(CarControllerVipro))]
    public class CarTrickSystem : MonoBehaviour
    {
        private float doubleTapWindow = 0.3f; 
        private float spinNitroReward = 40f;  

        private CarControllerVipro carController;
        private CarNitroSystem nitroSystem;

        private float lastTapLeftTime = 0f;
        private float lastTapRightTime = 0f;

        private void Start()
        {
            carController = GetComponent<CarControllerVipro>();
            nitroSystem = GetComponent<CarNitroSystem>(); 
        }

        private void Update()
        {
            if (carController == null || carController.isCountdown || !carController.isEngineOn) return;
            if (carController.isSpinning) return; 
            HandleDoubleTap();
        }

        private void HandleDoubleTap()
        {
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (Time.time - lastTapLeftTime < doubleTapWindow) StartCoroutine(Do360Spin(-1)); 
                lastTapLeftTime = Time.time;
            }

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (Time.time - lastTapRightTime < doubleTapWindow) StartCoroutine(Do360Spin(1)); 
                lastTapRightTime = Time.time;
            }
        }

        private IEnumerator Do360Spin(int direction)
        {
            carController.isSpinning = true;
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce(Vector3.up * 5f, ForceMode.VelocityChange);
            float spinDuration = 0.5f; 
            float timeElapsed = 0f;
            float nitroPerSecond = spinNitroReward / spinDuration;
            while (timeElapsed < spinDuration)
            {
                float step = (360f / spinDuration) * Time.deltaTime * direction;
                transform.Rotate(0, step, 0, Space.Self);
                
                if (nitroSystem != null)
                {
                    nitroSystem.AddNitro(nitroPerSecond * Time.deltaTime);
                }

                timeElapsed += Time.deltaTime;
                yield return null;
            }
            Vector3 euler = transform.eulerAngles;
            transform.eulerAngles = new Vector3(euler.x, Mathf.Round(euler.y / 90) * 90, euler.z);
            carController.isSpinning = false;
        }
    }
}