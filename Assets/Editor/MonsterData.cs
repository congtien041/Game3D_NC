using UnityEngine;

// File này định nghĩa dữ liệu cho Quái/Item
[CreateAssetMenu(fileName = "NewMonster", menuName = "GameData/Monster")]
public class MonsterData : ScriptableObject
{
    public string monsterName;
    public float hp;
    public float damage;
    
    // Tên chính xác của Prefab nằm trong AssetBundle kia
    public string prefabName; 
}