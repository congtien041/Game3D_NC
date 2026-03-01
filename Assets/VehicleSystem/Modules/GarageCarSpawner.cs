using UnityEngine;

public class GarageSimpleSpawner : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject[] emptyCarPrefabs;
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

        if (currentCarInstance != null)
        {
            Destroy(currentCarInstance);
        }
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
        }
    }
}