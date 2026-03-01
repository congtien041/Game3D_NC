using UnityEngine;

namespace VehicleSystem.UI
{
    public class SpeedometerUI : MonoBehaviour
    {
        [Header("References")]
        public RectTransform needle;       // Bạn vẫn kéo cái kim vào đây nha

        [Header("Speed Settings (km/h)")]
        public float maxSpeed = 180f;      

        [Header("Angle Settings (Độ)")]
        public float minSpeedAngle = 135f; 
        public float maxSpeedAngle = -135f;

        // Đổi thành private, tự động gán bằng code nên không cần kéo thả nữa
        private Rigidbody carRigidbody;     

        private void Update()
        {
            // 1. Nếu chưa có xe, tự động đi tìm xe
            if (carRigidbody == null)
            {
                FindPlayerCar();
                
                // Nếu tìm xong mà vẫn chưa thấy (xe chưa kịp spawn), thì dừng Update frame này
                if (carRigidbody == null) return; 
            }

            if (needle == null) return;

            // 2. Tính toán tốc độ khi đã có xe
            float currentSpeed = carRigidbody.linearVelocity.magnitude * 3.6f;

            currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

            float speedNormalized = currentSpeed / maxSpeed;
            float currentAngle = Mathf.Lerp(minSpeedAngle, maxSpeedAngle, speedNormalized);

            needle.localEulerAngles = new Vector3(0, 0, currentAngle);
        }

        private void FindPlayerCar()
        {
            // Tìm đối tượng có gắn tag "Player" trên Scene
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            
            if (playerObj != null)
            {
                // Lấy Rigidbody của xe đó gán vào biến
                carRigidbody = playerObj.GetComponent<Rigidbody>();
            }
        }
    }
}