using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class CarShopController : MonoBehaviour
{
    // --- 1. DATA STRUCTURE ---

    [System.Serializable]
    public class ColorOption
    {
        public string colorName;      // Tên màu (VD: Đỏ)
        public string bundleName;     // Tên Bundle chứa xe màu này
        public string prefabName;     // Tên Prefab trong Bundle
    }

    [System.Serializable]
    public class CarProfile
    {
        public string carID;          // Tên dòng xe (VD: Ferrari)
        // Danh sách các bundle màu của xe này
        public List<ColorOption> colorBundles = new List<ColorOption>(); 
    }

    // --- 2. CONFIGURATION ---
    [Header("Cấu hình Shop")]
    public Transform spawnPoint;      
    public List<CarProfile> carDatabase = new List<CarProfile>();

    // --- 3. BIẾN NỘI BỘ ---
    private AssetBundle currentLoadedBundle;
    private string currentBundleName = ""; 
    private GameObject currentCarInstance;

    // Chỉ số hiện tại
    public int currentCarIndex = 0; 
    private int currentColorIndex = 0;

    private Coroutine currentLoadingProcess;

    private void Start()
    {
        // Load xe đầu tiên, màu đầu tiên khi vào game
        if (carDatabase.Count > 0 && carDatabase[0].colorBundles.Count > 0)
        {
            LoadCarByBundle(0, 0);
        }
    }

    public void SelectCar(int carIndex)
    {
        // 1. Kiểm tra dữ liệu hợp lệ
        if (carDatabase.Count == 0) return;
        if (carIndex < 0 || carIndex >= carDatabase.Count)
        {
            Debug.LogWarning($"Không tìm thấy xe ở vị trí {carIndex}");
            return;
        }

        // 2. Nếu chọn lại đúng chiếc xe đang hiện thì không làm gì (Tối ưu)
        if (carIndex == currentCarIndex && currentCarInstance != null) return;

        // 3. Cập nhật Index xe mới
        currentCarIndex = carIndex;

        // 4. QUAN TRỌNG: Khi đổi xe, luôn Reset màu về màu đầu tiên (0)
        currentColorIndex = 0;

        // 5. Tiến hành load
        LoadCarByBundle(currentCarIndex, currentColorIndex);

        PlayerPrefs.SetInt("SavedCarID", currentCarIndex);
        PlayerPrefs.SetInt("SavedColorID", currentColorIndex);
        PlayerPrefs.Save();
    }
    public void SelectColor(int colorIndex)
    {
        if (carDatabase.Count == 0) return;

        var currentCar = carDatabase[currentCarIndex];

        if (colorIndex < 0 || colorIndex >= currentCar.colorBundles.Count)
        {
            Debug.LogWarning($"Xe {currentCar.carID} không có màu số {colorIndex}");
            return;
        }

        if (colorIndex == currentColorIndex && currentCarInstance != null) return;

        currentColorIndex = colorIndex;

        LoadCarByBundle(currentCarIndex, currentColorIndex);

        PlayerPrefs.SetInt("SavedCarID", currentCarIndex);
        PlayerPrefs.SetInt("SavedColorID", currentColorIndex);
        PlayerPrefs.Save();
    }


    private void LoadCarByBundle(int carIndex, int colorIndex)
    {
        var selectedCar = carDatabase[carIndex];
        
        if (selectedCar.colorBundles.Count == 0)
        {
            Debug.LogError($"Xe {selectedCar.carID} chưa được thiết lập Bundle màu nào!");
            return;
        }

        var selectedOption = selectedCar.colorBundles[colorIndex];

        if (currentLoadingProcess != null) StopCoroutine(currentLoadingProcess);
        
        currentLoadingProcess = StartCoroutine(ProcessLoadAndSpawn(selectedOption));
    }

    IEnumerator ProcessLoadAndSpawn(ColorOption option)
    {
        if (currentBundleName != option.bundleName)
        {
            // Unload cái cũ
            if (currentLoadedBundle != null)
            {
                currentLoadedBundle.Unload(true); 
                currentLoadedBundle = null;
                yield return null; 
            }

            // Load cái mới
            string path = Path.Combine(Application.streamingAssetsPath, "Bundles", option.bundleName);
            var request = AssetBundle.LoadFromFileAsync(path);
            yield return request;

            currentLoadedBundle = request.assetBundle;
            if (currentLoadedBundle == null)
            {
                Debug.LogError("Lỗi: Không tìm thấy Bundle tại " + path);
                currentBundleName = "";
                yield break;
            }
            currentBundleName = option.bundleName;
        }

        if (currentLoadedBundle != null)
        {
            if (currentCarInstance != null) Destroy(currentCarInstance);
            foreach (Transform child in spawnPoint) Destroy(child.gameObject);

            var assetRequest = currentLoadedBundle.LoadAssetAsync<GameObject>(option.prefabName);
            yield return assetRequest;

            GameObject prefab = assetRequest.asset as GameObject;
            if (prefab != null)
            {
                currentCarInstance = Instantiate(prefab, spawnPoint);
                SetupCarTransform(currentCarInstance);
                CleanupCarForShop(currentCarInstance);
            }
        }
    }

    // --- HÀM PHỤ TRỢ ---
    void SetupCarTransform(GameObject car)
    {
        car.transform.localPosition = Vector3.zero;
        car.transform.localRotation = Quaternion.identity;
        car.transform.localScale = Vector3.one;
    }

    void CleanupCarForShop(GameObject carObj)
    {
        Rigidbody rb = carObj.GetComponent<Rigidbody>();    
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        foreach (var script in carObj.GetComponentsInChildren<MonoBehaviour>())
        {
            script.enabled = false;
        }
    }
}