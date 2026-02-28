using UnityEngine;
using UnityEngine.SceneManagement; // Thư viện để chuyển Scene
using TMPro; // Thư viện TextMeshPro cho UI đẹp (nếu dùng Text thường thì đổi thành UnityEngine.UI)

public class RaceFinishManagers : MonoBehaviour
{
    [Header("Cài đặt chung")]
    public string menuSceneName = "MainMenu"; // Tên scene Menu để quay về (Public như bạn yêu cầu)
    
    [Header("UI Kết quả")]
    public GameObject resultPanel;      // Panel chứa bảng kết quả (ẩn đi lúc đầu)
    // public TextMeshProUGUI rankText;    // Text hiển thị hạng (VD: "Hạng: 1")
    public TextMeshProUGUI timeText;    // Text hiển thị thời gian (VD: "01:23.45")

    // Biến nội bộ để tính toán
    private float startTime;
    private int currentRankPosition = 1; // Bắt đầu là hạng 1
    private bool playerFinished = false;

    void Start()
    {
        // Ghi lại thời gian bắt đầu đua
        startTime = Time.time;
        
        // Ẩn bảng kết quả khi bắt đầu game
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    // Hàm này chạy khi có vật thể đi qua vạch đích
    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem đối tượng va chạm là Player hay Enemy
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            // Tính toán thời gian chạy của đối tượng này
            float finishTime = Time.time - startTime;
            
            // Lấy hạng hiện tại và tăng biến đếm hạng lên cho người sau
            int rank = currentRankPosition;
            currentRankPosition++;

            Debug.Log(other.name + " đã về đích! Hạng: " + rank + " - Thời gian: " + finishTime);

            // Nếu là Player thì hiện bảng UI
            if (other.CompareTag("Player") && !playerFinished)
            {
                playerFinished = true;
                ShowResultUI(rank, finishTime);
                
                // Tùy chọn: Tắt điều khiển xe/nhân vật tại đây nếu cần
                // other.GetComponent<CarController>().enabled = false; 
            }
            else
            {
                // Nếu là Enemy, có thể cho Enemy dừng lại hoặc chạy tiếp tùy logic game
                // Destroy(other.gameObject, 2f); // Ví dụ: Xóa Enemy sau 2s về đích
            }
        }
    }

    // Hàm hiển thị UI
    void ShowResultUI(int rank, float time)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true); // Hiện bảng

            // Hiển thị hạng
            // if (rankText != null)
            //     rankText.text = "THỨ HẠNG: " + rank;

            // Hiển thị thời gian định dạng Phút:Giây
            if (timeText != null)
            {
                string minutes = Mathf.Floor(time / 60).ToString("00");
                string seconds = (time % 60).ToString("00.00");
                timeText.text = "THỜI GIAN: " + minutes + ":" + seconds;
            }
        }
    }

    // Hàm này gắn vào nút "Về Menu" trên UI
    public void BackToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}