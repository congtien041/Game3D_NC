using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("Chỉ số hiện tại (Runtime)")]
    public string monsterName;
    public float maxHp;
    public float currentHp;
    public float damage;

    // Hàm này được gọi bởi Spawner
    public void SetupMonster(MonsterData data)
    {
        // 1. Copy dữ liệu mới
        this.monsterName = data.monsterName;
        this.maxHp = data.hp;
        this.currentHp = data.hp; // Reset máu về đầy
        this.damage = data.damage;

        // 2. Cập nhật tên trong Hierarchy để dễ tìm
        this.gameObject.name = $"{monsterName}_HP{maxHp}_{GetInstanceID()}";

        // 3. LOG KIỂM TRA (Dòng này sẽ hiện trong Console khi quái đẻ ra)
        Debug.Log($"<color=green>[Quái Spawn]</color> Tên: {monsterName} | Nhận Máu Mới: {this.maxHp}");
    }
}