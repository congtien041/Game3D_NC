using System.Collections.Generic;
using UnityEngine;

public class RadiusItemSystem : MonoBehaviour
{
    [Header("Cấu hình Player")]
    public Transform player;

    [Header("Cấu hình Item")]
    public GameObject[] itemPrefabs; // Danh sách các item (Coin, Đá, Cây...)
    
    [Header("Cấu hình Bán Kính")]
    public float spawnRadius = 30f;    // Bán kính sinh ra item
    public float despawnRadius = 45f;  // Bán kính hủy item (Nên > spawnRadius)
    
    [Header("Tần suất sinh")]
    public float spawnInterval = 0.2f; // Bao nhiêu giây sinh 1 lần

    private List<GameObject> activeItems = new List<GameObject>();
    private float timer;

    void Start()
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // 1. Sinh Item theo thời gian
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnItem();
            timer = 0f;
        }

        // 2. Kiểm tra và Hủy Item theo bán kính N
        CheckAndDespawn();
    }

    void SpawnItem()
    {
        if (itemPrefabs.Length == 0) return;

        // Logic: Sinh ngẫu nhiên phía trước mặt người chơi (từ 5m đến spawnRadius)
        // Dùng Random.insideUnitCircle để lấy tọa độ X, Z ngẫu nhiên
        Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
        
        // Chỉ lấy các điểm ở phía trước (Z dương) để item không sinh sau lưng
        float randomZ = Mathf.Abs(randomPoint.y) + 5f; 
        
        Vector3 spawnPos = new Vector3(
            player.position.x + randomPoint.x, // Lệch trái phải ngẫu nhiên
            0.5f,                              // Độ cao (Y) cố định
            player.position.z + randomZ        // Luôn ở phía trước
        );

        GameObject item = Instantiate(itemPrefabs[Random.Range(0, itemPrefabs.Length)], spawnPos, Quaternion.identity);
        
        // Đưa vào danh sách quản lý
        item.transform.SetParent(transform);
        activeItems.Add(item);
    }

    void CheckAndDespawn()
    {
        // Duyệt ngược danh sách để xóa an toàn
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            GameObject item = activeItems[i];

            // Nếu item đã bị ăn mất (null) -> xóa khỏi list
            if (item == null)
            {
                activeItems.RemoveAt(i);
                continue;
            }

            // Tính khoảng cách
            float dist = Vector3.Distance(player.position, item.transform.position);

            // Nếu vượt quá bán kính Despawn -> Xóa
            if (dist > despawnRadius)
            {
                Destroy(item);
                activeItems.RemoveAt(i);
            }
        }
    }

    // Vẽ vòng tròn trong Editor để dễ chỉnh
    private void OnDrawGizmos()
    {
        if (player)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, spawnRadius); // Vùng sinh
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, despawnRadius); // Vùng chết
        }
    }
}