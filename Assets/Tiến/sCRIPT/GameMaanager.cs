// using UnityEngine;

// public class GameManager : MonoBehaviour
// {
//     public static GameManager Instance;
//     public GameObject[] carPrefabs; 
//     public Transform spawnPoint;

//     [Header("References")]
//     public GameHUD gameHUD;
//     public CameraMovement cameraMovement;

//     void Awake() { Instance = this; }

//     void Start()
//     {
//         int selectedCar = PlayerPrefs.GetInt("SelectedCarIndex", 0);
//         GameObject playerCarObj = Instantiate(carPrefabs[selectedCar], spawnPoint.position, spawnPoint.rotation);
        
//         // Cập nhật Camera theo xe mới
//         // if (cameraMovement != null) cameraMovement.SetActiveCar(playerCarObj.transform);

//         // Cập nhật HUD theo xe mới
//         if (gameHUD != null) gameHUD.playerCar = playerCarObj.GetComponent<CarController>();

//         if (GlobalAudio.Instance != null) 
//             GlobalAudio.Instance.PlaySFX(GlobalAudio.Instance.ignition);
//     }
// }