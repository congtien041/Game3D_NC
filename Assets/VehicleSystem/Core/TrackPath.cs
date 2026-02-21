using UnityEngine;
using System.Collections.Generic;

namespace VehicleSystem.Core
{
    public class TrackPath : MonoBehaviour
    {
        public Color pathColor = Color.yellow;
        public List<Transform> waypoints = new List<Transform>();

        public int GetClosestWaypointIndex(Vector3 carPos)
        {
            int closestIndex = -1;
            float minDistance = Mathf.Infinity;

            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null) continue;
                float dist = Vector3.Distance(carPos, waypoints[i].position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestIndex = i;
                }
            }
            return closestIndex;
        }

        public float GetDistanceFromRoad(Vector3 carPos)
        {
            int i = GetClosestWaypointIndex(carPos);
            if (i == -1) return Mathf.Infinity;
            float distA = Mathf.Infinity;
            float distB = Mathf.Infinity;

            if (i > 0) 
                distA = DistancePointToSegment(carPos, waypoints[i-1].position, waypoints[i].position);
            
            if (i < waypoints.Count - 1)
                distB = DistancePointToSegment(carPos, waypoints[i].position, waypoints[i+1].position);

            return Mathf.Min(distA, distB);
        }

        float DistancePointToSegment(Vector3 P, Vector3 A, Vector3 B)
        {
            Vector3 AB = B - A;
            Vector3 AP = P - A;
            
            float magnitudeAB = AB.sqrMagnitude; 
            
            if (magnitudeAB == 0) return Vector3.Distance(P, A);

            float t = Vector3.Dot(AP, AB) / magnitudeAB;

            t = Mathf.Clamp01(t);

            Vector3 closestPointOnLine = A + AB * t;

            return Vector3.Distance(P, closestPointOnLine);
        }

        public Transform GetClosestWaypoint(Vector3 carPosition)
        {
            int index = GetClosestWaypointIndex(carPosition);
            if (index != -1) return waypoints[index];
            return null;
        }

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Count < 2) return;
            Gizmos.color = pathColor;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i+1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
                    Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);
                }
            }
        }
    }
}