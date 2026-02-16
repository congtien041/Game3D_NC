using UnityEngine;
using UnityEngine.AddressableAssets; // <--- BẮT BUỘC PHẢI CÓ DÒNG NÀY

namespace VehicleSystem.Data
{
    [CreateAssetMenu(fileName = "NewCarData", menuName = "Game Data/Car Data")]
    public class CarData : ScriptableObject
    {
        public string carName = "Tên Xe";
        public AssetReferenceGameObject carPrefab; 
        public float baseMotorTorque = 1500f;
        public float maxSteeringAngle = 30f;
        public float brakeForce = 3000f;
        public float baseMaxSpeed = 50f;      
        public float speedIncreasePerLevel = 10f; 
        public int maxLevel = 5;              
        public int baseUpgradeCost = 100;     
    }
}