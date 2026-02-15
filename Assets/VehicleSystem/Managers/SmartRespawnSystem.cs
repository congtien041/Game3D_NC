using TMPro;
using UnityEngine;
using UnityEngine.UI; 
using VehicleSystem.Core;

namespace VehicleSystem.Managers
{
    public class SmartRespawnSystem : MonoBehaviour
    {
        [Header("References")]
        public Transform playerCar;
        public Rigidbody carRb;
        public TrackPath trackPath; // Kéo cái đường đua vào đây

        [Header("Settings - Rớt Map")]
        public float fallLimitY = -10f; // Nếu Y thấp hơn số này -> Reset

        [Header("Settings - Ngược Chiều")]
        public float allowedAngle = 110f; 
        public float timeToReset = 3.0f;
        
        [Header("UI")]
        public GameObject wrongWayPanel;
        public TextMeshProUGUI timerText;

        private float wrongWayTimer = 0f;
        private bool isResetting = false;

        private void Start()
        {
            if (playerCar == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p)
                {
                    playerCar = p.transform;
                    carRb = p.GetComponent<Rigidbody>();
                }
            }
            if (wrongWayPanel) wrongWayPanel.SetActive(false);
        }

        [System.Obsolete]
        private void Update()
        {
            if (playerCar == null || trackPath == null) return;
            if (isResetting) return;

            // 1. KIỂM TRA RỚT MAP (Độ cao Y)
            if (playerCar.position.y < fallLimitY)
            {
                RespawnCar();
                return;
            }

            // 2. KIỂM TRA NGƯỢC CHIỀU & LỆCH ĐƯỜNG
            CheckDirectionAndDistance();
        }

        [System.Obsolete]
        void CheckDirectionAndDistance()
        {
            // Tìm điểm mốc gần nhất xe đang đứng
            Transform closestPoint = trackPath.GetClosestWaypoint(playerCar.position);

            if (closestPoint != null)
            {
                // Tính góc lệch giữa đầu xe và hướng của điểm mốc
                // (Điểm mốc luôn xoay theo hướng đường đi)
                float angle = Vector3.Angle(playerCar.forward, closestPoint.forward);

                // Nếu góc lệch quá lớn (đang quay đầu)
                if (angle > allowedAngle)
                {
                    wrongWayTimer += Time.deltaTime;
                    if (wrongWayPanel) 
                    {
                        wrongWayPanel.SetActive(true);
                        if (timerText) timerText.text = "RESET: " + (timeToReset - wrongWayTimer).ToString("F1");
                    }

                    if (wrongWayTimer >= timeToReset)
                    {
                        RespawnCar();
                    }
                }
                else
                {
                    // Đang đi đúng hướng
                    wrongWayTimer = 0f;
                    if (wrongWayPanel) wrongWayPanel.SetActive(false);
                }
            }
        }

        [System.Obsolete]
        public void RespawnCar()
        {
            isResetting = true;
            
            // Tìm điểm gần nhất
            Transform spawnPoint = trackPath.GetClosestWaypoint(playerCar.position);

            if (spawnPoint != null)
            {
                // Dịch chuyển xe về đó
                // Cộng thêm Vector3.up * 2f để thả xe từ trên cao xuống chút xíu cho đỡ kẹt đất
                playerCar.position = spawnPoint.position + Vector3.up * 2f;
                
                // QUAN TRỌNG: Xoay đầu xe theo hướng của đường đua
                playerCar.rotation = spawnPoint.rotation; 
                
                // Xóa quán tính (để xe không bị trôi tiếp)
                if (carRb != null)
                {
                    carRb.velocity = Vector3.zero;
                    carRb.angularVelocity = Vector3.zero;
                }
                
                Debug.Log("Đã hồi sinh tại mốc: " + spawnPoint.name);
            }

            // Reset UI
            wrongWayTimer = 0f;
            if (wrongWayPanel) wrongWayPanel.SetActive(false);
            
            isResetting = false;
        }
    }
}