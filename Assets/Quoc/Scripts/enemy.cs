using UnityEngine;
using System.Collections;

public class EnemyMove : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;
    public float startDelay = 2f;   // Thời gian chờ

    private int currentWaypointIndex = 0;
    private bool canMove = false;

    void Start()
    {
        StartCoroutine(StartAfterDelay());
    }

    IEnumerator StartAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        canMove = true;
    }

    void Update()
    {
        if (!canMove) return;
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
                currentWaypointIndex = 0;
        }
    }
}