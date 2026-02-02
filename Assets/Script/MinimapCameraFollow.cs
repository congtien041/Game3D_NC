using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        transform.position = new Vector3(
            player.position.x,
            transform.position.y,
            player.position.z
        );
    }
}
