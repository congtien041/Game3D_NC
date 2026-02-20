using UnityEngine;
using VehicleSystem.Core;

namespace VehicleSystem.Core
{    
    public class CarControllerVipro : MonoBehaviour
    {
        [Header("Wheel Colliders")]
        public WheelCollider frontLeftCollider;
        public WheelCollider frontRightCollider;
        public WheelCollider rearLeftCollider;
        public WheelCollider rearRightCollider;

        [Header("Wheel Transforms")]
        public Transform frontLeftMesh;
        public Transform frontRightMesh;
        public Transform rearLeftMesh;
        public Transform rearRightMesh;

        [Header("Car Settings")]
        private float motorTorque = 1500f;   
        private float maxSteeringAngle = 30f;
        private float brakeForce = 3000f;    
        private float decelerationForce = 300f;
        [HideInInspector] public float maxSpeed = 120f;

        [Header("Stability & Recovery")]
        public Transform centerOfMass;   
        private float waitTimeToFlip = 3f;
        [Header("CountDown & Status")]
        [HideInInspector] public bool isEngineOn = false; 
        [HideInInspector] public bool isCountdown = false; 
        private bool isTargetable = true;  
        private Renderer[] allRenderers;

        private float currentMotorTorque;
        private float currentSteeringAngle;
        private float currentBrakeForce;
        private float flipTimer = 0f;     
        private Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (centerOfMass != null) rb.centerOfMass = centerOfMass.localPosition;
            allRenderers = GetComponentsInChildren<Renderer>();
        }

        [System.Obsolete]
        private void Update() => CheckAndResetFlip();

        private void FixedUpdate()
        {
            HandleInput();
            HandleMotor();
            HandleSteering();
            UpdateWheelMeshes();
        }

        private void HandleInput()
        {
            if (!isEngineOn) { currentMotorTorque = 0f; currentBrakeForce = brakeForce; return; }
            currentMotorTorque = motorTorque;
            float horizontalInput = Input.GetAxis("Horizontal");
            currentSteeringAngle = maxSteeringAngle * horizontalInput;
            if (Input.GetKey(KeyCode.Space)) { currentBrakeForce = brakeForce; currentMotorTorque = 0f; }
            else { currentBrakeForce = 0f; }
        }
        private void HandleMotor()
        {
            float speed = rb.linearVelocity.magnitude * 3.6f; // đổi m/s -> km/h
            Debug.Log(speed);
            if (speed < maxSpeed)
            {
                frontLeftCollider.motorTorque = currentMotorTorque;
                frontRightCollider.motorTorque = currentMotorTorque;
                rearLeftCollider.motorTorque = currentMotorTorque;
                rearRightCollider.motorTorque = currentMotorTorque;
            }
            else
            {
                frontLeftCollider.motorTorque = 0f;
                frontRightCollider.motorTorque = 0f;
                rearLeftCollider.motorTorque = 0f;
                rearRightCollider.motorTorque = 0f;
            }


            // frontLeftCollider.motorTorque = frontRightCollider.motorTorque = rearLeftCollider.motorTorque = rearRightCollider.motorTorque = currentMotorTorque;
            if (currentBrakeForce > 0) ApplyBrake(currentBrakeForce);
            else if (currentMotorTorque == 0) ApplyBrake(decelerationForce);
            else ApplyBrake(0f);
        }
        private void ApplyBrake(float force)
        {
            frontLeftCollider.brakeTorque = frontRightCollider.brakeTorque = rearLeftCollider.brakeTorque = rearRightCollider.brakeTorque = force;
        }
        private void HandleSteering()
        {
            frontLeftCollider.steerAngle = frontRightCollider.steerAngle = currentSteeringAngle;
        }
        private void UpdateWheelMeshes()
        {
            UpdateSingleWheel(frontLeftCollider, frontLeftMesh);
            UpdateSingleWheel(frontRightCollider, frontRightMesh);
            UpdateSingleWheel(rearLeftCollider, rearLeftMesh);
            UpdateSingleWheel(rearRightCollider, rearRightMesh);
        }
        private void UpdateSingleWheel(WheelCollider col, Transform mesh)
        {
            col.GetWorldPose(out Vector3 pos, out Quaternion rot);
            mesh.position = pos;
            mesh.rotation = rot;
        }
        
        [System.Obsolete]
        private void CheckAndResetFlip()
        {
            if (transform.up.y < 0.2f) {
                flipTimer += Time.deltaTime; 
                if (flipTimer >= waitTimeToFlip) {
                    transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
                    transform.position = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
                    rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; flipTimer = 0f;
                }
            } else flipTimer = 0f;
        }

        public void SetInvincible(bool state)
        {
            isTargetable = !state;
            
            if (!state) ToggleMeshes(true); 
            else StartCoroutine(BlinkEffect(true));
        }

        private System.Collections.IEnumerator BlinkEffect(bool active)
        {
            float timer = 0;
            while (timer < 3f)
            {
                ToggleMeshes(false); 
                yield return new WaitForSeconds(0.15f); 
                
                ToggleMeshes(true);  
                yield return new WaitForSeconds(0.15f); 
                
                timer += 0.3f;
            }
            ToggleMeshes(true);
        }

        private void ToggleMeshes(bool show)
        {
            if (allRenderers != null)
            {
                foreach (var renderer in allRenderers)
                {
                    if (renderer != null)
                        renderer.enabled = show; 
                }
            }
        }
    }
}