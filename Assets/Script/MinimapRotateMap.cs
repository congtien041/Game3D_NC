using UnityEngine;

public class MinimapRotateMap : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        float playerY = player.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0, -playerY, 0);
    }
}
