using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using VehicleSystem.Core; // Gọi TrackPath

namespace VehicleSystem.Tools
{
    [RequireComponent(typeof(TrackPath))]
    public class AutoTrackGenerator : MonoBehaviour
    {
        [Header("--- CẤU HÌNH ĐẦU VÀO ---")]
        [Tooltip("Kéo Object cha (chứa tất cả các mảnh đường ghép lại) vào đây")]
        public Transform roadParent;

        [Tooltip("Chiều cao của điểm Node so với mặt đường (VD: 1.0 mét)")]
        public float heightOffset = 1.0f;

        [Header("--- CẤU HÌNH SẮP XẾP ---")]
        [Tooltip("Tick: Tự động tìm mảnh đường gần nhất để nối tiếp.\nBỏ Tick: Nối theo đúng thứ tự trên Hierarchy.")]
        public bool smartProximitySort = true;

        // =========================================================
        // NÚT BẤM THẦN THÁNH: CHUỘT PHẢI VÀO SCRIPT CHỌN "AUTO GENERATE"
        // =========================================================
        [ContextMenu("🚀 TỰ ĐỘNG TẠO ĐƯỜNG CHẠY (AUTO GENERATE)")]
        public void GenerateTrack()
        {
            if (roadParent == null)
            {
                Debug.LogError("Chưa kéo Road Parent vào! Hãy tạo một Object cha chứa tất cả các mảnh đường.");
                return;
            }

            TrackPath trackPath = GetComponent<TrackPath>();
            
            // 1. Dọn dẹp Control Points cũ
            if (trackPath.controlPoints != null)
            {
                foreach (Transform cp in trackPath.controlPoints)
                {
                    if (cp != null) DestroyImmediate(cp.gameObject);
                }
                trackPath.controlPoints.Clear();
            }
            else
            {
                trackPath.controlPoints = new List<Transform>();
            }

            // Xóa rác cũ nếu có
            Transform oldAnchors = transform.Find("Auto_Anchors");
            if (oldAnchors != null) DestroyImmediate(oldAnchors.gameObject);

            // 2. Tạo Group mới chứa các điểm neo (Anchor)
            GameObject anchorsGroup = new GameObject("Auto_Anchors");
            anchorsGroup.transform.SetParent(transform);

            // 3. Lấy tất cả các mảnh đường
            List<Transform> allRoads = new List<Transform>();
            foreach (Transform child in roadParent)
            {
                // Loại bỏ những object bị ẩn
                if (child.gameObject.activeInHierarchy) 
                    allRoads.Add(child);
            }

            if (allRoads.Count == 0) return;

            List<Vector3> finalPositions = new List<Vector3>();

            // 4. Thuật toán tìm tâm và sắp xếp
            if (smartProximitySort)
            {
                // Sắp xếp tự động bằng khoảng cách (Nearest Neighbor)
                Transform currentRoad = allRoads[0];
                allRoads.Remove(currentRoad);
                finalPositions.Add(GetCenterOfRoad(currentRoad));

                while (allRoads.Count > 0)
                {
                    Vector3 currentPos = GetCenterOfRoad(currentRoad);
                    // Tìm mảnh đường gần nhất với mảnh hiện tại
                    Transform closestRoad = allRoads.OrderBy(r => Vector3.Distance(currentPos, GetCenterOfRoad(r))).First();
                    
                    finalPositions.Add(GetCenterOfRoad(closestRoad));
                    currentRoad = closestRoad;
                    allRoads.Remove(closestRoad);
                }
            }
            else
            {
                // Sắp xếp theo đúng thứ tự 1, 2, 3... trong Hierarchy
                foreach (Transform road in allRoads)
                {
                    finalPositions.Add(GetCenterOfRoad(road));
                }
            }

            // 5. Sinh ra các Control Points
            for (int i = 0; i < finalPositions.Count; i++)
            {
                GameObject cp = new GameObject($"Anchor_{i}");
                cp.transform.SetParent(anchorsGroup.transform);
                // Nâng điểm neo lên một chút để không bị chìm dưới đường
                cp.transform.position = finalPositions[i] + Vector3.up * heightOffset; 
                
                trackPath.controlPoints.Add(cp.transform);
            }

            // 6. Tự động gọi hàm BAKE PATH của bạn để làm mượt
            trackPath.BakePath();

            Debug.Log($"<color=green>✅ Đã tạo thành công {finalPositions.Count} điểm Control Point và Bake đường mượt!</color>");
        }

        // Hàm hỗ trợ: Tìm chính xác tâm của vật thể dù Pivot bị lệch
        private Vector3 GetCenterOfRoad(Transform road)
        {
            // Ưu tiên dùng MeshRenderer để tính toán kích thước thật
            Renderer renderer = road.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds.center;
            }

            // Fallback nếu không có MeshRenderer thì dùng vị trí cục bộ
            return road.position;
        }
    }
}