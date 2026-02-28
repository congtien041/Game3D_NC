using UnityEngine;
using System.Collections.Generic;

namespace VehicleSystem.Core
{
    public class TrackPath : MonoBehaviour
    {
        [Header("--- CẤU HÌNH BAKE ---")]
        [Tooltip("Kéo các điểm mốc (Anchor) bạn tự đặt vào đây")]
        public List<Transform> controlPoints = new List<Transform>();
        
        [Tooltip("Khoảng cách giữa các điểm sau khi Bake (VD: 5 mét 1 điểm)")]
        public float spacing = 5.0f;
        
        [Tooltip("Độ phân giải đường cong (Càng cao càng mượt, mặc định 10)")]
        public int resolution = 10;
        
        [Tooltip("Có nối điểm cuối về điểm đầu không?")]
        public bool isLoop = true;

        [Header("--- KẾT QUẢ (READ ONLY) ---")]
        public Color pathColor = Color.yellow;
        // Đây là list mà xe sẽ dùng (được sinh ra tự động)
        public List<Transform> waypoints = new List<Transform>();

        // Container chứa các điểm đã sinh ra để không làm rác Hierarchy
        private GameObject bakedContainer;

        // =========================================================
        // CHỨC NĂNG BAKE (BẤM CHUỘT PHẢI VÀO SCRIPT CHỌN "BAKE PATH")
        // =========================================================
        [ContextMenu("⚡ BAKE PATH (Làm Mượt & Chia Đều)")]
        public void BakePath()
        {
            if (controlPoints == null || controlPoints.Count < 2)
            {
                Debug.LogError("Cần ít nhất 2 Control Points để Bake!");
                return;
            }

            // 1. Dọn dẹp cũ
            if (bakedContainer != null) DestroyImmediate(bakedContainer);
            // Tìm và xóa container cũ nếu bị mất tham chiếu
            Transform oldContainer = transform.Find("BakedWaypoints_Container");
            if (oldContainer != null) DestroyImmediate(oldContainer.gameObject);

            bakedContainer = new GameObject("BakedWaypoints_Container");
            bakedContainer.transform.SetParent(transform);
            bakedContainer.transform.localPosition = Vector3.zero;

            waypoints.Clear();

            // 2. Tạo đường cong mềm (Spline)
            List<Vector3> rawPoints = GenerateCatmullRomSpline();

            // 3. Chia đều khoảng cách (Resampling)
            List<Vector3> evenlySpacedPoints = ResamplePath(rawPoints, spacing);

            // 4. Sinh ra các GameObject thật
            for (int i = 0; i < evenlySpacedPoints.Count; i++)
            {
                GameObject p = new GameObject($"Node_{i}");
                p.transform.SetParent(bakedContainer.transform);
                p.transform.position = evenlySpacedPoints[i];
                
                // Tính hướng xoay cho node (để xe biết hướng nào là tới)
                if (i < evenlySpacedPoints.Count - 1)
                    p.transform.LookAt(evenlySpacedPoints[i + 1]);
                else if (isLoop && evenlySpacedPoints.Count > 1)
                    p.transform.LookAt(evenlySpacedPoints[0]); // Nối về đầu
                else if (i > 0)
                    p.transform.rotation = waypoints[i - 1].rotation; // Giữ nguyên hướng cũ

                waypoints.Add(p.transform);
            }

            Debug.Log($"<color=green>✅ Đã Bake xong! Tạo ra {waypoints.Count} điểm mượt mà.</color>");
        }

        // --- THUẬT TOÁN CATMULL-ROM SPLINE ---
        private List<Vector3> GenerateCatmullRomSpline()
        {
            List<Vector3> path = new List<Vector3>();
            
            int count = controlPoints.Count;
            // Nếu đóng vòng (Loop), ta xét thêm các điểm nối
            int limit = isLoop ? count : count - 1;

            for (int i = 0; i < limit; i++)
            {
                // Lấy 4 điểm: P0, P1(Start), P2(End), P3
                Vector3 p0 = controlPoints[(i - 1 + count) % count].position;
                Vector3 p1 = controlPoints[i % count].position;
                Vector3 p2 = controlPoints[(i + 1) % count].position;
                Vector3 p3 = controlPoints[(i + 2) % count].position;

                // Nếu không Loop, xử lý đoạn đầu và cuối đặc biệt
                if (!isLoop)
                {
                    if (i == 0) p0 = p1 - (p2 - p1);
                    if (i == count - 2) p3 = p2 + (p2 - p1);
                }

                // Nội suy các điểm giữa P1 và P2
                for (int j = 0; j < resolution; j++)
                {
                    float t = j / (float)resolution;
                    path.Add(GetCatmullRomPosition(t, p0, p1, p2, p3));
                }
            }
            
            // Thêm điểm cuối cùng nếu không loop
            if (!isLoop) path.Add(controlPoints[count - 1].position);

            return path;
        }

