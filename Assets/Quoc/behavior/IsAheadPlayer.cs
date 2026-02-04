using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class IsAheadPlayer : Conditional
{
    public SharedTransform enemy;
    public SharedTransform player;

    public override TaskStatus OnUpdate()
    {
        if (enemy.Value.position.z > player.Value.position.z)
            return TaskStatus.Success;

        return TaskStatus.Failure;
    }
}
