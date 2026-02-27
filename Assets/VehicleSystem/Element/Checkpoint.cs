using UnityEngine;

namespace VehicleSystem.Element
{
    public class Checkpoint : MonoBehaviour
    {
        public int index;
        public bool isFinishLine;
        public bool isTimeBonus;
        public float timeBonusAmount = 5f; 

        private void OnDrawGizmos()
        {
            Gizmos.color = isFinishLine ? new Color(0, 1, 0, 0.5f) : new Color(1, 1, 0, 0.5f);
            
            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
        }
    }
}