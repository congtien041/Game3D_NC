using System.Collections;
using UnityEngine;

public class LoadAssetBundle : MonoBehaviour
{
    [Header("Cấu hình Asset Bundle")]
    public string bundleName = "player"; // Tên bundle (folder)
    public string assetName = "player"; // Tên file Prefab trong bundle

    [Header("Cấu hình Spam Quái")]
    public float spawnInterval = 2.0f; // Bao lâu đẻ 1 con (giây)
    public int maxMonsters = 5;       // Giới hạn số lượng (để tránh lag máy)
    
    // Biến lưu trữ mẫu prefab để dùng lại nhiều lần
    private GameObject _prefabTemplate;
    private int _currentCount = 0;

    IEnumerator Start()
    {
        // --- BƯỚC 1: LOAD TÀI NGUYÊN (CHỈ LÀM 1 LẦN) ---
        string path = Application.streamingAssetsPath + "/Bundles/" + bundleName;
        
        // Load Bundle từ ổ cứng
        var bundleRequest = AssetBundle.LoadFromFileAsync(path);
        yield return bundleRequest;

        AssetBundle bundle = bundleRequest.assetBundle;
        if (bundle == null)
        {
            Debug.LogError("Lỗi: Không tìm thấy Bundle tại đường dẫn: " + path);
            yield break;
        }

        // Load Prefab từ Bundle
        var assetRequest = bundle.LoadAssetAsync<GameObject>(assetName);
        yield return assetRequest;

        _prefabTemplate = assetRequest.asset as GameObject;

        // Giải phóng bộ nhớ Bundle ngay (Giữ lại prefabTemplate để dùng)
        // false = Giữ lại các object đã load, chỉ xóa vỏ bundle
        bundle.Unload(false); 

        if (_prefabTemplate == null)
        {
            Debug.LogError($"Lỗi: Không tìm thấy Prefab tên '{assetName}' trong Bundle.");
            yield break;
        }

        // --- BƯỚC 2: BẮT ĐẦU SPAM QUÁI ---
        Debug.Log("Load xong! Bắt đầu thả quái...");
        StartCoroutine(SpamMonsterRoutine());
    }

    // Coroutine riêng để xử lý việc sinh quái
    IEnumerator SpamMonsterRoutine()
    {
        while (true) // Lặp vô tận
        {
            if (_currentCount < maxMonsters)
            {
                SpawnOneMonster();
                _currentCount++;
            }
            
            // Đợi X giây rồi lặp tiếp
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnOneMonster()
    {
        // Tạo vị trí ngẫu nhiên xung quanh object này
        float x = Random.Range(-5f, 5f);
        float z = Random.Range(-5f, 5f);
        Vector3 spawnPos = new Vector3(x, 0, z) + transform.position;

        // Sinh ra quái từ mẫu đã load
        Instantiate(_prefabTemplate, spawnPos, Quaternion.identity);
    }
}