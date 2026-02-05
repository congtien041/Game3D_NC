using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RaceFinishManager : MonoBehaviour
{
    [Header("Cài đặt chung")]
    public string menuSceneName = "MainMenu";

    [Header("UI Kết quả")]
    public GameObject resultPanel;
    public TextMeshProUGUI timeText;

    // Biến logic
    private bool isRaceStarted = false; 
    private float raceStartTime;        
    private bool playerFinished = false;
    
    // Biến chống lặp (Cooldown)
    private float lastTriggerTime = -999f;
    private float triggerCooldown = 3.0f; // 3 giây cooldown

    void Start()
    {
        Debug.Log("--- Game Bắt Đầu: Script RaceFinishManager đã chạy ---");
        
        if (resultPanel != null)
            resultPanel.SetActive(false);
        else
            Debug.LogWarning("CHÚ Ý: Chưa gắn Result Panel vào Script!");

        isRaceStarted = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Kiểm tra xem cái gì vừa chạm vào vạch
        Debug.Log($"[Va Chạm] Vật thể: '{other.name}' - Tag: '{other.tag}' đã chạm vạch.");

        // Chỉ xử lý nếu là Player
        if (other.CompareTag("Player"))
        {
            // 2. Kiểm tra Cooldown
            if (Time.time < lastTriggerTime + triggerCooldown)
            {
                float timeRemaining = (lastTriggerTime + triggerCooldown) - Time.time;
                Debug.Log($"[Bỏ qua] Đang trong thời gian chờ (Cooldown). Còn lại: {timeRemaining:0.00}s");
                return; // Thoát hàm ngay lập tức
            }

            // Cập nhật thời gian va chạm hợp lệ gần nhất
            lastTriggerTime = Time.time;

            // --- TRƯỜNG HỢP 1: LẦN ĐẦU CHẠM (XUẤT PHÁT) ---
            if (!isRaceStarted)
            {
                isRaceStarted = true;
                raceStartTime = Time.time; 
                
                Debug.Log("<color=green>--- XUẤT PHÁT! Bắt đầu tính giờ ---</color>");
                Debug.Log($"Thời điểm bắt đầu: {raceStartTime}");
            }
            // --- TRƯỜNG HỢP 2: LẦN SAU CHẠM (VỀ ĐÍCH) ---
            else if (!playerFinished)
            {
                playerFinished = true;
                float finalDuration = Time.time - raceStartTime;
                
                Debug.Log($"<color=yellow>--- VỀ ĐÍCH! ---</color>");
                Debug.Log($"Tổng thời gian chạy: {finalDuration} giây");

                ShowResultUI(finalDuration);
            }
        }
        else
        {
            Debug.LogWarning($"[Cảnh báo] Vật thể '{other.name}' không có Tag là 'Player'. Hãy kiểm tra lại Inspector.");
        }
    }

    void ShowResultUI(float time)
    {
        Debug.Log("Đang hiển thị bảng kết quả...");

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);

            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(time / 60); 
                int seconds = Mathf.FloorToInt(time % 60);

                string timeString = $"Time: {minutes}m{seconds}s";
                timeText.text = timeString;
                
                Debug.Log($"Đã cập nhật Text thành: '{timeString}'");
            }
            else
            {
                Debug.LogError("LỖI: Chưa gắn Time Text vào Script!");
            }
        }
        else
        {
            Debug.LogError("LỖI: Chưa gắn Result Panel vào Script!");
        }
    }

    public void BackToMenu()
    {
        Debug.Log("Người chơi bấm nút về Menu.");
        SceneManager.LoadScene(menuSceneName);
    }
}