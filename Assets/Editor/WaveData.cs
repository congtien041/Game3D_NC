using UnityEngine;

[CreateAssetMenu(menuName = "GameData/WaveConfig")]
public class WaveData : ScriptableObject
{
    public string bundleName;
    public string monsterName;
    public float spawnSpeed; // Tốc độ đẻ quái
    public int totalAmount;  // Tổng số lượng quái đợt này
}