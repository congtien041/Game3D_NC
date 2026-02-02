using UnityEngine;

public class CarSelection : MonoBehaviour
{
    public GameObject allCarsContainer;
    private GameObject[] allCars;
    private int currentIndex = 0;

    void Start()
    {
        allCars = new GameObject[allCarsContainer.transform.childCount];
        for (int i = 0; i < allCarsContainer.transform.childCount; i++)
        {
            allCars[i] = allCarsContainer.transform.GetChild(i).gameObject;
            allCars[i].SetActive(false);
        }
        currentIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        ShowCurrentCar();
    }

    void ShowCurrentCar()
    {
        foreach (GameObject car in allCars) car.SetActive(false);
        allCars[currentIndex].SetActive(true);
    }

    public void NextCar() { currentIndex = (currentIndex + 1) % allCars.Length; ShowCurrentCar(); }
    public void PreviousCar() { currentIndex = (currentIndex - 1 + allCars.Length) % allCars.Length; ShowCurrentCar(); }

    // Chỉ giữ lại một hàm OnDoneButton duy nhất
    public void OnDoneButton()
    {
        PlayerPrefs.SetInt("SelectedCarIndex", currentIndex);
        PlayerPrefs.Save();
        // Chuyển sang cảnh đua xe (Racing Scene)
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game"); 
    }

    // Hàm bổ sung để nâng cấp chỉ số xe
    public void UpgradeEngine()
    {
        int currentLevel = PlayerPrefs.GetInt("CarUpgrade_" + currentIndex, 0);
        PlayerPrefs.SetInt("CarUpgrade_" + currentIndex, currentLevel + 1);
        PlayerPrefs.Save();
        Debug.Log("Đã nâng cấp xe " + currentIndex + " lên cấp " + (currentLevel + 1));
    }
}