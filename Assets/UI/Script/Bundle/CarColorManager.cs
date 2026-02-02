using UnityEngine;
using System.Collections;
using System.IO;

public class CarShopController : MonoBehaviour
{
    [Header("Cấu hình Mặc định")]
    public string defaultBundleName = "cars_bundle"; // Bundle load khi vừa vào game
    public string defaultCarName = "Enemy";          // Xe hiện khi vừa vào game

    [Header("Cấu hình Tô màu")]
    public string paintMaterialKeyword = "Body"; 

    // Biến nội bộ
    private AssetBundle loadedBundle;
    private GameObject currentCarInstance;
    private string currentBundleName; // Lưu tên bundle đang dùng

    IEnumerator Start()
    {
        // Khi game bắt đầu, load bundle mặc định
        yield return LoadBundleProcess(defaultBundleName, defaultCarName);
    }

    // --- CHỨC NĂNG QUAN TRỌNG: ĐỔI BUNDLE (Logic Core) ---
    // Hàm này dùng để gọi quy trình hủy cũ -> nạp mới
    public void SwitchBundle(string newBundleName, string carToSpawn)
    {
        // Nếu đang dùng đúng bundle đó rồi thì không load lại, chỉ spawn lại xe
        if (currentBundleName == newBundleName && loadedBundle != null)
        {
            SpawnCar(carToSpawn);
            return;
        }

        // Bắt đầu Coroutine để tải bundle mới
        StartCoroutine(LoadBundleProcess(newBundleName, carToSpawn));
    }

    // Coroutine xử lý tải Bundle
    IEnumerator LoadBundleProcess(string newBundleName, string carToSpawn)
    {
        // 1. DỌN DẸP BUNDLE CŨ (Giải phóng RAM)
        if (loadedBundle != null)
        {
            // Xóa xe đang hiển thị trước
            if (currentCarInstance != null) Destroy(currentCarInstance);
            
            // Unload(true) để xóa sạch dữ liệu bundle cũ khỏi bộ nhớ
            loadedBundle.Unload(true);
            loadedBundle = null;
            
            // Đợi 1 frame cho Unity dọn rác
            yield return null; 
        }

        // 2. TẠO ĐƯỜNG DẪN
        string path = Path.Combine(Application.streamingAssetsPath, "Bundles", newBundleName);
        Debug.Log("Đang tải Bundle: " + newBundleName);

        // 3. LOAD BUNDLE MỚI
        var bundleLoadRequest = AssetBundle.LoadFromFileAsync(path);
        yield return bundleLoadRequest;

        loadedBundle = bundleLoadRequest.assetBundle;

        if (loadedBundle == null)
        {
            Debug.LogError("Lỗi: Không tìm thấy Bundle tại: " + path);
            currentBundleName = ""; 
            yield break;
        }

        // Cập nhật tên bundle hiện tại
        currentBundleName = newBundleName;

        // 4. Sinh chiếc xe đầu tiên của Bundle mới
        SpawnCar(carToSpawn);
    }

    // --- CÁC HÀM CẦU NỐI CHO UI (Gắn vào Button Unity) ---
    // Bạn hãy sửa tên string bên dưới cho khớp với tên file Bundle của bạn

    public void LoadBundleA()
    {
        // Ví dụ: Button 1 load gói xe Đua
        SwitchBundle("cars_bundle", "Enemy"); 
    }

    public void LoadBundleB()
    {
        // Ví dụ: Button 2 load gói xe Tải (Giả sử bạn có file tên 'trucks_bundle')
        SwitchBundle("yellow", "Enemy");
    }

    // --- CHỨC NĂNG: ĐỔI XE (Logic cũ) ---
    public void SpawnCar(string prefabName)
    {
        if (loadedBundle == null) return;

        GameObject sourcePrefab = loadedBundle.LoadAsset<GameObject>(prefabName);
        if (sourcePrefab == null)
        {
            Debug.LogError("Không tìm thấy xe: " + prefabName + " trong Bundle " + currentBundleName);
            return;
        }

        // Logic giữ xe cũ, reset transform (như code cũ của bạn)
        if (transform.childCount > 0)
        {
            GameObject currentChild = transform.GetChild(0).gameObject;
            string currentName = currentChild.name.Replace("(Clone)", "").Trim();

            if (currentName == prefabName)
            {
                currentChild.transform.localPosition = Vector3.zero;
                currentChild.transform.localRotation = Quaternion.identity;
                currentChild.transform.localScale = Vector3.one;
                ResetMaterialsToDefault(currentChild, sourcePrefab);
                currentCarInstance = currentChild;
                return;
            }
            Destroy(currentChild);
        }

        currentCarInstance = Instantiate(sourcePrefab, transform);
        currentCarInstance.transform.localPosition = Vector3.zero;
        currentCarInstance.transform.localRotation = Quaternion.identity;
    }

    // --- CÁC HÀM PHỤ TRỢ (Giữ nguyên) ---
    void ResetMaterialsToDefault(GameObject currentObj, GameObject sourcePrefab)
    {
        Renderer[] currentRenderers = currentObj.GetComponentsInChildren<Renderer>();
        Renderer[] sourceRenderers = sourcePrefab.GetComponentsInChildren<Renderer>();

        foreach (var curRend in currentRenderers)
        {
            foreach (var srcRend in sourceRenderers)
            {
                if (curRend.name == srcRend.name)
                {
                    curRend.sharedMaterials = srcRend.sharedMaterials;
                    break;
                }
            }
        }
    }

    public void SetColorRed() => ChangeColor(Color.red);
    public void SetColorBlue() => ChangeColor(Color.blue);
    public void SetColorGreen() => ChangeColor(Color.green);

    public void ChangeColor(Color newColor)
    {
        if (currentCarInstance == null) return;
        Renderer[] renderers = currentCarInstance.GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            foreach (var mat in rend.materials)
            {
                if (mat.name.Contains(paintMaterialKeyword))
                {
                    mat.color = newColor;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (loadedBundle != null) loadedBundle.Unload(true);
    }
}