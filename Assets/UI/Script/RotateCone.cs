using UnityEngine;
using System.Collections;

public class RotateCone : MonoBehaviour
{
    [SerializeField] private float autoSpeed = 20f;
    [SerializeField] private float dragSpeed = 5f;

    private bool isDragging = false;
    private Coroutine rotateRoutine;

    void Awake()
    {
        rotateRoutine = StartCoroutine(AutoRotate());
    }

    IEnumerator AutoRotate()
    {
        while (true)
        {
            if (!isDragging)
            {
                transform.Rotate(Vector3.up * autoSpeed * Time.deltaTime);
            }
            yield return null;
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void Update()
    {
        if (isDragging)
        {
            float mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(Vector3.up * -mouseX * dragSpeed);
        }
    }
}
