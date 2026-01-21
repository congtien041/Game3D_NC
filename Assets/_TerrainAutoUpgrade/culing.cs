using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ExampleClass : MonoBehaviour
{
    public float pro = 500f;
    void Start()
    {
        Camera cam = GetComponent<Camera>();

        float[] distances = new float[32];

        int propsLayer = LayerMask.NameToLayer("Props");
        if (propsLayer != -1)
        {
            distances[propsLayer] = pro; // Props xa hơn 15m sẽ không render
        }

        cam.layerCullDistances = distances;
        cam.layerCullSpherical = true; // nên bật
    }
}