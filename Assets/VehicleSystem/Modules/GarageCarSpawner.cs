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
            currentCarInstance = Instantiate(emptyCarPrefabs[index], spawnPoint.position, spawnPoint.rotation);
            currentCarInstance.transform.SetParent(spawnPoint);
        }
    }
}