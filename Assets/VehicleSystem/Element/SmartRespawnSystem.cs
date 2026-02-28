using TMPro;
using UnityEngine;
using System.Collections; // Cần thiết cho Coroutine
using VehicleSystem.Core; // Để gọi TrackPath và CarController

namespace VehicleSystem.Element
{
    public class SmartRespawnSystem : MonoBehaviour
    {
        [Header("References")]
        public CarControllerVipro playerCarScript; 
        public TrackPath trackPath; 

        [Header("Settings - Rớt Map")]
        public float fallLimitY = -10f; 

        [Header("Settings - Giới hạn")]
        public float maxDistanceFromRoad = 15.0f; 
        public float allowedAngle = 110f;         
        public float timeToReset = 3.0f;          
        
        [Header("UI")]

        public GameObject wrongWayPanel;

        public TextMeshProUGUI timerText;

        // Biến nội bộ
        private Transform playerTransform;
        private Rigidbody carRb;
        private float warningTimer = 0f; 
        private bool isResetting = false;

        [System.Obsolete]
        private void Start()
        {
            if (playerCarScript == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p) playerCarScript = p.GetComponent<CarControllerVipro>();
            }

            if (playerCarScript != null)
            {
                playerTransform = playerCarScript.transform;
                carRb = playerCarScript.GetComponent<Rigidbody>();
            }
            if (trackPath == null) trackPath = FindObjectOfType<TrackPath>();
            if (wrongWayPanel) wrongWayPanel.SetActive(false);
        }

        private void Update()
        {
            if (playerCarScript == null || trackPath == null) return;
            if (isResetting) return; 

            if (playerTransform.position.y < fallLimitY)
            {
                StartCoroutine(RespawnRoutine());
                return;
            }
            CheckErrors();
        }

        void CheckErrors()
        {
            bool hasError = false;
            string message = "";

            float distToRoad = trackPath.GetDistanceFromRoad(playerTransform.position);
            
            if (distToRoad > maxDistanceFromRoad)
            {
                hasError = true;
                message = "OFF TRACK!"; 
            }
            else
            {
                Transform closestPoint = trackPath.GetClosestWaypoint(playerTransform.position);
                if (closestPoint != null)
                {
                    float angle = Vector3.Angle(playerTransform.forward, closestPoint.forward);
                    if (angle > allowedAngle)
                    {
                        hasError = true;
                        message = "WRONG WAY!"; 
                    }
                }
            }

            if (hasError)
            {
                warningTimer += Time.deltaTime;
                
                if (wrongWayPanel) 
                {
                    wrongWayPanel.SetActive(true);
                    if (timerText) timerText.text = $"{message}\nRESET IN: {timeToReset - warningTimer:F1}";
                }

                if (warningTimer >= timeToReset)
                {
                    StartCoroutine(RespawnRoutine());
                }
            }
            else
            {
                warningTimer = 0f;
                if (wrongWayPanel) wrongWayPanel.SetActive(false);
            }
        }

        IEnumerator RespawnRoutine()
        {
            isResetting = true;
            
            if (wrongWayPanel) wrongWayPanel.SetActive(false);
            warningTimer = 0f;

            Transform spawnPoint = trackPath.GetClosestWaypoint(playerTransform.position);

            if (spawnPoint != null)
            {
                playerTransform.position = spawnPoint.position + Vector3.up * 2f;
                
                playerTransform.rotation = spawnPoint.rotation; 
                
                if (carRb != null)
                {
                    carRb.linearVelocity = Vector3.zero;
                    carRb.angularVelocity = Vector3.zero;
                    carRb.Sleep(); 
                }
                
                if (playerCarScript != null)
                {
                    playerCarScript.SetInvincible(true);
                }
            }

            yield return new WaitForSeconds(0.5f);
            if (carRb != null) carRb.WakeUp();

            isResetting = false;
        }
    }
}