using UnityEngine;
using UnityEngine.VFX; // Bắt buộc phải có thư viện này

public class NitroController : MonoBehaviour
{
    [Header("Cài đặt")]
    public VisualEffect nitroVFX; // Kéo Gameobject chứa VFX vào đây
    public float nitroStrength = 500f; // Số lượng hạt khi phun
    public KeyCode nitroKey = KeyCode.LeftShift; // Phím bấm

    private bool isNitroOn = false;

    void Start()
    {
        // Đảm bảo ban đầu tắt nitro
        if (nitroVFX != null)
        {
            nitroVFX.SetFloat("NitroRate", 0); 
        }
    }

    void Update()
    {
        // Kiểm tra phím bấm (hoặc chạm màn hình)
        if (Input.GetKeyDown(nitroKey))
        {
            ActivateNitro(true);
        }
        
        if (Input.GetKeyUp(nitroKey))
        {
            ActivateNitro(false);
        }
    }

    // Hàm này có thể gọi từ các script khác (ví dụ khi ăn vật phẩm)
    public void ActivateNitro(bool isActive)
    {
        isNitroOn = isActive;

        if (nitroVFX != null)
        {
            // Chỉnh tham số "NitroRate" mà ta đã tạo trong VFX Graph
            float rate = isActive ? nitroStrength : 0f;
            nitroVFX.SetFloat("NitroRate", rate);
        }
    }
}