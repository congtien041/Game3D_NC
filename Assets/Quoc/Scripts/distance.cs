using UnityEngine;

public class CameraCullSetup : MonoBehaviour
{
    void Start()
    {
        Camera cam = Camera.main;

        float[] distances = new float[32];

        distances[LayerMask.NameToLayer("Enemy")] = 30f;
        distances[LayerMask.NameToLayer("Props")] = 5f;
        distances[LayerMask.NameToLayer("FX")] = 15f;
        distances[LayerMask.NameToLayer("Environment")] = 250f;

        cam.layerCullDistances = distances;
        cam.layerCullSpherical = true; // ⭐ rất nên bật
    }
}