        private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            // Công thức Catmull-Rom
            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
            );
        }

        // --- THUẬT TOÁN CHIA ĐỀU KHOẢNG CÁCH (RESAMPLING) ---
        private List<Vector3> ResamplePath(List<Vector3> rawPoints, float segmentLen)
        {
            List<Vector3> newPoints = new List<Vector3>();
            if (rawPoints.Count == 0) return newPoints;

            newPoints.Add(rawPoints[0]);
            Vector3 prevPoint = rawPoints[0];
            float distCovered = 0f;

            for (int i = 1; i < rawPoints.Count; i++)
            {
                float dist = Vector3.Distance(prevPoint, rawPoints[i]);
                
                while (distCovered + dist >= segmentLen)
                {
                    float remaining = segmentLen - distCovered;
                    Vector3 newPos = prevPoint + (rawPoints[i] - prevPoint).normalized * remaining;
                    
                    newPoints.Add(newPos);
                    
                    // Cập nhật lại mốc để tính tiếp
                    prevPoint = newPos;
                    dist -= remaining;
                    distCovered = 0f;
                }
                
                distCovered += dist;
                prevPoint = rawPoints[i];
            }
            
            return newPoints;
        }

        // =========================================================
        // CÁC HÀM GET DỮ LIỆU CŨ (GIỮ NGUYÊN ĐỂ XE CHẠY ĐƯỢC)
        // =========================================================
        public Transform GetClosestWaypoint(Vector3 carPosition)
        {
            int index = GetClosestWaypointIndex(carPosition);
            if (index != -1 && index < waypoints.Count) return waypoints[index];
            return null;
        }

        public int GetClosestWaypointIndex(Vector3 carPos)
        {
            int closestIndex = -1;
            float minDistance = Mathf.Infinity;

            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null) continue;
                float dist = Vector3.SqrMagnitude(carPos - waypoints[i].position); // Dùng SqrMagnitude nhanh hơn
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
            if (i == -1 || waypoints.Count < 2) return Mathf.Infinity;

            // Kiểm tra 2 đoạn lân cận để tìm khoảng cách ngắn nhất
            float dist = Mathf.Infinity;
            
            // Đoạn trước đó
            if (i > 0) 
                dist = Mathf.Min(dist, DistancePointToSegment(carPos, waypoints[i-1].position, waypoints[i].position));
            // Đoạn sau đó
            if (i < waypoints.Count - 1)
                dist = Mathf.Min(dist, DistancePointToSegment(carPos, waypoints[i].position, waypoints[i+1].position));
            // Đoạn nối vòng (nếu là điểm cuối và có loop)
            if (isLoop && i == waypoints.Count - 1)
                dist = Mathf.Min(dist, DistancePointToSegment(carPos, waypoints[i].position, waypoints[0].position));

            return dist;
        }

        float DistancePointToSegment(Vector3 P, Vector3 A, Vector3 B)
        {
            Vector3 AB = B - A;
            Vector3 AP = P - A;
            float magnitudeAB = AB.sqrMagnitude; 
            if (magnitudeAB == 0) return Vector3.Distance(P, A);
            float t = Vector3.Dot(AP, AB) / magnitudeAB;
            t = Mathf.Clamp01(t);
            return Vector3.Distance(P, A + AB * t);
        }

        private void OnDrawGizmos()
        {
            // Vẽ Control Points (Màu đỏ - Để bạn dễ nhìn lúc chỉnh)
            if (controlPoints != null && controlPoints.Count > 0)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < controlPoints.Count; i++)
                {
                    if (controlPoints[i] != null)
                    {
                        Gizmos.DrawSphere(controlPoints[i].position, 1f);
                        if (i < controlPoints.Count - 1)
                            Gizmos.DrawLine(controlPoints[i].position, controlPoints[i+1].position);
                    }
                }
            }

            // Vẽ Waypoints đã Bake (Màu vàng - Đường xe chạy)
            if (waypoints != null && waypoints.Count > 1)
            {
                Gizmos.color = pathColor;
                for (int i = 0; i < waypoints.Count - 1; i++)
                {
                    if (waypoints[i] != null && waypoints[i+1] != null)
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
                }
                // Vẽ đường nối loop
                if (isLoop && waypoints.Count > 0 && waypoints[0] != null && waypoints[waypoints.Count - 1] != null)
                     Gizmos.DrawLine(waypoints[waypoints.Count - 1].position, waypoints[0].position);
            }
        }
    }
}