using UnityEngine;
using TMPro;
using VehicleSystem.Managers; 

namespace VehicleSystem.UI
{
    public class MoneyUIUpdater : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI moneyText;

        private void Start()
        {
            // Dùng Start thay vì OnEnable để tránh lỗi chạy trước MoneyManager
            if (MoneyManager.Instance != null)
            {
                // Đăng ký sự kiện: mỗi khi tiền thay đổi, tự động gọi UpdateMoneyDisplay
                MoneyManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
                
                // Hiển thị số tiền ngay lúc game vừa bật
                UpdateMoneyDisplay(MoneyManager.Instance.GetBalance());
            }
            else
            {
                Debug.LogError("Không tìm thấy MoneyManager trong Scene! Hãy chắc chắn bạn đã kéo prefab MoneyManager vào Hierarchy.");
            }
        }

        private void OnDestroy()
        {
            // Thay OnDisable bằng OnDestroy để dọn dẹp bộ nhớ khi tắt scene
            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
            }
        }

        private void UpdateMoneyDisplay(int currentMoney)
        {
            if (moneyText != null)
            {
                moneyText.text = currentMoney.ToString("N0"); 
            }
        }

        public void DEBUG_AddMoney(int amount)
        {
            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.AddMoney(amount);
            }
        }
    }
}