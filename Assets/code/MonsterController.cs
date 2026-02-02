using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("Chỉ số hiện tại (Runtime)")]
    public string monsterName;
    public float maxHp;
    public float currentHp;
    public float damage;

    public void SetupMonster(MonsterData data)
    {
        this.monsterName = data.monsterName;
        this.maxHp = data.hp;
        this.currentHp = data.hp; 
        this.damage = data.damage;

        // this.gameObject.name = $"{monsterName}_{GetInstanceID()}";
    }
}