using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    public CarController playerCar;
    public Text speedText;
    public Text gearText;
    public Slider nitroSlider;

    void Update()
    {
        if (playerCar == null) {
            playerCar = GameObject.FindWithTag("Player").GetComponent<CarController>();
            return;
        }

        // Cập nhật thông số lên màn hình
        speedText.text = Mathf.RoundToInt(playerCar.speedKmh) + " KM/H";
        gearText.text = "GEAR: " + playerCar.currentGear;
        nitroSlider.value = playerCar.nitroCharge;
    }
}