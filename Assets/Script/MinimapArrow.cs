using UnityEngine;

public class MinimapArrow : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        Vector3 pos = player.position;
        pos.y = 20f; // cao ngang minimap camera
        transform.position = pos;

        transform.rotation = Quaternion.Euler(90, player.eulerAngles.y, 0);
    }
}
