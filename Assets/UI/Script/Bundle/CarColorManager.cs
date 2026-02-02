using UnityEngine;
using System.Collections;
using System.IO;

public class CarShopController : MonoBehaviour
{
    [Header("Cấu hình Bundle")]
    public string bundleName = "cars_bundle"; // Tên file bundle đã build
    public string currentCarName = "Enemy";   // Tên mặc định khi vào game

    [Header("Cấu hình Tô màu")]
    public string paintMaterialKeyword = "Body"; // Chỉ tô màu Material nào có chữ này

    private AssetBundle loadedBundle;
    private GameObject currentCarInstance;
    private string bundlePath;

    IEnumerator Start()
    {
        // 1. Tạo đường dẫn
        bundlePath = Path.Combine(Application.streamingAssetsPath, "Bundles", bundleName);

        // 2. Load Bundle (Chỉ load 1 lần duy nhất)
        if (loadedBundle == null)
        {
            var bundleLoadRequest = AssetBundle.LoadFromFileAsync(bundlePath);
            yield return bundleLoadRequest;

            loadedBundle = bundleLoadRequest.assetBundle;

            if (loadedBundle == null)
            {
                Debug.LogError("Không tìm thấy Bundle tại: " + bundlePath);
                yield break;
            }
        }

        // 3. Sinh chiếc xe đầu tiên
        SpawnCar(currentCarName);
    }

    // --- CHỨC NĂNG 1: ĐỔI XE (Dùng cho UI Button) ---
    public void SpawnCar(string prefabName)
{
    if (loadedBundle == null) return;

    // 1. Luôn phải Load Prefab gốc từ Bundle ra trước để làm "mẫu chuẩn"
    GameObject sourcePrefab = loadedBundle.LoadAsset<GameObject>(prefabName);

    if (sourcePrefab == null)
    {
        Debug.LogError("Không tìm thấy xe: " + prefabName + " trong Bundle!");
        return;
    }

    // 2. Kiểm tra xem có xe nào đang hiển thị không
    if (transform.childCount > 0)
    {
        GameObject currentChild = transform.GetChild(0).gameObject;
        string currentName = currentChild.name.Replace("(Clone)", "").Trim();

        // 3. Nếu ĐÚNG xe mình cần
        if (currentName == prefabName)
        {
            Debug.Log("Xe đã có sẵn. Đang reset về trạng thái gốc của Bundle...");

            // Reset vị trí
            currentChild.transform.localPosition = Vector3.zero;
            currentChild.transform.localRotation = Quaternion.identity;
            currentChild.transform.localScale = Vector3.one;

            // --- BƯỚC QUAN TRỌNG: RESET MÀU/MATERIAL ---
            // Gọi hàm copy dữ liệu từ Prefab gốc đè lên xe hiện tại
            ResetMaterialsToDefault(currentChild, sourcePrefab);

            currentCarInstance = currentChild;
            return; // Xong việc, thoát hàm
        }
        
        // Nếu sai xe thì xóa đi
        Destroy(currentChild);
    }

    // 4. Nếu chưa có xe hoặc vừa xóa xe cũ -> Tạo mới
    currentCarInstance = Instantiate(sourcePrefab, transform);
    currentCarInstance.transform.localPosition = Vector3.zero;
    currentCarInstance.transform.localRotation = Quaternion.identity;
}

// Hàm này sẽ đi tìm từng bộ phận và trả lại màu gốc (Material gốc)
void ResetMaterialsToDefault(GameObject currentObj, GameObject sourcePrefab)
{
    // Lấy tất cả Renderer của xe hiện tại và xe gốc (Prefab)
    Renderer[] currentRenderers = currentObj.GetComponentsInChildren<Renderer>();
    Renderer[] sourceRenderers = sourcePrefab.GetComponentsInChildren<Renderer>();

    // Duyệt qua từng bộ phận của xe hiện tại
    foreach (var curRend in currentRenderers)
    {
        // Tìm bộ phận tương ứng bên xe gốc (so sánh theo tên)
        // Ví dụ: Tìm cái "Body" bên Prefab để lấy màu gốc
        foreach (var srcRend in sourceRenderers)
        {
            if (curRend.name == srcRend.name)
            {
                // Trả lại Material gốc (sharedMaterial)
                // Việc này sẽ xóa bay màu đỏ/xanh bạn đã tô, về lại màu mặc định
                curRend.sharedMaterials = srcRend.sharedMaterials;
                break;
            }
        }
    }
}

    // --- CHỨC NĂNG 2: ĐỔI MÀU (Dùng cho UI Button) ---
    // Hàm trung gian để Button gọi được (vì Button không truyền được Color trực tiếp dễ dàng)
    public void SetColorRed() => ChangeColor(Color.red);
    public void SetColorBlue() => ChangeColor(Color.blue);
    public void SetColorGreen() => ChangeColor(Color.green);

    public void ChangeColor(Color newColor)
    {
        if (currentCarInstance == null) return;

        // Lấy tất cả Renderer trong xe (kể cả con cháu)
        Renderer[] renderers = currentCarInstance.GetComponentsInChildren<Renderer>();

        foreach (var rend in renderers)
        {
            // Duyệt qua từng Material của từng bộ phận
            foreach (var mat in rend.materials)
            {
                // MẤU CHỐT: Chỉ đổi màu nếu tên Material chứa từ khóa (VD: "Body")
                if (mat.name.Contains(paintMaterialKeyword))
                {
                    mat.color = newColor;
                    // Nếu dùng URP/HDRP có thể cần: mat.SetColor("_BaseColor", newColor);
                }
            }
        }
    }

    // Giải phóng bộ nhớ khi tắt Shop hoặc chuyển Scene
    void OnDestroy()
    {
        if (loadedBundle != null)
        {
            loadedBundle.Unload(true);
        }
    }
}