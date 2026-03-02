using UnityEngine;
using VehicleSystem.UI; // Thêm dòng này để gọi được CarColorPicker

public class GarageSimpleSpawner : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject[] emptyCarPrefabs;
    
    [Header("UI References")]
    public CarColorPicker colorPicker; // MỚI: Khai báo tham chiếu đến bảng chọn màu

    private GameObject currentCarInstance;

    private void Start()
    {
        SpawnCarByIndex(0);
    }

    public void SpawnCarByIndex(int index)
    {
        if (emptyCarPrefabs == null || index < 0 || index >= emptyCarPrefabs.Length)
        {
            Debug.LogWarning("Không tìm thấy xe ở vị trí số: " + index);
            return;
        }

        // Xóa xe cũ nếu có
        if (currentCarInstance != null)
        {
            Destroy(currentCarInstance);
        }

        // Spawn xe mới
        if (emptyCarPrefabs[index] != null)
        {
            currentCarInstance = Instantiate(emptyCarPrefabs[index]);
            currentCarInstance.transform.SetParent(spawnPoint);
            currentCarInstance.transform.localPosition = Vector3.zero; 
            currentCarInstance.transform.localRotation = Quaternion.identity;
            currentCarInstance.transform.localScale = new Vector3(1, 1, 1);
            
            Rigidbody rb = currentCarInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; 
                rb.useGravity = false; 
            }

            // MỚI: Gửi Root của xe mới này sang cho Color Picker để nó đổi màu
            if (colorPicker != null)
            {
                colorPicker.SetupNewCar(currentCarInstance.transform);
            }
            else
            {
                Debug.LogWarning("Chưa kéo CarColorPicker vào GarageSimpleSpawner!");
            }
        }
    }
}