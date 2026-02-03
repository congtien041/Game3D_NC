using UnityEngine;
using UnityEngine.SceneManagement;

public class CarSelection : MonoBehaviour
{
    public GameObject allCarsContainer;
    private GameObject[] allCars;
    private int currentIndex = 0;

    void Start() {
        allCars = new GameObject[allCarsContainer.transform.childCount];
        for (int i = 0; i < allCarsContainer.transform.childCount; i++) {
            allCars[i] = allCarsContainer.transform.GetChild(i).gameObject;
            allCars[i].SetActive(false);
        }
        currentIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        ShowCurrentCar();
    }

    void ShowCurrentCar() {
        foreach (GameObject car in allCars) car.SetActive(false);
        allCars[currentIndex].SetActive(true);
        PlayerPrefs.SetInt("SelectedCarIndex", currentIndex);
    }

    public void NextCar() { currentIndex = (currentIndex + 1) % allCars.Length; ShowCurrentCar(); }
    public void PreviousCar() { currentIndex = (currentIndex - 1 + allCars.Length) % allCars.Length; ShowCurrentCar(); }

    public void OnDoneButton() {
        PlayerPrefs.Save();
        SceneManager.LoadScene("Game"); 
    }

    public void UpgradeEngine() {
        int currentLvl = PlayerPrefs.GetInt("CarUpgrade_" + currentIndex, 0);
        
        // KIỂM TRA TỐI ĐA 5 CẤP
        if (currentLvl >= 5) {
            Debug.Log("Xe này đã đạt cấp độ tối đa!");
            return;
        }

        int playerMoney = PlayerPrefs.GetInt("TotalMoney", 0);
        int cost = 1000; 

        if (playerMoney >= cost) {
            PlayerPrefs.SetInt("CarUpgrade_" + currentIndex, currentLvl + 1);
            PlayerPrefs.SetInt("TotalMoney", playerMoney - cost);
            PlayerPrefs.Save();
            Debug.Log("Nâng cấp thành công lên cấp " + (currentLvl + 1));
        }
    }
}