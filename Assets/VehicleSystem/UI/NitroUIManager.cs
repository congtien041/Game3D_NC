using UnityEngine;
using UnityEngine.UI; 
using VehicleSystem.Modules; 
namespace VehicleSystem.UI
{
    public class NitroUIManager : MonoBehaviour
    {
        public Image nitroFillImage; 
        private CarNitroSystem playerNitroSystem;

        private void Update()
        {
            if (playerNitroSystem == null)
            {
                FindPlayerCar();
                return; 
            }

            UpdateNitroUI();
        }

        private void FindPlayerCar()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerNitroSystem = player.GetComponent<CarNitroSystem>();
                Debug.Log("Lay đc player");
            }
        }

        private void UpdateNitroUI()
        {
            if (nitroFillImage == null) return;
            float fillRatio = playerNitroSystem.currentNitro / playerNitroSystem.maxNitro;
            nitroFillImage.fillAmount = fillRatio;
            if (playerNitroSystem.IsUsingNitro)
            {
                nitroFillImage.color = new Color(1f, 0.6f, 0f); 
            }
            else if (fillRatio >= 1f)
            {
                nitroFillImage.color = Color.cyan; 
            }
            else
            {
                nitroFillImage.color = new Color(0f, 0.5f, 1f); 
            }
        }
    }
}