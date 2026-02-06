using UnityEngine;
using System.Collections;

public class VehicleBoost : MonoBehaviour
{
    [SerializeField] private GameObject boostEffect; // Kéo thả Particle System vào đây
    [SerializeField] private float boostDuration = 2f; // Thời gian hiện hiệu ứng

    private Coroutine boostRoutine;

    void Start()
    {
        // Đảm bảo lúc đầu hiệu ứng tắt
        if (boostEffect != null) boostEffect.SetActive(false);
    }

    // Hàm này sẽ được gọi khi xe chạm vào cục Boost
    public void ActivateBoost()
    {
        // Nếu đang trong quá trình boost mà ăn thêm cục nữa thì reset thời gian
        if (boostRoutine != null) StopCoroutine(boostRoutine);
        
        boostRoutine = StartCoroutine(BoostSequence());
    }

    IEnumerator BoostSequence()
    {
        boostEffect.SetActive(true);
        
        // Đợi trong x giây
        yield return new WaitForSeconds(boostDuration);
        
        boostEffect.SetActive(false);
        boostRoutine = null;
    }
}