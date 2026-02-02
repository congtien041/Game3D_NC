using System.Collections;
using System.Collections.Generic; // Để dùng Queue
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; // Chỉ dùng thư viện này trong Editor
#endif

public class LoadAssetBundle : MonoBehaviour
{
    [Header("Cấu hình Chung")]
    public bool useEditorMode = true; // Tích vào để chỉnh sửa Real-time

    [Header("Đường dẫn file gốc (Dùng cho Editor Mode)")]
    // Bạn phải điền đường dẫn chính xác của file SO trong Project
    // Ví dụ: Assets/GameData/Monster/DragonData.asset
    public string editorDataPath = "Assets/Editor/player.asset";
    // public ScriptableObject editorDataPath; // Chứa tham chiếu file gốc

    [Header("Cấu hình Bundle (Dùng cho Build thật)")]
    public string modelBundleName = "player"; 
    public string assetName = "Capsule";      
    public string dataBundleName = "monster_data"; 
    public string dataFileName = "DragonData";     

    [Header("Cấu hình Spam Quái")]
    public float spawnInterval = 2.0f;
    public int maxMonsters = 5;       
    
    private GameObject _prefabTemplate;
    private MonsterData _loadedData;
    private int _currentCount = 0;
    
    // Object Pooling
    private Queue<GameObject> _monsterPool = new Queue<GameObject>();

    IEnumerator Start()
    {
        // --- BƯỚC 1: LOAD DỮ LIỆU (Hybrid) ---
        
        #if UNITY_EDITOR
        if (useEditorMode)
        {
            // CÁCH 1: LOAD TRỰC TIẾP TỪ DATABASE (Cập nhật tức thì)
            Debug.Log("Mode: Editor (Real-time update)");
            _loadedData = AssetDatabase.LoadAssetAtPath<MonsterData>(editorDataPath);
            
            if (_loadedData == null)
            {
                Debug.LogError($"Lỗi: Đường dẫn Editor sai! Kiểm tra lại: {editorDataPath}");
                yield break;
            }
        }
        else
        {
            // Nếu bỏ tích useEditorMode thì load bundle như thường để test
            yield return StartCoroutine(LoadDataFromBundle());
        }
        #else
            // CÁCH 2: KHI ĐÃ BUILD GAME THÌ BẮT BUỘC DÙNG BUNDLE
            yield return StartCoroutine(LoadDataFromBundle());
        #endif

        // --- BƯỚC 2: LOAD MODEL (Vẫn dùng Bundle hoặc load thường tùy bạn) ---
        // Ở đây tôi giữ nguyên load Bundle cho Model để test hình ảnh
        string pathPrefix = Application.streamingAssetsPath + "/Bundles/";
        var modelBundleReq = AssetBundle.LoadFromFileAsync(pathPrefix + modelBundleName);
        yield return modelBundleReq;

        var modelBundle = modelBundleReq.assetBundle;
        if (modelBundle == null) { Debug.LogError("Không thấy Bundle Model"); yield break; }

        _prefabTemplate = modelBundle.LoadAsset<GameObject>(assetName);
        modelBundle.Unload(false);

        // --- BƯỚC 3: START SPAM ---
        if (_loadedData != null && _prefabTemplate != null)
        {
            StartCoroutine(SpamMonsterRoutine());
        }
    }

    // Tách hàm load bundle ra riêng để gọn code
    IEnumerator LoadDataFromBundle()
    {
        string pathPrefix = Application.streamingAssetsPath + "/Bundles/";
        var dataBundleReq = AssetBundle.LoadFromFileAsync(pathPrefix + dataBundleName);
        yield return dataBundleReq;

        var dataBundle = dataBundleReq.assetBundle;
        if (dataBundle == null) { Debug.LogError("Không thấy Bundle Data"); yield break; }

        _loadedData = dataBundle.LoadAsset<MonsterData>(dataFileName);
        dataBundle.Unload(false);
    }

    IEnumerator SpamMonsterRoutine()
    {
        while (true)
        {
            if (_currentCount < maxMonsters)
            {
                SpawnOneMonster();
                _currentCount++;
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnOneMonster()
    {
        if (_prefabTemplate == null || _loadedData == null) return;

        GameObject minion = null;
        if (_monsterPool.Count > 0)
        {
            minion = _monsterPool.Dequeue();
            minion.SetActive(true);
        }
        else
        {
            minion = Instantiate(_prefabTemplate);
            var ctrl = minion.GetComponent<MonsterController>();
            if (ctrl == null) ctrl = minion.AddComponent<MonsterController>();
            
            // QUAN TRỌNG: Lúc này _loadedData là file gốc
            // Bạn thay đổi file gốc, con quái mới sinh ra sẽ nhận chỉ số mới ngay lập tức
            ctrl.SetupMonster(_loadedData);
        }
        
        // Setup vị trí ngẫu nhiên
        float x = Random.Range(-5f, 5f);
        float z = Random.Range(-5f, 5f);
        minion.transform.position = new Vector3(x, 0, z) + transform.position;
    }
}