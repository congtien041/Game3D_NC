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

    // --- BIẾN MỚI THÊM VÀO ---
    private bool isRaceStarted = false; // Kiểm tra xem đua đã bắt đầu chưa
    private float raceStartTime;        // Thời điểm bắt đầu đua
    private bool playerFinished = false;
    
    // Biến chống lặp (Cooldown): Giúp xe không bị tính 2 lần khi vừa chạm vạch
    private float lastTriggerTime = -999f;
    private float triggerCooldown = 3.0f; // 3 giây sau khi xuất phát mới được tính là về đích (tránh lỗi)

    void Start()
    {
        // Lúc đầu game chưa tính giờ ngay
        if (resultPanel != null)
            resultPanel.SetActive(false);
            
        isRaceStarted = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ xử lý logic cho Player theo yêu cầu
        if (other.CompareTag("Player"))
        {
            // Kiểm tra thời gian hồi (tránh va chạm kép trong 1 khung hình)
            if (Time.time < lastTriggerTime + triggerCooldown) return;

            // --- TRƯỜNG HỢP 1: LẦN ĐẦU CHẠM (XUẤT PHÁT) ---
            if (!isRaceStarted)
            {
                isRaceStarted = true;
                raceStartTime = Time.time; // Ghi lại mốc thời gian bắt đầu
                lastTriggerTime = Time.time; // Ghi lại thời gian va chạm
                
                Debug.Log("Đã qua vạch xuất phát! Bắt đầu tính giờ.");
                
                // Gợi ý: Tại đây bạn có thể hiện UI thông báo "GO!"
            }
            // --- TRƯỜNG HỢP 2: LẦN SAU CHẠM (VỀ ĐÍCH) ---
            else if (!playerFinished)
            {
                playerFinished = true;
                float finalDuration = Time.time - raceStartTime; // Tính tổng thời gian chạy
                
                Debug.Log("Đã về đích! Hoàn thành vòng đua.");
                ShowResultUI(finalDuration);
            }
        }
    }

    // Hàm hiển thị UI với định dạng mới
    void ShowResultUI(float time)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);

            if (timeText != null)
            {
                // Tính toán Phút và Giây
                // Mathf.FloorToInt làm tròn xuống số nguyên (VD: 64s -> 1m)
                int minutes = Mathf.FloorToInt(time / 60); 
                int seconds = Mathf.FloorToInt(time % 60);

                // Định dạng chuỗi theo yêu cầu: "Time: 1m4s"
                timeText.text = $"Time: {minutes}m{seconds}s";
            }
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}