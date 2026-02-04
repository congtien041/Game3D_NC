using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Cần thêm thư viện này để tìm kiếm Bundle

public class GameCarLoader : MonoBehaviour
{
    // --- KHAI BÁO DỮ LIỆU ---
    [System.Serializable]
    public class ColorOption { public string colorName; public string bundleName; public string prefabName; }

    [System.Serializable]
    public class CarProfile { public string carID; public List<ColorOption> colorBundles = new List<ColorOption>(); }

    // --- CẤU HÌNH ---
    public Transform startPoint;
    public List<CarProfile> carDatabase = new List<CarProfile>();

    void Start()
    {
        SpawnSavedCar();
    }

    void SpawnSavedCar()
    {
        int savedCarIndex = PlayerPrefs.GetInt("SavedCarID", 0);
        int savedColorIndex = PlayerPrefs.GetInt("SavedColorID", 0);

        if (carDatabase.Count == 0) return;
        if (savedCarIndex >= carDatabase.Count) savedCarIndex = 0;
        
        var carProfile = carDatabase[savedCarIndex];
        if (savedColorIndex >= carProfile.colorBundles.Count) savedColorIndex = 0;
        
        var colorOption = carProfile.colorBundles[savedColorIndex];

        StartCoroutine(ProcessSpawn(colorOption));
    }

    IEnumerator ProcessSpawn(ColorOption option)
    {
        AssetBundle bundleToUse = null;
        string bundlePath = Path.Combine(Application.streamingAssetsPath, "Bundles", option.bundleName);

        // =======================================================
        // BƯỚC 1: KIỂM TRA XEM BUNDLE ĐÃ CÓ TRONG RAM CHƯA? (FIX LỖI CHẬP CHỜN)
        // =======================================================
        
        // Lấy tất cả bundle đang nằm trong bộ nhớ
        var loadedBundles = AssetBundle.GetAllLoadedAssetBundles();
        
        // Tìm xem có cái nào trùng tên với cái mình cần không
        foreach (var b in loadedBundles)
        {
            if (b.name == option.bundleName)
            {
                bundleToUse = b;
                Debug.Log("<color=yellow>Bundle này đã có sẵn trong RAM. Dùng lại luôn!</color>");
                break;
            }
        }

        // =======================================================
        // BƯỚC 2: NẾU CHƯA CÓ THÌ MỚI LOAD TỪ Ổ CỨNG
        // =======================================================
        if (bundleToUse == null)
        {
            var request = AssetBundle.LoadFromFileAsync(bundlePath);
            yield return request;

            if (request.assetBundle == null)
            {
                Debug.LogError("Lỗi: Không tìm thấy file Bundle tại " + bundlePath);
                yield break;
            }
            bundleToUse = request.assetBundle;
        }

        // =======================================================
        // BƯỚC 3: SPAWN PREFAB
        // =======================================================
        var assetRequest = bundleToUse.LoadAssetAsync<GameObject>(option.prefabName);
        yield return assetRequest;

        GameObject prefab = assetRequest.asset as GameObject;

        if (prefab != null)
        {
            GameObject playerCar = Instantiate(prefab, startPoint.position, startPoint.rotation);
            SetupCarForGameplay(playerCar);
        }
        else
        {
            Debug.LogError($"Có Bundle nhưng không tìm thấy Prefab '{option.prefabName}'");
        }

        // QUAN TRỌNG: Ở màn Game, ĐỪNG gọi Unload(true).
        // Nếu bạn Unload(true), lần sau load lại sẽ bị mất Texture.
        // Chỉ Unload(false) để giải phóng header thôi, hoặc giữ nguyên cũng được.
        // bundleToUse.Unload(false); 
    }

    void SetupCarForGameplay(GameObject car)
    {
        Rigidbody rb = car.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = false; rb.useGravity = true; }
        foreach (var script in car.GetComponentsInChildren<MonoBehaviour>()) script.enabled = true;
    }
    
    // Khi thoát màn chơi, ta mới dọn dẹp sạch sẽ
    private void OnDestroy()
    {
        // Tùy chọn: Có thể gọi AssetBundle.UnloadAllAssetBundles(false) nếu muốn dọn rác
    }
}