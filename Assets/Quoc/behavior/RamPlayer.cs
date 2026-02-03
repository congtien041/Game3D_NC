using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class RamPlayer : Action
{
    public SharedTransform enemy;
    public SharedTransform player;
    public SharedFloat ramSpeed;
    public SharedFloat ramDistance;

    private Rigidbody rb;

    public override void OnStart()
    {
        rb = enemy.Value.GetComponent<Rigidbody>();
    }

    public override TaskStatus OnUpdate()
    {
        float dist = Vector3.Distance(enemy.Value.position, player.Value.position);

        if (dist > ramDistance.Value)
            return TaskStatus.Failure;

        Vector3 dir = (player.Value.position - enemy.Value.position).normalized;

        enemy.Value.rotation = Quaternion.LookRotation(dir);
        rb.linearVelocity = enemy.Value.forward * ramSpeed.Value;

        return TaskStatus.Running;
    }
}
