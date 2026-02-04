using UnityEngine;
using UnityEngine.UI; // Cần thư viện này để làm việc với UI

public class CarSelector : MonoBehaviour
{
    [Header("Kéo cái Ảnh To (trên cùng) vào đây")]
    public Image anhHienThi; 

    private Image anhCuaNut;
    private Button nutBam;

    void Start()
    {
        // 1. Tự động tìm component Image và Button trên chính cái nút này
        anhCuaNut = GetComponent<Image>();
        nutBam = GetComponent<Button>();

        // 2. Tự động lắng nghe sự kiện Click
        if (nutBam != null)
        {
            nutBam.onClick.AddListener(ThayDoiAnh);
        }
    }

    void ThayDoiAnh()
    {
        if (anhHienThi != null && anhCuaNut != null)
        {
            // Lấy sprite của nút này -> Gán cho sprite của ảnh to
            anhHienThi.sprite = anhCuaNut.sprite;

            // (Tuỳ chọn) Giữ tỉ lệ ảnh gốc để không bị méo hình
            anhHienThi.preserveAspect = true; 
            
            // Debug để kiểm tra xem code có chạy không
            Debug.Log("Đã đổi ảnh sang: " + anhCuaNut.sprite.name);
        }
    }
}