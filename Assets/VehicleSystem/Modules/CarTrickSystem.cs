using UnityEngine;
using System.Collections;
using VehicleSystem.Core;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;

namespace VehicleSystem.Modules
{
    [RequireComponent(typeof(CarControllerVipro))]
    public class CarTrickSystem : MonoBehaviour
    {
        [Header("--- THIẾT LẬP TRICK ---")]
        public float doubleTapWindow = 0.3f; // Thời gian giữa 2 lần bấm phím
        public float spinNitroReward = 40f;  // Thưởng bao nhiêu Nitro

        private CarControllerVipro carController;
        private CarNitroSystem nitroSystem;

        private float lastTapLeftTime = 0f;
        private float lastTapRightTime = 0f;

        private void Start()
        {
            carController = GetComponent<CarControllerVipro>();
            // Tìm Module Nitro (nếu có gắn trên xe)
            nitroSystem = GetComponent<CarNitroSystem>(); 
        }

        private void Update()
        {
            if (carController == null || carController.isCountdown || !carController.isEngineOn) return;
            if (carController.isSpinning) return; // Đang xoay thì không nhận nút nữa

            HandleDoubleTap();
        }

        private void HandleDoubleTap()
        {
            // Nhấp đúp Trái
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (Time.time - lastTapLeftTime < doubleTapWindow)
                    StartCoroutine(Do360Spin(-1)); 
                lastTapLeftTime = Time.time;
            }

            // Nhấp đúp Phải
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (Time.time - lastTapRightTime < doubleTapWindow)
                    StartCoroutine(Do360Spin(1)); 
                lastTapRightTime = Time.time;
            }
        }

        private IEnumerator Do360Spin(int direction)
        {
            // 1. Báo cho Core biết xe đang làm trò (Core sẽ ngừng chạy vật lý bánh xe)
            carController.isSpinning = true;

            // 2. Nhảy lên nhẹ
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce(Vector3.up * 5f, ForceMode.VelocityChange);

            // 3. Xoay 360 trên không (0.5 giây)
            float spinDuration = 0.5f; 
            float timeElapsed = 0f;

            while (timeElapsed < spinDuration)
            {
                float step = (360f / spinDuration) * Time.deltaTime * direction;
                transform.Rotate(0, step, 0, Space.Self);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // Cân bằng lại góc xoay cho thẳng
            Vector3 euler = transform.eulerAngles;
            transform.eulerAngles = new Vector3(euler.x, Mathf.Round(euler.y / 90) * 90, euler.z);

            // 4. Thưởng Nitro
            if (nitroSystem != null)
            {
                nitroSystem.AddNitro(spinNitroReward);
                Debug.Log($"Tricks! +{spinNitroReward} Nitro!");
            }

            // 5. Báo cho Core biết đã xong
            carController.isSpinning = false;
        }
    }
}