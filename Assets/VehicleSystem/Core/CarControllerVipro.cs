using UnityEngine;
namespace VehicleSystem.Core
{    
    public class CarControllerVipro : MonoBehaviour
    {
        [Header("Wheel Colliders")]
        public WheelCollider frontLeftCollider;
        public WheelCollider frontRightCollider;
        public WheelCollider rearLeftCollider;
        public WheelCollider rearRightCollider;

        [Header("Wheel Transforms (Hình ảnh bánh xe)")]
        public Transform frontLeftMesh;
        public Transform frontRightMesh;
        public Transform rearLeftMesh;
        public Transform rearRightMesh;

        [Header("Car Settings")]
        private float motorTorque = 1500f;   
        private float maxSteeringAngle = 30f;
        private float brakeForce = 3000f;    
        private float decelerationForce = 300f; 

        [Header("Stability & Recovery (Chống lật & Khôi phục)")]
        public Transform centerOfMass;   
        private float waitTimeToFlip = 3f;
        private float currentMotorTorque;
        private float currentSteeringAngle;
        private float currentBrakeForce;
        private float flipTimer = 0f;     
        private Rigidbody rb; 
        [Header("CountDown")]
        [HideInInspector] public bool isEngineOn = false;  
        [Header("Status")]
        private bool isTargetable = true;  
        public GameObject Car;
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            Car = this.gameObject;
            if (centerOfMass != null)
            {
                rb.centerOfMass = centerOfMass.localPosition;
            }
        }

        [System.Obsolete]
        private void Update()
        {
            CheckAndResetFlip();
        }

        private void FixedUpdate()
        {
            HandleInput();
            HandleMotor();
            HandleSteering();
            UpdateWheelMeshes();
        }

        private void HandleInput()
        {

            if (!isEngineOn)
            {
                currentMotorTorque = 0f;
                currentBrakeForce = brakeForce; 
                return; 
            }
            currentMotorTorque = motorTorque;

            float horizontalInput = Input.GetAxis("Horizontal");
            currentSteeringAngle = maxSteeringAngle * horizontalInput;

            if (Input.GetKey(KeyCode.Space))
            {
                currentBrakeForce = brakeForce;
                currentMotorTorque = 0f; 
            }
            else
            {
                currentBrakeForce = 0f;
            }
        }

        private void HandleMotor()
        {
            frontLeftCollider.motorTorque = currentMotorTorque;
            frontRightCollider.motorTorque = currentMotorTorque;
            rearLeftCollider.motorTorque = currentMotorTorque;
            rearRightCollider.motorTorque = currentMotorTorque;

            if (currentBrakeForce > 0)
            {
                ApplyBrake(currentBrakeForce);
            }
            else if (currentMotorTorque == 0)
            {
                ApplyBrake(decelerationForce);
            }
            else
            {
                ApplyBrake(0f);
            }
        }
        
        private void ApplyBrake(float force)
        {
            frontLeftCollider.brakeTorque = force;
            frontRightCollider.brakeTorque = force;
            rearLeftCollider.brakeTorque = force;
            rearRightCollider.brakeTorque = force;
        }

        private void HandleSteering()
        {
            frontLeftCollider.steerAngle = currentSteeringAngle;
            frontRightCollider.steerAngle = currentSteeringAngle;
        }

        private void UpdateWheelMeshes()
        {
            UpdateSingleWheel(frontLeftCollider, frontLeftMesh);
            UpdateSingleWheel(frontRightCollider, frontRightMesh);
            UpdateSingleWheel(rearLeftCollider, rearLeftMesh);
            UpdateSingleWheel(rearRightCollider, rearRightMesh);
        }

        private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelMesh)
        {
            Vector3 pos;
            Quaternion rot;
            
            wheelCollider.GetWorldPose(out pos, out rot);

            wheelMesh.position = pos;
            wheelMesh.rotation = rot;
        }

        [System.Obsolete]
        private void CheckAndResetFlip()
        {
            if (transform.up.y < 0.2f)
            {
                flipTimer += Time.deltaTime; 
                
                if (flipTimer >= waitTimeToFlip)
                {
                    transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
                    
                    transform.position = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
                    
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    
                    flipTimer = 0f;
                }
            }
            else
            {
                flipTimer = 0f;
            }
        }

        public void SetInvincible(bool state)
        {
            isTargetable = !state;
            
            StartCoroutine(BlinkEffect(state));
        }

        private System.Collections.IEnumerator BlinkEffect(bool active)
        {
            if (!active) 
            {
                ToggleMeshes(true);
                yield break;
            }

            float timer = 0;
            while (timer < 3f)
            {
                ToggleMeshes(false);
                yield return new WaitForSeconds(0.1f);
                ToggleMeshes(true);
                yield return new WaitForSeconds(0.1f);
                timer += 0.2f;
            }
            ToggleMeshes(true);
        }

        private void ToggleMeshes(bool show)
        {
            if (Car) Car.gameObject.SetActive(show);
        }
    }
}   