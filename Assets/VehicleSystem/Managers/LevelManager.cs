using UnityEngine;
using VehicleSystem.Data;
using VehicleSystem.Core;

namespace VehicleSystem.Managers
{
    public class LevelManager : MonoBehaviour
    {
        [Header("--- SPAWN SETTINGS ---")]
        public Transform spawnPoint; // Kéo vị trí bạn muốn đẻ xe vào đây
        public CarDataSO[] allCars;  // Kéo danh sách xe vào đây để nó tìm

        [Header("--- PAINT SETTINGS ---")]
        public string paintMaterialName = "body_paint"; 
        public float emissionIntensity = 1.5f; // Cường độ sáng phải giống hệt lúc ở ngoài Menu

        private void Start()
        {
            SpawnPlayerCar();
        }

        private void SpawnPlayerCar()
        {
            // 1. Đọc ID chiếc xe mà người chơi đã chọn ngoài Menu 
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

            // 5. SƠN MÀU CHO XE (Dựa vào màu đã lưu)
            ApplySavedColor(carObj.transform);
        }

        // --- HÀM MỚI: TỰ ĐỘNG TÌM VÀ SƠN MÀU ---
        private void ApplySavedColor(Transform carRoot)
        {
            // Đọc lại giá trị dải màu đã lưu từ CarColorPicker (Mặc định là 0)
            float savedHue = PlayerPrefs.GetFloat("SavedCarColor", 0f);
            
            // Đổi từ Hue sang màu RGB thực tế
            Color savedColor = Color.HSVToRGB(savedHue, 1f, 1f);

            // Quét tìm các chi tiết vỏ xe và tô màu
            Renderer[] allRenderers = carRoot.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in allRenderers)
            {
                foreach (Material mat in rend.materials)
                {
                    if (mat.name.Contains(paintMaterialName))
                    {
                        mat.color = savedColor;
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", savedColor * emissionIntensity);
                    }
                }
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