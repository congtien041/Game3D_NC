using UnityEngine;

public class MinimapArrowFollow : MonoBehaviour
{
    public Transform player;
    public float height = 80f;

    void LateUpdate()
    {
        Vector3 pos = player.position;
        transform.position = new Vector3(pos.x, height, pos.z);
        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}
