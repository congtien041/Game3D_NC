using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarShopUI : MonoBehaviour
{
    [Header("UI Chung")]
    public TextMeshProUGUI txtMoney;     // Text hiển thị tiền người chơi
    public Button btnPlay;               // Nút Play (Xanh)
    public Button btnAction;             // Nút Hành động (Trắng: Buy/Upgrade)
    public TextMeshProUGUI txtAction;    // Chữ bên trong nút Hành động

    [Header("Danh sách Xe (UI)")]
    public CarItem[] cars;               // Điền số 3 vào Size

    private int currentCarIndex = 0;
    private int currentMoney;

    [System.Serializable]
    public class CarItem
    {
        public string carName;           // Tên xe (để hiển thị hoặc debug)
        public string carID;             // ID duy nhất (ví dụ: car_1, car_2)
        public int price;                // Giá tiền
        public bool isFree;              // Có phải xe mặc định không?
        public Button selectButton;      // Nút nhỏ bên dưới để chọn xe này
    }

    void Start()
    {
        // 1. Lấy tiền (Mặc định 5000)
        currentMoney = PlayerPrefs.GetInt("TotalMoney", 5000);
        UpdateMoneyUI();

        // 2. Cài đặt sự kiện click cho từng nút chọn xe
        for (int i = 0; i < cars.Length; i++)
        {
            int index = i; 
            // Khi bấm vào nút ảnh nhỏ -> Gọi hàm SelectCar
            cars[i].selectButton.onClick.AddListener(() => SelectCar(index));
        }

        // 3. Chọn xe đầu tiên khi vào game
        SelectCar(0);

        // 4. Gán sự kiện cho 2 nút lớn
        btnAction.onClick.AddListener(OnActionClick);
        btnPlay.onClick.AddListener(OnPlayClick);
    }

    // Hàm xử lý khi chọn xe (Chỉ xử lý logic UI)
    void SelectCar(int index)
    {
        currentCarIndex = index;
        Debug.Log("Đã chọn xe UI: " + cars[index].carName);

        // Cập nhật trạng thái nút Play/Buy theo xe vừa chọn
        CheckCarStatus();
    }

    void CheckCarStatus()
    {
        CarItem currentCar = cars[currentCarIndex];

        // Kiểm tra đã mua hay chưa
        bool isOwned = currentCar.isFree || PlayerPrefs.GetInt(currentCar.carID + "_Owned", 0) == 1;

        if (isOwned)
        {
            // --- ĐÃ MUA ---
            txtAction.text = "UPGRADE";      // Đổi chữ thành Upgrade
            btnPlay.interactable = true;     // Cho phép bấm Play
        }
        else
        {
            // --- CHƯA MUA ---
            txtAction.text = "BUY " + currentCar.price + "$"; // Hiện giá tiền
            btnPlay.interactable = false;    // Khóa nút Play
        }
    }

    void OnActionClick()
    {
        CarItem currentCar = cars[currentCarIndex];
        bool isOwned = currentCar.isFree || PlayerPrefs.GetInt(currentCar.carID + "_Owned", 0) == 1;

        if (isOwned)
        {
            // Logic Nâng cấp (Nếu có)
            Debug.Log("Nâng cấp xe: " + currentCar.carName);
        }
        else
        {
            // Logic Mua xe
            if (currentMoney >= currentCar.price)
            {
                currentMoney -= currentCar.price;
                
                // Lưu dữ liệu
                PlayerPrefs.SetInt("TotalMoney", currentMoney);
                PlayerPrefs.SetInt(currentCar.carID + "_Owned", 1);
                PlayerPrefs.Save();

                UpdateMoneyUI();
                CheckCarStatus(); // Cập nhật lại giao diện ngay lập tức
                Debug.Log("Mua thành công!");
            }
            else
            {
                Debug.Log("Không đủ tiền!");
            }
        }
    }

    void OnPlayClick()
    {
        // Logic vào game
        Debug.Log("Vào game với xe: " + cars[currentCarIndex].carID);
        // SceneManager.LoadScene("GamePlay");
    }

    void UpdateMoneyUI()
    {
        if (txtMoney != null) txtMoney.text = currentMoney.ToString();
    }
}