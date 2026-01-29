using UnityEngine;

public class AutoSaveManager : MonoBehaviour
{
    [Header("Dữ liệu cần lưu")]
    public string playerName = "Player 1";
    public int currentLevel = 1;
    public float musicVolume = 0.8f;
    public bool isMuted = false;

    public static AutoSaveManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        LoadData();
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey("Saved_PlayerName"))
        {
            playerName = PlayerPrefs.GetString("Saved_PlayerName");
            currentLevel = PlayerPrefs.GetInt("Saved_Level", 1);
            musicVolume = PlayerPrefs.GetFloat("Saved_Volume", 0.5f);
            isMuted = PlayerPrefs.GetInt("Saved_Muted", 0) == 1; // Convert int sang bool
            
            Debug.Log("<color=green>Dữ liệu đã được tải tự động!</color>");
        }
        else
        {
            Debug.Log("<color=yellow>Không tìm thấy dữ liệu cũ, dùng mặc định.</color>");
        }
    }

    // 2. HÀM LƯU DỮ LIỆU
    public void SaveData()
    {
        PlayerPrefs.SetString("Saved_PlayerName", playerName);
        PlayerPrefs.SetInt("Saved_Level", currentLevel);
        PlayerPrefs.SetFloat("Saved_Volume", musicVolume);
        PlayerPrefs.SetInt("Saved_Muted", isMuted ? 1 : 0); 

        PlayerPrefs.Save();
        Debug.Log("<color=cyan>Dữ liệu đã được lưu tự động!</color>");
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveData();
        }
    }

    [ContextMenu("Reset All Data")]
    public void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Đã xóa sạch dữ liệu!");
    }
}