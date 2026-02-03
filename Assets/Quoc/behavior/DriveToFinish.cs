using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class DriveToFinish : Action
{
    public SharedTransform enemy;
    public SharedTransform finish;
    public SharedFloat speed;
    public SharedFloat turnSpeed;

    private Rigidbody rb;

    public override void OnStart()
    {
        rb = enemy.Value.GetComponent<Rigidbody>();
    }

    public override TaskStatus OnUpdate()
    {
        Vector3 dir = (finish.Value.position - enemy.Value.position).normalized;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        enemy.Value.rotation = Quaternion.Slerp(
            enemy.Value.rotation,
            targetRot,
            turnSpeed.Value * Time.deltaTime
        );

        rb.linearVelocity = enemy.Value.forward * speed.Value;

        return TaskStatus.Running;
    }
}
