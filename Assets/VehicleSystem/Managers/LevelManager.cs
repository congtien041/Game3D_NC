using UnityEngine;
using VehicleSystem.Data;
using VehicleSystem.Core;

namespace VehicleSystem.Managers
{
    public class LevelManager : MonoBehaviour
    {
        public Transform spawnPoint; // Kéo vị trí bạn muốn đẻ xe vào đây
        public CarDataSO[] allCars;  // Kéo danh sách xe vào đây để nó tìm

        private void Start()
        {
            SpawnPlayerCar();
        }

        private void SpawnPlayerCar()
        {
            // 1. Đọc ID chiếc xe mà người chơi đã chọn ngoài Menu 
            // (Bạn nhớ thêm lệnh PlayerPrefs.SetString("SelectedCarID", carID) ở nút Play ngoài Menu nhé)
            string selectedCarID = PlayerPrefs.GetString("SelectedCarID", allCars[0].carID); 

            // 2. Tìm CarDataSO tương ứng với ID đó
            CarDataSO carDataToSpawn = null;
            foreach (var car in allCars)
            {
                if (car.carID == selectedCarID)
                {
                    carDataToSpawn = car;
                    break;
                }
            }

            if (carDataToSpawn == null) return;

            // 3. Spawn xe ra bản đồ
            GameObject carObj = Instantiate(carDataToSpawn.carPrefab, spawnPoint.position, spawnPoint.rotation);
            carObj.tag = "Player"; // Gắn tag để đồng hồ tốc độ và camera tìm được

            // 4. BƠM DỮ LIỆU CHỈ SỐ VÀO XE
            CarControllerVipro carController = carObj.GetComponent<CarControllerVipro>();
            if (carController != null)
            {
                // Load dữ liệu nâng cấp của xe này
                CarUpgradeSave save = LoadCarSave(carDataToSpawn.carID);
                
                // Tính toán chỉ số cuối cùng (Gốc + Nâng cấp)
                CarStatsData finalStats = carDataToSpawn.CalculateFinalStats(save);

                // Nạp vào xe và khởi động động cơ
                carController.InitializeStats(finalStats);
                carController.isEngineOn = true; // <--- Bật chìa khóa xe
            }
        }

        private CarUpgradeSave LoadCarSave(string carID)
        {
            string saveKey = "CarSave_" + carID;
            if (PlayerPrefs.HasKey(saveKey))
            {
                string json = PlayerPrefs.GetString(saveKey);
                return JsonUtility.FromJson<CarUpgradeSave>(json);
            }
            return new CarUpgradeSave { carID = carID };
        }
    }
}