using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeMenuUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI statsText;
    public Button upgradeButton;
    public CarSelection carSelection;

    void Update() {
        int currentIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        int totalMoney = PlayerPrefs.GetInt("TotalMoney", 0);
        int currentLevel = PlayerPrefs.GetInt("CarUpgrade_" + currentIndex, 0);

        moneyText.text = "Tiền: $" + totalMoney;

        if (currentLevel >= 5) {
            statsText.text = "CẤP ĐỘ: TỐI ĐA (5)\nĐã mở khóa toàn bộ sức mạnh!";
            upgradeButton.interactable = false; 
        } else {
            statsText.text = "Cấp độ: " + currentLevel + "/5\nTốc độ: +" + (currentLevel * 250) + "\nPhí: $1000";
            upgradeButton.interactable = true;
        }
    }

    public void OnUpgradeClick() {
        carSelection.UpgradeEngine();
        // Fix lỗi gọi buttonClick
        if (GlobalAudio.Instance != null)
            GlobalAudio.Instance.PlaySFX(GlobalAudio.Instance.buttonClick);
    }

    public void OnHackMoney() {
        PlayerPrefs.SetInt("TotalMoney", PlayerPrefs.GetInt("TotalMoney", 0) + 10000);
        PlayerPrefs.Save();
    }
}