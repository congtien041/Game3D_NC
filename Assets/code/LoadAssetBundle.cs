using System.Collections;
using UnityEngine;

public class LoadAssetBundle : MonoBehaviour
{
    [Header("Cấu hình Bundle")]
    public string dataBundleName = "1";   // Bundle chứa file SO
    public string modelBundleName = "player"; // Bundle chứa Prefab 3D
    
    [Header("File cần load")]
    public string targetDataFileName = "DragonData"; // Tên file MonsterData bạn đã tạo

    IEnumerator Start()
    {
        string pathPrefix = Application.streamingAssetsPath + "/Bundles/";

        // --- BƯỚC 1: Load Data (Nhẹ) ---
        // Load bundle chứa dữ liệu
        AssetBundle dataBundle = AssetBundle.LoadFromFile(pathPrefix + dataBundleName);
        if (dataBundle == null)
        {
            Debug.LogError("Không tìm thấy Data Bundle!");
            yield break;
        }

        // Load file SO từ bundle
        MonsterData data = dataBundle.LoadAsset<MonsterData>(targetDataFileName);
        
        if (data != null)
        {
            Debug.Log($"Đã load dữ liệu: {data.monsterName} | HP: {data.hp}");
            
            // --- BƯỚC 2: Load Model (Nặng - Chỉ load khi cần) ---
            AssetBundle modelBundle = AssetBundle.LoadFromFile(pathPrefix + modelBundleName);
            if (modelBundle != null)
            {
                // Dùng tên prefab được lưu trong Data để load model tương ứng
                GameObject prefab = modelBundle.LoadAsset<GameObject>(data.prefabName);
                if (prefab != null)
                {
                    Instantiate(prefab, transform.position, Quaternion.identity);
                }
                else
                {
                    Debug.LogError($"Không tìm thấy Prefab tên: {data.prefabName} trong bundle models");
                }
                
                // Giải phóng bundle Model ngay sau khi lấy được prefab để tiết kiệm RAM
                modelBundle.Unload(false);
            }
        }
        else
        {
            Debug.LogError($"Không tìm thấy file Data tên: {targetDataFileName}");
        }

        // Giải phóng bundle Data (giữ lại object data đã load)
        dataBundle.Unload(false);
    }
}