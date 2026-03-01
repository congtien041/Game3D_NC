using UnityEngine;

namespace VehicleSystem.Data
{
    [System.Serializable]
    public class CarStatsData
    {
        [Header("UI Stats (Chỉ số hiển thị trên màn hình)")]
        public float uiTopSpeed;
        public float uiAcceleration;
        public float uiHandling;
        public float uiNitro;

        [Header("Physics Stats (Chỉ số thực tế cho WheelCollider)")]
        public float maxSpeed; 
        public float motorTorque;
        public float brakeForce;
        public float decelerationForce;
        public float maxSteeringAngle;
        
        [Header("Nitro Real Stats")]
        public float nitroCapacity; // Dung lượng bình nitro
        public float nitroMultiplier; // Tốc độ cộng thêm khi bơm nitro
        public CarStatsData Clone()
        {
            return (CarStatsData)this.MemberwiseClone();
        }
    }
}