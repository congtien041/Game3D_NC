using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class CarShopController : MonoBehaviour
{
    // --- PHẦN ĐỊNH NGHĨA DỮ LIỆU ---

    [System.Serializable]
    public class CarColorInfo
    {
        public string colorName;      
        public string bundleName;     
        public string prefabName;     
    }

    [System.Serializable]
    public class CarProfile
    {
        public string carID;          
        public List<CarColorInfo> colors = new List<CarColorInfo>(); 
    }

    // --- PHẦN CẤU HÌNH & QUẢN LÝ ---

    [Header("Cấu hình Shop")]
    public Transform spawnPoint;      
    
    [Header("Dữ liệu Xe & Bundle")]
    public List<CarProfile> carDatabase = new List<CarProfile>();

    // --- BIẾN NỘI BỘ ---
    private AssetBundle currentLoadedBundle;
    private string currentBundleName = "";
    private GameObject currentCarInstance;
    private int currentCarIndex = 0; 

    void Awake()
    {
        LoadCar(1, 1);
    }
    private void Start()
    {
        if (carDatabase.Count > 0 && carDatabase[0].colors.Count > 0)
        {
            LoadCar(0, 0);
        }
    }

    // --- CÁC HÀM UI ---
    public void SelectCarType(int carIndex)
    {
        if (carIndex < 0 || carIndex >= carDatabase.Count) return;
        currentCarIndex = carIndex;
        LoadCar(currentCarIndex, 0); 
    }

    public void SelectColor(int colorIndex)
    {
        LoadCar(currentCarIndex, colorIndex);
    }

    // --- XỬ LÝ LOGIC ---

    public void LoadCar(int carIndex, int colorIndex)
    {
        if (carIndex >= carDatabase.Count) return;
        var selectedCar = carDatabase[carIndex];

        if (colorIndex >= selectedCar.colors.Count) return;
        var selectedColor = selectedCar.colors[colorIndex];

        StartCoroutine(ProcessLoadBundle(selectedColor));
    }

    IEnumerator ProcessLoadBundle(CarColorInfo info)
    {
        // 1. Load Bundle (Giữ nguyên logic cũ)
        if (currentBundleName != info.bundleName)
        {
            if (currentLoadedBundle != null)
            {
                currentLoadedBundle.Unload(true);
                currentLoadedBundle = null;
                currentCarInstance = null;
            }

            string path = Path.Combine(Application.streamingAssetsPath, "Bundles", info.bundleName);
            var request = AssetBundle.LoadFromFileAsync(path);
            yield return request;

            currentLoadedBundle = request.assetBundle;
            if (currentLoadedBundle == null)
            {
                Debug.LogError("Lỗi: Không tìm thấy Bundle tại " + path);
                currentBundleName = ""; 
                yield break;
            }
            currentBundleName = info.bundleName;
        }

        // 2. Spawn Xe và Xử lý lỗi
        if (currentLoadedBundle != null)
        {
            if (currentCarInstance != null) Destroy(currentCarInstance);
            foreach (Transform child in spawnPoint) Destroy(child.gameObject);

            GameObject prefab = currentLoadedBundle.LoadAsset<GameObject>(info.prefabName);
            if (prefab != null)
            {
                currentCarInstance = Instantiate(prefab, spawnPoint);
                
                // --- CẬP NHẬT MỚI: Reset vị trí chuẩn ---
                currentCarInstance.transform.localPosition = Vector3.zero; // Về đúng tâm 0,0,0
                currentCarInstance.transform.localRotation = Quaternion.identity;
                currentCarInstance.transform.localScale = Vector3.one;

                // --- CẬP NHẬT MỚI: Tắt Vật lý & Script ---
                CleanupCarForShop(currentCarInstance);
            }
            else
            {
                Debug.LogError($"Không tìm thấy Prefab '{info.prefabName}' trong Bundle");
            }
        }
    }

    // Hàm phụ trợ: Tắt hết chức năng thừa để xe chỉ đứng yên làm cảnh
    void CleanupCarForShop(GameObject carObj)
    {
        // 1. Tắt Vật Lý (Rigidbody) để xe không bị rơi tự do
        Rigidbody rb = carObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Đóng băng vật lý
            rb.useGravity = false; // Tắt trọng lực
        }

        // 2. Tắt các Script điều khiển (CarController, Audio, AI...)
        // Lấy tất cả các script MonoBehaviour có trên xe (bao gồm cả con của nó)
        MonoBehaviour[] scripts = carObj.GetComponentsInChildren<MonoBehaviour>();
        
        foreach (var script in scripts)
        {
            // Tránh tắt chính script này nếu lỡ gắn nhầm, hoặc các script hệ thống quan trọng (tùy chỉnh)
            // Ở đây ta tắt tất cả để an toàn nhất
            script.enabled = false;
        }

        // Lưu ý: Collider (va chạm) vẫn giữ nguyên để bạn có thể xoay xe bằng chuột nếu muốn sau này.
        // Nếu muốn tắt luôn va chạm thì dùng:
        // foreach(var col in carObj.GetComponentsInChildren<Collider>()) col.enabled = false;
    }
}