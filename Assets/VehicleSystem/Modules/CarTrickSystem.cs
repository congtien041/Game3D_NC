using UnityEngine;
using System.Collections;
using VehicleSystem.Core;

namespace VehicleSystem.Modules
{
    [RequireComponent(typeof(CarControllerVipro))]
    public class CarTrickSystem : MonoBehaviour
    {
        [Header("--- THIẾT LẬP TRICK ---")]
        private float doubleTapWindow = 0.3f; 
        private CarControllerVipro carController;
        private CarNitroSystem nitroSystem;
        private Rigidbody rb; // Cache Rigidbody để dùng nhiều lần

        private float lastTapLeftTime = 0f;
        private float lastTapRightTime = 0f;

        private void Start()
        {
            carController = GetComponent<CarControllerVipro>();
            nitroSystem = GetComponent<CarNitroSystem>(); 
            rb = GetComponent<Rigidbody>(); // Lấy Rigidbody 1 lần ở Start
        }

        private void Update()
        {
            if (carController == null || carController.isCountdown || !carController.isEngineOn) return;
            if (carController.isSpinning) return; 

            HandleDoubleTap();
        }

        private void HandleDoubleTap()
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (Time.time - lastTapLeftTime < doubleTapWindow) StartCoroutine(Do360Spin(-1)); 
                lastTapLeftTime = Time.time;
            }

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (Time.time - lastTapRightTime < doubleTapWindow) StartCoroutine(Do360Spin(1)); 
                lastTapRightTime = Time.time;
            }
        }

        private IEnumerator Do360Spin(int direction)
        {
            carController.isSpinning = true;

            // 1. NHẢY LÊN CHÚT (Giảm ma sát với mặt đất)
            rb.AddForce(Vector3.up * 2f, ForceMode.VelocityChange);

            // 2. LƯU LẠI VẬN TỐC HIỆN TẠI (Quan trọng!)
            // Chỉ lấy vận tốc theo phương ngang (X, Z), bỏ qua trục Y
            Vector3 originalVelocity = rb.linearVelocity;
            Vector3 horizontalDirection = new Vector3(originalVelocity.x, 0, originalVelocity.z).normalized;
            float originalSpeed = new Vector3(originalVelocity.x, 0, originalVelocity.z).magnitude;

            // Nếu xe đang đứng yên thì cho nó chút lực đẩy tới để xoay cho đẹp
            if (originalSpeed < 5f) 
            {
                originalSpeed = 10f; 
                horizontalDirection = transform.forward;
            }

            float spinDuration = 0.5f; 
            float timeElapsed = 0f;

            while (timeElapsed < spinDuration)
            {
                float step = (360f / spinDuration) * Time.deltaTime * direction;
                transform.Rotate(0, step, 0, Space.Self);
                
                // 3. ÉP VẬN TỐC XE KHÔNG ĐƯỢC GIẢM (FIX LỖI MẤT TỐC)
                // Giữ nguyên vận tốc rơi tự do (Y), nhưng reset vận tốc ngang về mức ban đầu
                Vector3 currentVel = rb.linearVelocity;
                Vector3 newVelocity = horizontalDirection * originalSpeed; // Hướng cũ * Tốc độ cũ
                newVelocity.y = currentVel.y; // Giữ nguyên trọng lực đang rơi
                
                rb.linearVelocity = newVelocity;

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // Kết thúc xoay, làm tròn góc cho thẳng
            Vector3 euler = transform.eulerAngles;
            transform.eulerAngles = new Vector3(euler.x, Mathf.Round(euler.y / 90) * 90, euler.z);

            // Trả lại quyền điều khiển
            carController.isSpinning = false;
            
            // Đảm bảo vận tốc cuối cùng vẫn được bảo toàn
            Vector3 finalVel = horizontalDirection * originalSpeed;
            finalVel.y = rb.linearVelocity.y;
            rb.linearVelocity = finalVel;

            if (nitroSystem != null)
            {
                nitroSystem.AddSpinBonus();
            }
        }
    }
}