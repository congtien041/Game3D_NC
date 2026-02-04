using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class ChasePlayer : Action
{
    public SharedTransform enemy;
    public SharedTransform player;
    public SharedFloat moveSpeed;
    public SharedFloat turnSpeed;

    private Rigidbody rb;

    public override void OnStart()
    {
        rb = enemy.Value.GetComponent<Rigidbody>();
    }

    public override TaskStatus OnUpdate()
    {
        Vector3 dir = (player.Value.position - enemy.Value.position).normalized;

        // Lái về phía player
        Quaternion targetRot = Quaternion.LookRotation(dir);
        enemy.Value.rotation = Quaternion.Slerp(
            enemy.Value.rotation,
            targetRot,
            turnSpeed.Value * Time.deltaTime
        );

        // Tiến lên
        rb.linearVelocity = enemy.Value.forward * moveSpeed.Value;

        return TaskStatus.Running;
    }
}
