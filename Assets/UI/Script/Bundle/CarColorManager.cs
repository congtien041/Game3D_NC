using UnityEngine;
using System.Collections;
using System.IO;

public class CarShopController : MonoBehaviour
{
    [Header("Cấu hình Mặc định")]
    public string defaultBundleName = "cars_bundle"; 
    public string defaultCarName = "Enemy";       

    [Header("Cấu hình Tô màu")]
    public string paintMaterialKeyword = "Body"; 

    private AssetBundle loadedBundle;
    private GameObject currentCarInstance;
    private string currentBundleName; 
    IEnumerator Start()
    {
        yield return LoadBundleProcess(defaultBundleName, defaultCarName);
    }

    public void SwitchBundle(string newBundleName, string carToSpawn)
    {
        // Nếu đang dùng đúng bundle đó rồi thì không load lại, chỉ spawn lại xe
        if (currentBundleName == newBundleName && loadedBundle != null)
        {
            SpawnCar(carToSpawn);
            return;
        }

        StartCoroutine(LoadBundleProcess(newBundleName, carToSpawn));
    }

    IEnumerator LoadBundleProcess(string newBundleName, string carToSpawn)
    {
        if (loadedBundle != null)
        {
            if (currentCarInstance != null) Destroy(currentCarInstance);
            
            loadedBundle.Unload(true);
            loadedBundle = null;
            
            yield return null; 
        }

        string path = Path.Combine(Application.streamingAssetsPath, "Bundles", newBundleName);
        Debug.Log("Đang tải Bundle: " + newBundleName);

        var bundleLoadRequest = AssetBundle.LoadFromFileAsync(path);
        yield return bundleLoadRequest;

        loadedBundle = bundleLoadRequest.assetBundle;

        if (loadedBundle == null)
        {
            Debug.LogError("Lỗi: Không tìm thấy Bundle tại: " + path);
            currentBundleName = ""; 
            yield break;
        }

        currentBundleName = newBundleName;

        SpawnCar(carToSpawn);
    }


    public void LoadBundleA()
    {
        SwitchBundle("cars_bundle", "Enemy"); 
    }

    public void LoadBundleB()
    {
        SwitchBundle("yellow", "Enemy");
    }

    public void SpawnCar(string prefabName)
    {
        if (loadedBundle == null) return;

        GameObject sourcePrefab = loadedBundle.LoadAsset<GameObject>(prefabName);
        if (sourcePrefab == null)
        {
            Debug.LogError("Không tìm thấy xe: " + prefabName + " trong Bundle " + currentBundleName);
            return;
        }

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

    void OnDestroy()
    {
        if (loadedBundle != null) loadedBundle.Unload(true);
    }
}