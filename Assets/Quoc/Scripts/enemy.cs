using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public Transform[] waypoints;   // Danh sách node
    public float speed = 3f;

    private int currentWaypointIndex = 0;

    void Update()
    {
        if (currentWaypointIndex >= waypoints.Length)
            return;

        Transform target = waypoints[currentWaypointIndex];

        // Di chuyển tới node
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // Kiểm tra nếu đã tới gần node
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentWaypointIndex++;
        }
    }
}