using UnityEngine;
using System.Collections.Generic;

namespace VehicleSystem.Core
{
    public class TrackPath : MonoBehaviour
    {
        public Color pathColor = Color.yellow;
        // Danh sách các điểm mốc chạy dọc đường đua
        public List<Transform> waypoints = new List<Transform>();

        // Hàm này giúp vẽ đường nối các điểm trong màn hình Scene để dễ chỉnh sửa
        private void OnDrawGizmos()
        {
            Gizmos.color = pathColor;
            
            // Tự động lấy các con (child) làm waypoint nếu danh sách trống
            if (waypoints.Count == 0)
            {
                waypoints = new List<Transform>();
                foreach (Transform child in transform)
                {
                    waypoints.Add(child);
                }
            }

            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null) continue;

                Vector3 current = waypoints[i].position;
                
                // Vẽ cầu (điểm mốc)
                Gizmos.DrawWireSphere(current, 1f);

                // Vẽ đường nối tới điểm tiếp theo
                if (i < waypoints.Count - 1 && waypoints[i+1] != null)
                {
                    Gizmos.DrawLine(current, waypoints[i+1].position);
                }
            }
        }

        // HÀM QUAN TRỌNG: Tìm điểm mốc gần chiếc xe nhất
        public Transform GetClosestWaypoint(Vector3 carPosition)
        {
            Transform closestPoint = null;
            float minDistance = Mathf.Infinity;

            foreach (Transform point in waypoints)
            {
                if (point == null) continue;

                float dist = Vector3.Distance(carPosition, point.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestPoint = point;
                }
            }
            return closestPoint;
        }
    }
}