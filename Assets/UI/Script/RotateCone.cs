using UnityEngine;
using System.Collections;

public class RotateCone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float autoSpeed = 20f;
    [SerializeField] private float dragSpeed = 5f;
    [SerializeField] private float upgradeSpinDuration = 0.8f; 

    [Header("Visual Effects")]
    // Kéo GameObject chứa VFX (đang bị tắt) vào đây
    [SerializeField] private GameObject upgradeVFXObject; 

    private bool isDragging = false;
    private bool isUpgrading = false;

    void Awake()
    {
        // Đảm bảo lúc đầu nó tắt
        if (upgradeVFXObject != null) 
        {
            upgradeVFXObject.SetActive(false);
        }
        StartCoroutine(AutoRotate());
    }

    IEnumerator AutoRotate()
    {
        while (true)
        {
            if (!isDragging && !isUpgrading)
            {
                transform.Rotate(Vector3.up * autoSpeed * Time.deltaTime);
            }
            yield return null;
        }
    }

    public void OnUpgradeButtonClicked()
    {
        if (!isUpgrading)
        {
            StartCoroutine(UpgradeSpinRoutine());
        }
    }

    IEnumerator UpgradeSpinRoutine()
    {
        isUpgrading = true;
        
        // 1. SET ACTIVE = TRUE
        if (upgradeVFXObject != null) 
        {
            upgradeVFXObject.SetActive(true);
        }

        float elapsed = 0f;
        float totalRotation = 720f; // Quay 2 vòng

        while (elapsed < upgradeSpinDuration)
        {
            // Quay đều theo thời gian
            float step = (totalRotation / upgradeSpinDuration) * Time.deltaTime;
            transform.Rotate(Vector3.up * step);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. SET ACTIVE = FALSE
        if (upgradeVFXObject != null) 
        {
            upgradeVFXObject.SetActive(false);
        }

        isUpgrading = false;
    }

    #region Mouse Input
    void OnMouseDown() => isDragging = true;
    void OnMouseUp() => isDragging = false;

    void Update()
    {
        if (isDragging && !isUpgrading)
        {
            float mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(Vector3.up * -mouseX * dragSpeed);
        }
    }
    #endregion
}