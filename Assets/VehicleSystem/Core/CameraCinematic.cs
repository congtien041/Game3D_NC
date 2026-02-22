using UnityEngine;
using System.Collections;

namespace VehicleSystem.Core
{
    public class CameraCinematic : MonoBehaviour
    {
        [Header("Target Settings")]
        private Transform activeCar; 
        private CarControllerVipro carController;

        [Header("Game Mode")]
        public bool isCinematic = true; 
        private bool isWinCinematic = false;   // 🔥 thêm

        [System.Serializable]
        public struct CinematicShot
        {
            public string name;
            public Vector3 offset;    
            public float fov;         
            public Transform lookAtTarget; 
        }

        [Header("Cinematic Settings")]
        public float changeShotInterval = 1.0f; 
        public float cinematicSmoothness = 2.0f; 
        
        public CinematicShot[] introShots = new CinematicShot[]
        {
            new CinematicShot { offset = new Vector3(-2.5f, 1f, 4f), fov = 60 },
            new CinematicShot { offset = new Vector3(4f, 0.5f, 0f), fov = 50 },
            new CinematicShot { offset = new Vector3(0f, 4f, 2f), fov = 70 },
            new CinematicShot { offset = new Vector3(2f, 0.3f, 3.5f), fov = 45 }
        };

        // 🔥 WIN SETTINGS
        [Header("Win Cinematic Settings")]
        
        private float winAngle = 0f;
        public Vector3 winOffset = new Vector3(-4f, 2f, 6f);
        public float winRotateSpeed = 25f;
        public float winFOV = 50f;

        [Header("Gameplay Follow Settings")]
        public float distance = 6.0f;    
        public float height = 2.5f;      
        public float lookAtHeight = 1.0f; 
        public float rotationDamping = 3.0f; 
        public float heightDamping = 2.0f;   

        private int currentShotIndex = 0;
        private float timer = 0;
        private Camera cam; 

        private void Start()
        {
            cam = GetComponent<Camera>();
            
            if (activeCar == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player) activeCar = player.transform;
            }

            if (activeCar != null)
                carController = activeCar.GetComponent<CarControllerVipro>();

            if (isCinematic) RandomizeShot();
        }

        private void LateUpdate()
        {
            if (activeCar == null) return;

            if (isWinCinematic)          // 🔥 ưu tiên win trước
            {
                HandleWinMode();
            }
            else if (isCinematic)
            {
                HandleCinematicMode();
            }
            else
            {
                HandleGameplayMode();
            }
        }

        void HandleCinematicMode()
        {
            timer += Time.deltaTime;
            
            if (timer > changeShotInterval)
            {
                RandomizeShot();
                timer = 0;
            }

            CinematicShot shot = introShots[currentShotIndex];

            Vector3 targetPos = activeCar.TransformPoint(shot.offset);

            transform.position = Vector3.Lerp(
                transform.position,
                targetPos,
                Time.deltaTime * cinematicSmoothness
            );

            transform.LookAt(activeCar.position + Vector3.up * 0.5f);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, shot.fov, Time.deltaTime * 2f);
        }

        // 🔥 WIN MODE
        void HandleWinMode()
{
    winAngle += winRotateSpeed * Time.deltaTime;

    // Tính vị trí xoay quanh xe bằng sin/cos
    Vector3 offset = new Vector3(
        Mathf.Sin(winAngle * Mathf.Deg2Rad) * winOffset.z,
        winOffset.y,
        Mathf.Cos(winAngle * Mathf.Deg2Rad) * winOffset.z
    );

    Vector3 desiredPos = activeCar.position + offset;

    transform.position = Vector3.Lerp(
        transform.position,
        desiredPos,
        Time.deltaTime * 5f   // tăng smooth lên cho mượt
    );

    transform.LookAt(activeCar.position + Vector3.up * 0.8f);

    cam.fieldOfView = Mathf.Lerp(
        cam.fieldOfView,
        winFOV,
        Time.deltaTime * 3f
    );
}

        void RandomizeShot()
        {
            currentShotIndex = Random.Range(0, introShots.Length);
        }

        void HandleGameplayMode()
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60, Time.deltaTime * 2f);

            float wantedRotationAngle = activeCar.eulerAngles.y;
            if (carController != null && carController.isSpinning)
                wantedRotationAngle = transform.eulerAngles.y;

            float wantedHeight = activeCar.position.y + height;

            float currentRotationAngle = transform.eulerAngles.y;
            float currentHeight = transform.position.y;

            currentRotationAngle = Mathf.LerpAngle(
                currentRotationAngle,
                wantedRotationAngle,
                rotationDamping * Time.deltaTime
            );

            currentHeight = Mathf.Lerp(
                currentHeight,
                wantedHeight,
                heightDamping * Time.deltaTime
            );

            Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            Vector3 newPosition = activeCar.position;
            newPosition -= currentRotation * Vector3.forward * distance;
            newPosition.y = currentHeight;

            transform.position = newPosition;
            transform.LookAt(activeCar.position + Vector3.up * lookAtHeight);
        }

        public void StartGameplay()
        {
            isCinematic = false;
        }

        // 🔥 GỌI KHI WIN
        public void TriggerWin()
        {
            isCinematic = false;
            isWinCinematic = true;
        }
    }
}   