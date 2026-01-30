using System;
using System.Collections;
using UnityEngine;

public class CarAudioSetter : MonoBehaviour
{
    public Rigidbody rb;

    public KeyCode KeyCode;

    public int StartIndex;

    private HDCarController rootScript;

    void Start()
    {
        rootScript = rb.transform.root.GetComponent<HDCarController>();
        StartIndex = 0;
    }

    void Update()
    {
        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(kcode) && kcode == KeyCode)
            {
                StartIndex = (rootScript.SelectedCamPositionIndex + 1) % rootScript.CamSetup.SoundsAtCamPositions.Length;
                rootScript.SelectedCamPositionIndex = StartIndex;
            }
        }
    }

}
