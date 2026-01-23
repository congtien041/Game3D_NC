using System.Collections.Generic;
public class Selector : Node {
    private List<Node> nodes;
    public Selector(List<Node> nodes) { this.nodes = nodes; }
    public override NodeState Evaluate() {
        foreach (var node in nodes) {
            switch (node.Evaluate()) {
                case NodeState.SUCCESS: return NodeState.SUCCESS;
                case NodeState.RUNNING: return NodeState.RUNNING;
            }
        }
        return NodeState.FAILURE;
    }
}