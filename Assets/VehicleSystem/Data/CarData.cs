using UnityEngine;

// Bọc toàn bộ code vào bên trong namespace
namespace VehicleSystem.Data
{
    [CreateAssetMenu(fileName = "NewCarData", menuName = "Game Data/Car Data")]
    public class CarData : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
        public string carName = "Tên Xe";
        public GameObject carPrefab; 

        [Header("Chỉ số Vật lý cốt lõi")]
        public float baseMotorTorque = 1500f;
        public float maxSteeringAngle = 30f;
        public float brakeForce = 3000f;

        [Header("Hệ thống Nâng cấp")]
        public float baseMaxSpeed = 50f;      
        public float speedIncreasePerLevel = 10f; 
        public int maxLevel = 5;              
        public int baseUpgradeCost = 100;     
    }
}