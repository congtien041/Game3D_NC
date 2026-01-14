using System.Collections.Generic;
using UnityEngine;

public class InfiniteMapSystem : MonoBehaviour
{
    [Header("Cấu hình Map")]
    public GameObject[] mapPrefabs;  // Các đoạn đường (Prefab)
    public Transform player;
    
    [Header("Thông số kỹ thuật")]
    public float tileLength = 20f;   // Độ dài chính xác của 1 đoạn đường (QUAN TRỌNG)
    public int initialTiles = 5;     // Số lượng map khởi tạo ban đầu
    public float bufferDistance = 40f; // Khoảng cách đệm (sinh trước khi player tới nơi)

    private List<GameObject> activeTiles = new List<GameObject>();
    private float spawnZ = 0;        // Vị trí Z tiếp theo để đặt map

    void Start()
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;

        // Sinh ra các đoạn đường đầu tiên để lấp đầy màn hình
        for (int i = 0; i < initialTiles; i++)
        {
            SpawnTile(Random.Range(0, mapPrefabs.Length));
        }
    }

    void Update()
    {
        // Logic: Nếu Player chạy gần đến điểm cuối của danh sách map hiện tại
        // (Vị trí Player > Vị trí Spawn tiếp theo - Tổng độ dài map đang có + vùng đệm)
        if (player.position.z > spawnZ - (initialTiles * tileLength) + bufferDistance)
        {
            SpawnTile(Random.Range(0, mapPrefabs.Length));
            DeleteOldTile();
        }
    }

    void SpawnTile(int prefabIndex)
    {
        // Tạo map mới tại vị trí spawnZ
        GameObject go = Instantiate(mapPrefabs[prefabIndex], Vector3.forward * spawnZ, Quaternion.identity);
        go.transform.SetParent(transform);
        
        activeTiles.Add(go);
        
        // Tịnh tiến điểm spawnZ lên phía trước
        spawnZ += tileLength;
    }

    void DeleteOldTile()
    {
        // Xóa phần tử đầu tiên trong danh sách (là map cũ nhất phía sau lưng)
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
    }
}