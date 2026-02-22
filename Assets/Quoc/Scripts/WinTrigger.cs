using UnityEngine;
using VehicleSystem.Core;

public class WinTrigger : MonoBehaviour
{
    public VehicleSystem.Core.CameraCinematic cameraScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraScript.TriggerWin();
        }
    }
}