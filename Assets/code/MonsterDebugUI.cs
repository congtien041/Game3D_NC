using UnityEngine;

public class MonsterDebugUI : MonoBehaviour
{
    // Tham chiếu đến bộ Spawner để lấy dữ liệu
    public LoadAssetBundle spawner;

    // Biến tạm để lưu text người dùng nhập
    private string inputHP = "";
    private string inputDamage = "";
    private string inputInterval = "";

    // Biến kiểm tra xem đã lấy được data chưa
    private bool isInitialized = false;

    void Start()
    {
        // Tự động tìm script LoadAssetBundle trong scene nếu chưa gán
        if (spawner == null)
            spawner = FindFirstObjectByType<LoadAssetBundle>();
    }

    void OnGUI()
    {
        // 1. Vẽ cái hộp nền ở góc trái trên màn hình
        GUI.Box(new Rect(10, 10, 250, 200), "BẢNG CHỈNH SỐ LIỆU (DEBUG)");

        if (spawner == null || spawner._loadedData == null)
        {
            GUI.Label(new Rect(20, 40, 230, 30), "Đang chờ load data...");
            return;
        }

        // Nếu lần đầu tiên thấy data, cập nhật hiển thị lên ô nhập
        if (!isInitialized)
        {
            RefreshUI();
            isInitialized = true;
        }

        // --- HÀNG 1: Tên quái ---
        GUI.Label(new Rect(20, 40, 200, 20), $"Quái: {spawner._loadedData.monsterName}");

        // --- HÀNG 2: Máu (HP) ---
        GUI.Label(new Rect(20, 70, 60, 20), "Max HP:");
        // Ô nhập liệu cho HP
        inputHP = GUI.TextField(new Rect(90, 70, 100, 20), inputHP);

        // --- HÀNG 3: Sát thương (Damage) ---
        GUI.Label(new Rect(20, 100, 60, 20), "Damage:");
        inputDamage = GUI.TextField(new Rect(90, 100, 100, 20), inputDamage);

        // --- HÀNG 4: Tốc độ đẻ quái ---
        GUI.Label(new Rect(20, 130, 60, 20), "Tốc độ:");
        inputInterval = GUI.TextField(new Rect(90, 130, 100, 20), inputInterval);

        // --- NÚT BẤM: Cập nhật ---
        if (GUI.Button(new Rect(20, 160, 210, 30), "CẬP NHẬT NGAY"))
        {
            ApplyChanges();
        }
    }

    // Hàm lưu giá trị từ ô nhập vào ScriptableObject
    void ApplyChanges()
    {
        if (spawner != null && spawner._loadedData != null)
        {
            // Chuyển đổi text sang số (float)
            float newHp, newDmg, newSpeed;

            if (float.TryParse(inputHP, out newHp))
            {
                spawner._loadedData.hp = newHp;
            }

            if (float.TryParse(inputDamage, out newDmg))
            {
                spawner._loadedData.damage = newDmg;
            }
            
            // Cập nhật tốc độ spawn trực tiếp vào biến của spawner
            if (float.TryParse(inputInterval, out newSpeed))
            {
                spawner.spawnInterval = newSpeed;
            }

            Debug.Log($"Đã cập nhật: HP={newHp}, DMG={newDmg}, Speed={newSpeed}");
        }
    }

    // Hàm lấy giá trị hiện tại điền vào ô nhập
    void RefreshUI()
    {
        inputHP = spawner._loadedData.hp.ToString();
        inputDamage = spawner._loadedData.damage.ToString();
        inputInterval = spawner.spawnInterval.ToString();
    }
}