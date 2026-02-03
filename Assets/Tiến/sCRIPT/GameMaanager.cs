using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject[] carPrefabs; // Kéo thả các mẫu xe vào đây theo đúng thứ tự
    public Transform spawnPoint;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        int selectedCar = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        Instantiate(carPrefabs[selectedCar], spawnPoint.position, spawnPoint.rotation);
        GlobalAudio.Instance.PlaySFX(GlobalAudio.Instance.ignition);
    }
}