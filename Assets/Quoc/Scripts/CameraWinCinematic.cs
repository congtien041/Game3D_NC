using UnityEngine;

namespace VehicleSystem.Core
{
    public class CameraWinCinematic : MonoBehaviour
    {
        [Header("Target")]
        public Transform targetCar;

        [Header("Win Camera Settings")]
        public Vector3 winOffset = new Vector3(-4f, 2f, 6f);
        public float moveSpeed = 3f;
        public float rotationSpeed = 20f;
        public float winFOV = 50f;

        private bool isPlaying = false;
        private Camera cam;

        private void Start()
        {
            cam = GetComponent<Camera>();

            if (targetCar == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player) targetCar = player.transform;
            }
        }

        private void LateUpdate()
        {
            if (!isPlaying || targetCar == null) return;

            // Di chuyển tới vị trí cinematic
            Vector3 desiredPosition = targetCar.TransformPoint(winOffset);

            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                Time.deltaTime * moveSpeed
            );

            // Nhìn vào xe
            transform.LookAt(targetCar.position + Vector3.up * 0.8f);

            // Zoom nhẹ
            cam.fieldOfView = Mathf.Lerp(
                cam.fieldOfView,
                winFOV,
                Time.deltaTime * 2f
            );

            // Xoay vòng quanh xe
            transform.RotateAround(
                targetCar.position,
                Vector3.up,
                rotationSpeed * Time.deltaTime
            );
        }

        public void PlayWinCinematic()
{
    Debug.Log("WIN CINEMATIC STARTED");
    isPlaying = true;
}
    }
}