using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance;

    [Header("Dữ liệu trò chơi")]
    public int totalCoins = 0;
    public float musicVolume = 1.0f;

    private void Awake()
    {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData(); // Tự động tải khi mở game
        } else {
            Destroy(gameObject);
        }
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
        Debug.Log("Đã lưu: Coin = " + totalCoins + " | Vol = " + musicVolume);
    }

    public void LoadData()
    {
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
    }

    private void OnApplicationQuit() => SaveData();
    private void OnApplicationPause(bool pause) { if (pause) SaveData(); }
}