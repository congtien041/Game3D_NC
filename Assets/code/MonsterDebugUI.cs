using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; // Thư viện để ép Inspector cập nhật
#endif

public class MonsterDebugUI : MonoBehaviour
{
    public LoadAssetBundle spawner;

    // Dùng string để hứng text nhập vào
    private string inputHP = "";
    private string inputDamage = "";
    private string inputInterval = "";

    private bool isInitialized = false;

    void Start()
    {
        if (spawner == null)
            spawner = FindFirstObjectByType<LoadAssetBundle>();
    }

    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 260, 220), "BẢNG ĐIỀU KHIỂN (LIVE)");

        if (spawner == null || spawner._loadedData == null)
        {
            GUI.Label(new Rect(20, 40, 230, 30), "Đang chờ load data...");
            return;
        }

        // Lấy số liệu lần đầu tiên
        if (!isInitialized)
        {
            RefreshUI();
            isInitialized = true;
        }

        // HIỂN THỊ THÔNG TIN THỰC TẾ TRONG FILE
        GUI.Label(new Rect(20, 30, 240, 20), $"Quái: {spawner._loadedData.monsterName}");
        GUI.Label(new Rect(20, 50, 240, 20), $"<color=yellow>Data Gốc: HP={spawner._loadedData.hp} | DMG={spawner._loadedData.damage}</color>");

        // --- Ô NHẬP LIỆU ---
        GUI.Label(new Rect(20, 80, 60, 20), "Max HP:");
        inputHP = GUI.TextField(new Rect(90, 80, 100, 20), inputHP);

        GUI.Label(new Rect(20, 110, 60, 20), "Damage:");
        inputDamage = GUI.TextField(new Rect(90, 110, 100, 20), inputDamage);

        GUI.Label(new Rect(20, 140, 60, 20), "Tốc độ:");
        inputInterval = GUI.TextField(new Rect(90, 140, 100, 20), inputInterval);

        // --- NÚT CẬP NHẬT ---
        if (GUI.Button(new Rect(20, 170, 220, 35), "CẬP NHẬT NGAY"))
        {
            ApplyChanges();
        }
    }

    void ApplyChanges()
    {
        if (spawner != null && spawner._loadedData != null)
        {
            float parsedVal;

            // Chỉ cập nhật nếu ô nhập KHÔNG TRỐNG và là SỐ HỢP LỆ
            if (float.TryParse(inputHP, out parsedVal))
            {
                spawner._loadedData.hp = parsedVal;
            }

            if (float.TryParse(inputDamage, out parsedVal))
            {
                spawner._loadedData.damage = parsedVal;
            }
            
            if (float.TryParse(inputInterval, out parsedVal))
            {
                spawner.spawnInterval = parsedVal;
            }

            // --- BÍ KÍP: ÉP UNITY INSPECTOR CẬP NHẬT GIAO DIỆN ---
            #if UNITY_EDITOR
            EditorUtility.SetDirty(spawner._loadedData); // Đánh dấu file đã thay đổi
            #endif

            Debug.Log($"<color=cyan>Đã lưu vào Data:</color> HP={spawner._loadedData.hp} | DMG={spawner._loadedData.damage}");
        }
    }

    // Hàm lấy lại số liệu từ data điền vào ô nhập
    void RefreshUI()
    {
        if (spawner._loadedData != null)
        {
            inputHP = spawner._loadedData.hp.ToString();
            inputDamage = spawner._loadedData.damage.ToString();
            inputInterval = spawner.spawnInterval.ToString();
        }
    }
}