using UnityEngine;
using TMPro;

namespace VehicleSystem.UI
{
    public class RaceUIManager : MonoBehaviour
    {
        public static RaceUIManager Instance;
        [Header("Warning UI")]
        public GameObject warningPanel;
        public TextMeshProUGUI warningText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            HideWarning(); 
        }

        public void ShowWarning(string message)
        {
            if (warningPanel) warningPanel.SetActive(true);
            if (warningText) warningText.text = message;
        }

        public void HideWarning()
        {
            if (warningPanel) warningPanel.SetActive(false);
        }
    }
}