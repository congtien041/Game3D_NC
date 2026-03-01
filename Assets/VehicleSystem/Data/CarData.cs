using UnityEngine;
using UnityEngine.AddressableAssets; // <--- BẮT BUỘC PHẢI CÓ DÒNG NÀY

namespace VehicleSystem.Data
{
    [CreateAssetMenu(fileName = "New Car", menuName = "Hyperdrive Racing/Car Data")]
    public class CarData : ScriptableObject
    {
        public string carID;
        public string carName; // Ví dụ: "BlackSportCar"
        public GameObject carPrefab; // Model 3D hiện trong garage
        public Sprite carIcon; // MỚI: Ảnh 2D vuông vuông hiển thị trên UI
        public int price;
        public bool isUnlockedByDefault;
        
        [Header("Car Stats (Max 500)")]
        public float topSpeed;     // Tương ứng thanh 1
        public float acceleration; // Tương ứng thanh 2
        public float handling;     // Tương ứng thanh 3
        public float nitro;        // MỚI: Tương ứng thanh 4
    }
}