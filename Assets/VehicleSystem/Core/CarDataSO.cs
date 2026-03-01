using UnityEngine;
using VehicleSystem.Data;

namespace VehicleSystem.Core
{
    [CreateAssetMenu(fileName = "NewCarData", menuName = "RacingGame/Car Data")]
    public class CarDataSO : ScriptableObject
    {
        public string carID;
        public string carName;
        public GameObject carPrefab;
        public Sprite carIcon;             
        public int carPrice;               
        public bool isUnlockedByDefault;   
        public float baseTopSpeed = 250.0f;
        public float baseAcceleration = 58.60f;
        public float baseHandling = 39.02f;
        public float baseNitro = 45.09f;
        public int maxUpgradeLevel = 5;    
        public float topSpeedPerLvl = 2.5f;
        public float accelerationPerLvl = 1.2f;
        public float handlingPerLvl = 0.8f;
        public float nitroPerLvl = 1.5f;
        public int[] upgradeCosts = new int[] { 100, 250, 500, 1000, 2000 };

        public CarStatsData CalculateFinalStats(CarUpgradeSave progress)
        {
            CarStatsData finalStats = new CarStatsData();

            finalStats.uiTopSpeed = baseTopSpeed + (progress.topSpeedLevel * topSpeedPerLvl);
            finalStats.uiAcceleration = baseAcceleration + (progress.accelerationLevel * accelerationPerLvl);
            finalStats.uiHandling = baseHandling + (progress.handlingLevel * handlingPerLvl);
            finalStats.uiNitro = baseNitro + (progress.nitroLevel * nitroPerLvl);
            finalStats.maxSpeed = finalStats.uiTopSpeed; 
            finalStats.motorTorque = finalStats.uiAcceleration * 15f; 
            finalStats.maxSteeringAngle = Mathf.Clamp(30f + (finalStats.uiHandling * 0.1f), 30f, 45f);
            finalStats.brakeForce = finalStats.uiHandling * 80f;
            finalStats.decelerationForce = finalStats.uiHandling * 10f;
            finalStats.nitroMultiplier = 1f + (finalStats.uiNitro * 0.01f); 
            finalStats.nitroCapacity = finalStats.uiNitro * 2f; 
            return finalStats;
        }

        public int GetUpgradeCost(int currentLevel)
        {
            if (currentLevel >= maxUpgradeLevel || currentLevel >= upgradeCosts.Length)
            {
                return -1; 
            }
            
            return upgradeCosts[currentLevel];
        }
    }
}