using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class DriveToNextWaypoint : Action
{
    public SharedTransform enemy;
    public SharedTransformList waypoints;
    public SharedInt currentIndex;
    public SharedFloat speed;
    public SharedFloat turnSpeed;
    public SharedFloat reachDistance;

    private Rigidbody rb;

    public override void OnStart()
    {
        rb = enemy.Value.GetComponent<Rigidbody>();
    }

    public override TaskStatus OnUpdate()
    {
        Transform target = waypoints.Value[currentIndex.Value];

        Vector3 dir = (target.position - enemy.Value.position).normalized;

        enemy.Value.rotation = Quaternion.Slerp(
            enemy.Value.rotation,
            Quaternion.LookRotation(dir),
            turnSpeed.Value * Time.deltaTime
        );

        rb.linearVelocity = enemy.Value.forward * speed.Value;

        if (Vector3.Distance(enemy.Value.position, target.position) < reachDistance.Value)
        {
            currentIndex.Value = (currentIndex.Value + 1) % waypoints.Value.Count;
        }

        return TaskStatus.Running;
    }
}
