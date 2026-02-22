using UnityEngine;
using System.Collections;
using VehicleSystem.Core;

public class WinTrigger : MonoBehaviour
{
    public CameraCinematic cameraScript;

    public GameObject winUI;
    public GameObject nextUI;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            cameraScript.TriggerWin();

            StartCoroutine(ShowWinSequence());
        }
    }

    IEnumerator ShowWinSequence()
    {
        // Bật UI Win
        winUI.SetActive(true);

        // Chờ 5 giây
        yield return new WaitForSeconds(5f);

        // Tắt Win UI
        winUI.SetActive(false);

        // Bật UI tiếp theo
        nextUI.SetActive(true);
    }
}