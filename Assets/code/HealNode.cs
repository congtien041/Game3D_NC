using UnityEngine; // THÊM DÒNG NÀY

public class HealNode : Node {
    GuardAI ai;
    bool isHealing = false;

    public HealNode(GuardAI ai) { this.ai = ai; }

    public override NodeState Evaluate() {
        if (ai.health < 100f || (isHealing && ai.health < 500f)) {
            isHealing = true;
            
            // Kiểm tra xem đã kéo HealStation vào Inspector chưa
            if (ai.healStation == null) {
                Debug.LogError("Chưa kéo HealStation vào GuardAI!");
                return NodeState.FAILURE;
            }

            float distToStation = Vector3.Distance(ai.transform.position, ai.healStation.position);

            if (distToStation > 2f) {
                ai.agent.isStopped = false;
                ai.agent.speed = 20f; // Cho chạy nhanh hẳn lên để thoát thân
                ai.agent.SetDestination(ai.healStation.position);
                return NodeState.RUNNING;
            } else {
                ai.agent.isStopped = true;
                ai.health += Time.deltaTime * 100f; 
                return NodeState.RUNNING;
            }
        }

        isHealing = false;
        ai.agent.isStopped = false;
        return NodeState.FAILURE; 
    }
}