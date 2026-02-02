using System.Collections;
using System.IO;
using UnityEngine;

public class LoadAssetBundle : MonoBehaviour
{
    // Tên này PHẢI khớp chính xác với nhãn bạn đặt ở Bước 2
    public string bundleName = "car-paints"; 
    // Tên này PHẢI khớp với tên file Prefab trong Unity
    public string assetName = "Enemy"; 

    IEnumerator Start()
    {
        // 1. Kiểm tra đường dẫn
        string path = Path.Combine(Application.streamingAssetsPath, "Bundles", bundleName);
        
        if (!File.Exists(path)) {
            Debug.LogError("Không tìm thấy file Bundle tại: " + path);
            yield break;
        }

        // 2. Load Bundle
        AssetBundle bundle = AssetBundle.LoadFromFile(path);
        if (bundle == null) {
            Debug.LogError("Bundle bị Null khi LoadFromFile!");
            yield break;
        }

        // 3. Load Asset từ Bundle
        GameObject prefab = bundle.LoadAsset<GameObject>(assetName);
        if (prefab == null) {
            Debug.LogError("Asset '" + assetName + "' bị Null bên trong Bundle!");
            bundle.Unload(true);
            yield break;
        }

        // 4. Sinh ra xe và đổi màu
        GameObject car = Instantiate(prefab);
        ApplyColor(car, Color.red);

        // Giải phóng bộ nhớ (Unload false để giữ lại object đã sinh ra)
        bundle.Unload(false);
    }

    void ApplyColor(GameObject obj, Color color) {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) {
            r.material.color = color;
        }
    }
}