using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LoadAssetBundle : MonoBehaviour
{
    [Header("Cấu hình Chung")]
    public bool useEditorMode = true; // Tích vào để sửa Real-time
    public string editorDataPath = "Assets/GameData/Monster/DragonData.asset";

    [Header("Cấu hình Bundle")]
    public string modelBundleName = "player"; 
    public string assetName = "Capsule";      
    public string dataBundleName = "monster_data"; 
    public string dataFileName = "DragonData";     

    [Header("Spam Quái")]
    public float spawnInterval = 2.0f;
    public int maxMonsters = 5;       
    
    // ĐỂ PUBLIC CHO UI TRUY CẬP
    public MonsterData _loadedData; 

    private GameObject _prefabTemplate;
    private int _currentCount = 0;
    
    // Hồ chứa quái (Object Pooling)
    private Queue<GameObject> _monsterPool = new Queue<GameObject>();

    IEnumerator Start()
    {
        // --- 1. LOAD DATA ---
        #if UNITY_EDITOR
        if (useEditorMode)
        {
            _loadedData = AssetDatabase.LoadAssetAtPath<MonsterData>(editorDataPath);
            if (_loadedData == null) Debug.LogError("Sai đường dẫn Editor Data Path!");
        }
        else
        {
            yield return StartCoroutine(LoadDataFromBundle());
        }
        #else
            yield return StartCoroutine(LoadDataFromBundle());
        #endif

        // --- 2. LOAD MODEL ---
        string pathPrefix = Application.streamingAssetsPath + "/Bundles/";
        var modelBundleReq = AssetBundle.LoadFromFileAsync(pathPrefix + modelBundleName);
        yield return modelBundleReq;

        var modelBundle = modelBundleReq.assetBundle;
        if (modelBundle != null)
        {
            _prefabTemplate = modelBundle.LoadAsset<GameObject>(assetName);
            modelBundle.Unload(false);
        }

        // --- 3. START SPAM ---
        if (_loadedData != null && _prefabTemplate != null)
        {
            StartCoroutine(SpamMonsterRoutine());
        }
    }

    IEnumerator LoadDataFromBundle()
    {
        string pathPrefix = Application.streamingAssetsPath + "/Bundles/";
        var dataBundleReq = AssetBundle.LoadFromFileAsync(pathPrefix + dataBundleName);
        yield return dataBundleReq;

        if (dataBundleReq.assetBundle != null)
        {
            _loadedData = dataBundleReq.assetBundle.LoadAsset<MonsterData>(dataFileName);
            dataBundleReq.assetBundle.Unload(false);
        }
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

    // --- HÀM QUAN TRỌNG NHẤT ĐÃ ĐƯỢC SỬA ---
    void SpawnOneMonster()
    {
        if (_prefabTemplate == null || _loadedData == null) return;

        GameObject minion = null;
        MonsterController ctrl = null;

        // BƯỚC A: TÌM XÁC (Mới hoặc Cũ)
        if (_monsterPool.Count > 0)
        {
            minion = _monsterPool.Dequeue();
            minion.SetActive(true); // Bật lại quái cũ
            ctrl = minion.GetComponent<MonsterController>();
        }
        else
        {
            minion = Instantiate(_prefabTemplate);
            ctrl = minion.GetComponent<MonsterController>();
            if (ctrl == null) ctrl = minion.AddComponent<MonsterController>();
        }

        // BƯỚC B: NẠP DỮ LIỆU (BẮT BUỘC CHẠY CHO CẢ 2 TRƯỜNG HỢP)
        if (ctrl != null)
        {
            // Dòng này sẽ lấy dữ liệu MỚI NHẤT từ UI và đẩy vào con quái
            ctrl.SetupMonster(_loadedData);
        }

        // BƯỚC C: ĐẶT VỊ TRÍ
        float x = Random.Range(-5f, 5f);
        float z = Random.Range(-5f, 5f);
        minion.transform.position = new Vector3(x, 0, z) + transform.position;
        minion.transform.rotation = Quaternion.identity;
    }
}