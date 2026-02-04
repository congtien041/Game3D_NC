using UnityEngine;

public class NitroSpawnerOnRoad : MonoBehaviour
{
    public GameObject nitroPrefab;
    public GameObject roadObject; // Kéo Object mặt đường vào đây
    public float spawnInterval = 2f;
    public float nitroHeight = 0.5f; // Chiều cao cách mặt đất

    private Collider roadCollider;
    private float timer;

    void Start()
    {
        // Lấy Collider của mặt đường để tính toán kích thước
        roadCollider = roadObject.GetComponent<Collider>();
        
        if (roadCollider == null)
        {
            Debug.LogError("Object mặt đường cần có Collider (Box Collider, Mesh Collider,...)");
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnNitro();
            timer = 0f;
        }
    }

    void SpawnNitro()
    {
        if (roadCollider == null) return;

        // Lấy giới hạn không gian của mặt đường
        Bounds bounds = roadCollider.bounds;

        // Tạo vị trí ngẫu nhiên nằm trong Bounds của đường
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        
        // Vị trí Y sẽ lấy theo bề mặt đường + một khoảng offset
        float spawnY = bounds.max.y + nitroHeight;

        Vector3 spawnPosition = new Vector3(randomX, spawnY, randomZ);

        // Tạo Nitro
        Instantiate(nitroPrefab, spawnPosition, Quaternion.identity);
    }
}