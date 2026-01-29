using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class GuardAI : MonoBehaviour {
    public float health = 500f;
    public Transform player;
    public NavMeshAgent agent;
    public Transform[] waypoints;
    public Transform healStation;
    private Node rootNode;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        // Khởi tạo cây hành vi
        rootNode = new Selector(new List<Node> {
            new HealNode(this), // Nó sẽ tự tìm đến file HealNode.cs
            new ChaseNode(this),
            new PatrolNode(this)
        });
    }

    void Update() {
        rootNode.Evaluate();
        // Giới hạn máu không vượt quá 100
        if (health > 500f) health = 500f;
        if (Input.GetKeyDown(KeyCode.K)) health -= 50f;
    }
}

// XÓA PHẦN CLASS HEALNODE CŨ Ở ĐÂY ĐI
// CHỈ GIỮ LẠI CHASE VÀ PATROL NẾU BẠN MUỐN ĐỂ CHUNG

public class ChaseNode : Node {
    GuardAI ai;
    public ChaseNode(GuardAI ai) { this.ai = ai; }
    public override NodeState Evaluate() {
        float dist = Vector3.Distance(ai.transform.position, ai.player.position);
        if (dist < 100f) {
            ai.agent.speed = 20.0f; 
            ai.agent.SetDestination(ai.player.position);
            return NodeState.RUNNING;
        }
        return NodeState.FAILURE;
    }
}

public class PatrolNode : Node {
    GuardAI ai; int index = 0;
    public PatrolNode(GuardAI ai) { this.ai = ai; }
    public override NodeState Evaluate() {
        if (ai.waypoints.Length == 0) return NodeState.FAILURE;
        ai.agent.speed = 7.0f;
        if (ai.agent.remainingDistance < 0.5f) {
            index = (index + 1) % ai.waypoints.Length;
            ai.agent.SetDestination(ai.waypoints[index].position);
        }
        return NodeState.RUNNING;
    }
}