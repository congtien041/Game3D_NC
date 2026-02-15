using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VehicleSystem.Core;
using TMPro; // Để gọi TrackPath và CarController

namespace VehicleSystem.Managers
{
    public class RaceManager : MonoBehaviour
    {
        [Header("Target")]
        public CarControllerVipro playerCar;
        public Rigidbody carRb;
        public TrackPath trackPath;          

        [Header("Điều kiện 1: Đi quá xa (Off-track)")]
        public float maxDistanceFromTrack = 15.0f; 

        [Header("Điều kiện 2: Chạy ngược chiều (Wrong Way)")]
        public float allowedAngle = 110f; 
        public float wrongWayTimeLimit = 3.0f;

        [Header("Điều kiện 3: Bị kẹt (Stuck)")]
        public float stuckVelocityThreshold = 1.0f; 
        public float stuckTimeLimit = 2.0f;         

        [Header("Ghost Mode")]
        public float invincibilityDuration = 3.0f; 

        [Header("UI")]
        public GameObject warningPanel;
        public TextMeshProUGUI warningText;
        private float wrongWayTimer = 0f;
        private float stuckTimer = 0f;
        private bool isResetting = false;

        private void Update()
        {
            if (playerCar == null || trackPath == null || isResetting) return;

            Transform closestPoint = trackPath.GetClosestWaypoint(playerCar.transform.position);
            if (closestPoint == null) return;

            float distanceToTrack = Vector3.Distance(playerCar.transform.position, closestPoint.position);
            if (distanceToTrack > maxDistanceFromTrack)
            {
                StartCoroutine(RespawnProcess(closestPoint));
                return;
            }

            // 2. CHECK NGƯỢC CHIỀU
            float angle = Vector3.Angle(playerCar.transform.forward, closestPoint.forward);
            if (angle > allowedAngle)
            {
                wrongWayTimer += Time.deltaTime;
                ShowWarning($"WRONG WAY! {wrongWayTimeLimit - wrongWayTimer:F1}");

                if (wrongWayTimer >= wrongWayTimeLimit)
                {
                    StartCoroutine(RespawnProcess(closestPoint));
                    return;
                }
            }
            else
            {
                wrongWayTimer = 0f;
                HideWarning();
            }

            bool isPressingGas = Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;
            float currentSpeed = carRb.linearVelocity.magnitude;

            if (isPressingGas && currentSpeed < stuckVelocityThreshold)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > 1.0f) 
                {
                    ShowWarning($"STUCK! RESET IN {stuckTimeLimit - stuckTimer:F1}");
                }

                if (stuckTimer >= stuckTimeLimit)
                {
                    StartCoroutine(RespawnProcess(closestPoint));
                }
            }
            else
            {
                stuckTimer = 0f;
                if (wrongWayTimer == 0) HideWarning();
            }
        }

        IEnumerator RespawnProcess(Transform spawnPoint)
        {
            isResetting = true;
            HideWarning();
            Vector3 safePosition = spawnPoint.position + Vector3.up * 2.0f;
            
            playerCar.transform.position = safePosition;
            playerCar.transform.rotation = spawnPoint.rotation;

            carRb.linearVelocity = Vector3.zero;
            carRb.angularVelocity = Vector3.zero;

            playerCar.SetInvincible(true); 
            Debug.Log("Xe đang ở trạng thái Bất tử!");

            wrongWayTimer = 0f;
            stuckTimer = 0f;
            isResetting = false;

            yield return new WaitForSeconds(invincibilityDuration);
            
            playerCar.SetInvincible(false);
            Debug.Log("Xe đã hết Bất tử, có thể bị tấn công.");
        }

        void ShowWarning(string message)
        {
            if (warningPanel) warningPanel.SetActive(true);
            if (warningText) warningText.text = message;
        }

        void HideWarning()
        {
            if (warningPanel) warningPanel.SetActive(false);
        }
    }
}