using System.Collections.Generic;
public class Sequence : Node {
    private List<Node> nodes;
    public Sequence(List<Node> nodes) { this.nodes = nodes; }
    public override NodeState Evaluate() {
        bool isRunning = false;
        foreach (var node in nodes) {
            switch (node.Evaluate()) {
                case NodeState.FAILURE: return NodeState.FAILURE;
                case NodeState.RUNNING: isRunning = true; break;
            }
        }
        return isRunning ? NodeState.RUNNING : NodeState.SUCCESS;
    }
}