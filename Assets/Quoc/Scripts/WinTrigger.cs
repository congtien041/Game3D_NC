using UnityEngine;
using System.Collections;
using VehicleSystem.Core;

public class WinTrigger : MonoBehaviour
{
    public CameraCinematic cameraScript;

    public GameObject winUI;
    public GameObject nextUI;

    private int triggerCount = 0;   // 👈 đếm số lần đi qua
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        triggerCount++;

        // Chỉ trigger ở lần thứ 2
        if (triggerCount >= 2 && !hasTriggered)
        {
            hasTriggered = true;

            cameraScript.TriggerWin();
            StartCoroutine(ShowWinSequence());
        }
    }

    IEnumerator ShowWinSequence()
    {
        winUI.SetActive(true);

        yield return new WaitForSeconds(5f);

        winUI.SetActive(false);
        nextUI.SetActive(true);
    }
}