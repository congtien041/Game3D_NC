using UnityEngine;
using System.Collections;

public class NitroItem : MonoBehaviour
{
    public float boostDuration = 5f; 
    public GameObject itemModel;   
    public float respawnTime = 7f; 
    private bool isAvailable = true;

    private void OnTriggerEnter(Collider other)
    {
        CarController car = other.GetComponentInParent<CarController>();
        if (car != null && isAvailable)
        {
            StartCoroutine(ActivateNitroBoost(car));
        }
    }

    IEnumerator ActivateNitroBoost(CarController car)
    {
        isAvailable = false;
        if (itemModel) itemModel.SetActive(false); 

        // Bật trạng thái Boost cưỡng bức từ Item
        car.isItemBoostActive = true; 
        if (GlobalAudio.Instance != null) GlobalAudio.Instance.PlayNitroSound(true);

        yield return new WaitForSeconds(boostDuration);

        // Tắt trạng thái Boost
        car.isItemBoostActive = false;
        if (GlobalAudio.Instance != null) GlobalAudio.Instance.PlayNitroSound(false);

        yield return new WaitForSeconds(respawnTime);
        if (itemModel) itemModel.SetActive(true);
        isAvailable = true;
    }
}