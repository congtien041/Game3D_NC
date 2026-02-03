using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class BoostSpeed : Action
{
    public SharedTransform enemy;
    public SharedFloat boostSpeed;

    private Rigidbody rb;

    public override void OnStart()
    {
        rb = enemy.Value.GetComponent<Rigidbody>();
    }

    public override TaskStatus OnUpdate()
    {
        rb.linearVelocity = enemy.Value.forward * boostSpeed.Value;
        return TaskStatus.Running;
    }
}
