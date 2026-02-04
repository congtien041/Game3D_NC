using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameCarLoader : MonoBehaviour
{
    // =========================================================
    // PHẦN 1: KHAI BÁO DỮ LIỆU (COPY Y HỆT BÊN SHOP)
    // =========================================================
    // Để không phải tạo file chung, ta khai báo lại ở đây để Unity hiểu
    
    [System.Serializable]
    public class ColorOption
    {
        public string colorName;      
        public string bundleName;     
        public string prefabName;     
    }

    [System.Serializable]
    public class CarProfile
    {
        public string carID;          
        public List<ColorOption> colorBundles = new List<ColorOption>(); 
    }

    // =========================================================
    // PHẦN 2: CẤU HÌNH
    // =========================================================

    [Header("Điểm Xuất Phát")]
    public Transform startPoint; // Kéo vị trí xe sẽ hiện ra vào đây

    [Header("Dữ liệu Xe (Phải nhập GIỐNG HỆT bên Shop)")]
    // Bạn phải điền danh sách này trong Inspector giống y chang bên Shop
    // Để code biết được: "Xe số 0, Màu số 1" ứng với Bundle tên là gì.
    public List<CarProfile> carDatabase = new List<CarProfile>();

    // =========================================================
    // PHẦN 3: LOGIC LOAD & SPAWN
    // =========================================================

    void Start()
    {
        SpawnSavedCar();
    }

    void SpawnSavedCar()
    {
        // 1. Đọc dữ liệu đã lưu từ Shop
        // Nếu không tìm thấy (chưa chơi bao giờ) thì mặc định lấy số 0
        int savedCarIndex = PlayerPrefs.GetInt("SavedCarID", 0);
        int savedColorIndex = PlayerPrefs.GetInt("SavedColorID", 0);

        Debug.Log($"Game đang load: Xe Index {savedCarIndex} - Màu Index {savedColorIndex}");

        // 2. Kiểm tra an toàn (Tránh lỗi nếu danh sách rỗng)
        if (carDatabase.Count == 0) return;

        // Đảm bảo index không vượt quá danh sách (Safety Check)
        if (savedCarIndex >= carDatabase.Count) savedCarIndex = 0;
        var carProfile = carDatabase[savedCarIndex];

        if (savedColorIndex >= carProfile.colorBundles.Count) savedColorIndex = 0;
        var colorOption = carProfile.colorBundles[savedColorIndex];

        // 3. Có tên bundle rồi -> Tiến hành Load
        StartCoroutine(ProcessSpawn(colorOption));
    }

    IEnumerator ProcessSpawn(ColorOption option)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Bundles", option.bundleName);

        // A. Load AssetBundle từ ổ cứng
        var request = AssetBundle.LoadFromFileAsync(path);
        yield return request;

        AssetBundle bundle = request.assetBundle;
        if (bundle == null)
        {
            Debug.LogError("Lỗi: Không tìm thấy Bundle game tại " + path);
            yield break;
        }

        // B. Load Prefab từ Bundle
        var assetRequest = bundle.LoadAssetAsync<GameObject>(option.prefabName);
        yield return assetRequest;

        GameObject prefab = assetRequest.asset as GameObject;

        if (prefab != null)
        {
            // C. Spawn xe ra tại vị trí Start Point
            GameObject playerCar = Instantiate(prefab, startPoint.position, startPoint.rotation);

            // D. Bật lại chức năng xe để chơi (QUAN TRỌNG)
            // Vì bên Shop ta tắt hết để ngắm, bên Game phải bật lại
            SetupCarForGameplay(playerCar);
        }
        else
        {
            Debug.LogError($"Không tìm thấy Prefab '{option.prefabName}' trong Bundle!");
        }

        // E. Giữ lại Texture/Model trong RAM để chơi, chỉ bỏ cái vỏ Bundle đi cho nhẹ
        // false = Giữ lại objects đã load
        bundle.Unload(false);
    }

    // Hàm bật vật lý và script lái xe
    void SetupCarForGameplay(GameObject car)
    {
        // 1. Bật Vật Lý (Rigidbody)
        Rigidbody rb = car.GetComponent<Rigidbody>();
        if (rb != null) 
        {
            rb.isKinematic = false; // Cho phép di chuyển
            rb.useGravity = true;   // Chịu trọng lực
        }

        // 2. Bật tất cả các Script điều khiển (CarController, Audio, v.v...)
        foreach (var script in car.GetComponentsInChildren<MonoBehaviour>())
        {
            script.enabled = true;
        }
        
        // 3. (Tùy chọn) Nếu bạn dùng Camera Follow, gán target tại đây
        // if (Camera.main.GetComponent<CameraFollow>()) 
        //     Camera.main.GetComponent<CameraFollow>().target = car.transform;

        Debug.Log("Xe đã sẵn sàng đua!");
    }
}