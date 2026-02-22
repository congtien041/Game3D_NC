using UnityEngine;
using UnityEngine.UI; 
using VehicleSystem.Modules; // Gọi tới namespace chứa CarNitroSystem

namespace VehicleSystem.UI
{
    public class NitroUIManager : MonoBehaviour
    {
        [Header("--- THÀNH PHẦN UI ---")]
        [Tooltip("Kéo Image dùng làm thanh fill Nitro vào đây (Image Type phải là Filled)")]
        public Image nitroFillImage; 

        // Lưu trữ kết nối với bình Nitro của người chơi
        private CarNitroSystem playerNitroSystem;

        // --- ĐĂNG KÝ SỰ KIỆN (TỐI ƯU HIỆU NĂNG TỐI ĐA) ---
        private void OnEnable()
        {
            // Lắng nghe tiếng "hét" của xe khi nó vừa spawn ra
            CarNitroSystem.OnPlayerNitroSpawned += ConnectToPlayer;
        }

        private void OnDisable()
        {
            // Bỏ lắng nghe khi UI tắt đi (Chống rò rỉ bộ nhớ)
            CarNitroSystem.OnPlayerNitroSpawned -= ConnectToPlayer;
        }

        // Hàm này sẽ tự động chạy khi xe xuất hiện trên Map
        private void ConnectToPlayer(CarNitroSystem nitroSystem)
        {
            playerNitroSystem = nitroSystem;
            Debug.Log("<color=green>UI: Đã kết nối thành công với bình Nitro của xe!</color>");
        }

        private void Update()
        {
            // Cực kỳ nhẹ CPU: Không cần Find GameObject hay GetComponent liên tục nữa
            if (playerNitroSystem == null || nitroFillImage == null) return; 

            UpdateNitroUI();
        }

        private void UpdateNitroUI()
        {
            // 1. Tính toán tỷ lệ % của bình Nitro (từ 0.0 đến 1.0)
            float fillRatio = playerNitroSystem.currentNitro / playerNitroSystem.maxNitro;

            // 2. Đổ đầy thanh Image dựa trên tỷ lệ
            nitroFillImage.fillAmount = fillRatio;

            // 3. Hiệu ứng đổi màu thanh Nitro cho sinh động
            if (playerNitroSystem.IsUsingNitro)
            {
                // Khi đang bứt tốc (Burst) -> Màu Vàng Cam rực cháy
                nitroFillImage.color = new Color(1f, 0.6f, 0f); 
            }
            else if (fillRatio >= 1f)
            {
                // Khi bình sạc đầy 100% -> Màu Xanh Cyan nhấp nháy
                nitroFillImage.color = Color.cyan; 
            }
            else
            {
                // Đang trong quá trình sạc (Regen) -> Màu Xanh dương đậm
                nitroFillImage.color = new Color(0f, 0.5f, 1f); 
            }
        }
    }
}