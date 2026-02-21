using UnityEngine;
using VehicleSystem.Core;

namespace VehicleSystem.Modules
{
    [RequireComponent(typeof(Rigidbody))]
    public class AutoHeadLift : MonoBehaviour
    {
        [Header("Kết nối")]
        public CarControllerVipro carController;

        [Header("Cấu hình Nhún")]
        [Tooltip("Lực nhấc đầu xe. Chỉnh thật to (10.000 - 50.000)")]
        public float liftForce = 20000f; 
        
        [Tooltip("Tốc độ nhịp (3 = Vroom... Vroom..., 5 = VromVromVrom)")]
        public float rhythmSpeed = 3.0f;

        private Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (carController == null) carController = GetComponent<CarControllerVipro>();
        }

        private void FixedUpdate()
        {
            // Chỉ chạy khi đang đếm ngược
            if (carController != null && carController.isCountdown)
            {
                LiftTheHead();
            }
        }

        void LiftTheHead()
        {
            // 1. TẠO NHỊP ĐIỆU (Từ 0 đến 1 rồi về 0)
            // Dùng hàm Sin để tạo nhịp thở đều đặn
            float rhythm = Mathf.Abs(Mathf.Sin(Time.time * rhythmSpeed));

            // Mẹo: Bình phương lên để nhịp nó "gắt" hơn (lên nhanh xuống chậm)
            // rhythm = rhythm * rhythm; 

            // 2. KHÓA CỨNG BÁNH XE (Để xe không bị trôi khi nhấc đầu)
            // (Giả sử bạn có hàm ApplyBrake public bên CarController, hoặc gọi trực tiếp collider)
            if(carController.rearLeftCollider) carController.rearLeftCollider.brakeTorque = 10000f;
            if(carController.rearRightCollider) carController.rearRightCollider.brakeTorque = 10000f;
            // Khóa bánh sau thôi, bánh trước để tự do cho nó nảy

            // 3. TẠO LỰC XOAY TRỤC X (Pitch)
            // Vector3.right là trục ngang. Xoay quanh trục này sẽ làm đầu xe ngóc lên.
            // Lực này tác động thẳng vào khung xe (Rigidbody)
            // Dùng -Vector3.right hay Vector3.right tùy vào hướng xe của bạn. 
            // Hãy thử dấu (-) trước, nếu thấy đuôi xe nhấc lên thì đổi thành dấu (+)
            rb.AddRelativeTorque(Vector3.right * liftForce * rhythm);
        }
    }
}