using UnityEngine;
using UnityEngine.UI;
using VehicleSystem.Managers; // Để gọi GameMode

namespace VehicleSystem.UI
{
    public class ModeSelectionManager : MonoBehaviour
    {
        public static ModeSelectionManager Instance { get; private set; }
        public Button circuitBtn;
        public Button timeAttackBtn;
        public Button FreeRaom;
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            GameMode savedMode = (GameMode)PlayerPrefs.GetInt("SelectedGameMode", 0);
            circuitBtn.onClick.AddListener(() => SelectMode(GameMode.Circuit));
            timeAttackBtn.onClick.AddListener(() => SelectMode(GameMode.TimeAttack));
            FreeRaom.onClick.AddListener(() => SelectMode(GameMode.FreeRoam));
            
        }

        private void SelectMode(GameMode mode)
        {
            PlayerPrefs.SetInt("SelectedGameMode", (int)mode);
            PlayerPrefs.Save();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentMode = mode;
            }
        }
    }
}