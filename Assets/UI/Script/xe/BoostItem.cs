using UnityEngine;

public class BoostItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem thứ chạm vào có script VehicleBoost không
        VehicleBoost vehicle = other.GetComponent<VehicleBoost>();

        if (vehicle != null)
        {
            vehicle.ActivateBoost(); // Kích hoạt hiệu ứng trên xe đó
            
            // Tùy chọn: Xóa cục boost hoặc cho nó ẩn đi để hồi lại sau
            // Destroy(gameObject); 
        }
    }
}