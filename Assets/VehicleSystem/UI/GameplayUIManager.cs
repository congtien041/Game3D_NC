using UnityEngine;
using TMPro;
using VehicleSystem.Managers;

namespace VehicleSystem.UI
{
    public class GameplayUIManager : MonoBehaviour
    {
        public TextMeshProUGUI mainInfoText; 

        public void UpdateMainInfoText(string infoString)
        {
            if (mainInfoText != null)
            {
                mainInfoText.text = infoString;
            }
        }
    }
}