using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Cài đặt tốc độ")]
    public float forwardSpeed = 10f; // Tốc độ chạy thẳng
    public float strafeSpeed = 5f;   // Tốc độ di chuyển trái/phải
    public float gravity = 20f;      // Trọng lực để nhân vật bám đất

    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        moveDirection.z = forwardSpeed;

        float xInput = Input.GetAxis("Horizontal"); 
        moveDirection.x = xInput * strafeSpeed;

        if (controller.isGrounded)
        {
            moveDirection.y = -1f; 
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime; 
        }

        controller.Move(moveDirection * Time.deltaTime);
    }
}