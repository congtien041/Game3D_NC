using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VehicleSystem.Core;

public class WinTrigger : MonoBehaviour
{
    public CameraCinematic cameraScript;

    public List<GameObject> uiToHide;   // 👈 List UI cần ẩn
    public GameObject winUI;
    public GameObject nextUI;

    private int triggerCount = 0;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        triggerCount++;

        if (triggerCount >= 2 && !hasTriggered)
        {
            hasTriggered = true;

            // 👇 Ẩn toàn bộ UI trong list
            foreach (GameObject ui in uiToHide)
            {
                if (ui != null)
                    ui.SetActive(false);
            }

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