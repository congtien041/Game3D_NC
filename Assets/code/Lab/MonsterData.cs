using UnityEngine;

[CreateAssetMenu(fileName = "NewMonster", menuName = "GameData/Monster")]
public class MonsterData : ScriptableObject
{
    public string monsterName;
    public float hp;
    public float damage;
    
    public string prefabName; 
}