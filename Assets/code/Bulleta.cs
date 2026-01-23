using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 40f; // Lượng máu sẽ trừ

    void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem thứ vừa chạm vào có phải NPC không
        if (other.CompareTag("NPC"))
        {
            // Lấy script GuardAI từ NPC đó
            GuardAI npc = other.GetComponent<GuardAI>();
            
            if (npc != null)
            {
                npc.health -= damage; // Trừ máu NPC
                Debug.Log("NPC trúng đạn! Máu còn: " + npc.health);
            }

            // Phá hủy viên đạn sau khi trúng
            Destroy(gameObject);
        }
    }
}