using UnityEngine;
using System.Collections;
using VehicleSystem.Core;
using VehicleSystem.UI; // Gọi namespace UI

namespace VehicleSystem.Managers
{
    public class RaceManager : MonoBehaviour
    {
        private CarControllerVipro playerCar; 
        private Rigidbody carRb;
        public TrackPath trackPath; 
        private float maxDistanceFromTrack = 15.0f; 
        private float allowedAngle = 110f; 
        private float wrongWayTimeLimit = 3.0f;
        private float stuckVelocityThreshold = 1.0f; 
        private float stuckTimeLimit = 2.0f;         
        private float invincibilityDuration = 3.0f; 
        private float wrongWayTimer = 0f;
        private float stuckTimer = 0f;
        private bool isResetting = false;

        private void Update()
        {
            if (playerCar == null)
            {
                FindPlayerCar();
                return;
            }

            if (trackPath == null || isResetting) return;

            Transform closestPoint = trackPath.GetClosestWaypoint(playerCar.transform.position);
            if (closestPoint == null) return;
            float distanceToTrack = Vector3.Distance(playerCar.transform.position, closestPoint.position);
            
            if (distanceToTrack > maxDistanceFromTrack)
            {
                StartCoroutine(RespawnProcess(closestPoint));
                return;
            }

            float angle = Vector3.Angle(playerCar.transform.forward, closestPoint.forward);
            if (angle > allowedAngle)
            {
                wrongWayTimer += Time.deltaTime;
                
                RaceUIManager.Instance.ShowWarning($"WRONG WAY! {wrongWayTimeLimit - wrongWayTimer:F1}");

                if (wrongWayTimer >= wrongWayTimeLimit)
                {
                    StartCoroutine(RespawnProcess(closestPoint));
                    return;
                }
            }
            else
            {
                wrongWayTimer = 0f;
                if (stuckTimer == 0) RaceUIManager.Instance.HideWarning();
            }

            bool isPressingGas = Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;
            float currentSpeed = carRb.linearVelocity.magnitude; 

            if (isPressingGas && currentSpeed < stuckVelocityThreshold)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > 1.0f) 
                {
                    RaceUIManager.Instance.ShowWarning($"STUCK! RESET IN {stuckTimeLimit - stuckTimer:F1}");
                }

                if (stuckTimer >= stuckTimeLimit)
                {
                    StartCoroutine(RespawnProcess(closestPoint));
                }
            }
            else
            {
                stuckTimer = 0f;
                if (wrongWayTimer == 0) RaceUIManager.Instance.HideWarning();
            }
        }

        void FindPlayerCar()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                playerCar = p.GetComponent<CarControllerVipro>();
                carRb = p.GetComponent<Rigidbody>();
            }
        }

        IEnumerator RespawnProcess(Transform spawnPoint)
        {
            isResetting = true;
            RaceUIManager.Instance.HideWarning(); 
            Vector3 safePosition = spawnPoint.position + Vector3.up * 2.0f;
            playerCar.transform.position = safePosition;
            playerCar.transform.rotation = spawnPoint.rotation;
            carRb.linearVelocity = Vector3.zero;
            carRb.angularVelocity = Vector3.zero;
            playerCar.SetInvincible(true); 
            wrongWayTimer = 0f;
            stuckTimer = 0f;
            isResetting = false;
            yield return new WaitForSeconds(invincibilityDuration);
            playerCar.SetInvincible(false);
        }
    }
}