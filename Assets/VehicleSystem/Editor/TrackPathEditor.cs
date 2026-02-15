using UnityEngine;
using UnityEditor; 
using VehicleSystem.Core;

[CustomEditor(typeof(TrackPath))]
public class TrackPathEditor : Editor
{
    private TrackPath script;
    private bool isEditMode = false;

    private void OnEnable()
    {
        script = (TrackPath)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); 

        GUILayout.Space(10);
        
        if (!isEditMode)
        {
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("BẮT ĐẦU VẼ ĐƯỜNG (Start Editing)", GUILayout.Height(40)))
            {
                isEditMode = true;
                Selection.activeGameObject = script.gameObject;
            }
        }
        else
        {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("DỪNG VẼ (Stop Editing)", GUILayout.Height(40)))
            {
                isEditMode = false;
            }
            
            EditorGUILayout.HelpBox(
                "HƯỚNG DẪN:\n" +
                "- Giữ CTRL + Chuột Trái: Tạo điểm mới trên mặt đất.\n" +
                "- Shift: Thoát chế độ vẽ.", 
                MessageType.Info);
        }
    }

    private void OnSceneGUI()
    {
        if (!isEditMode) return;

        Event e = Event.current;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.LeftShift)
        {
            isEditMode = false;
            Repaint(); 
            return;
        }

        if (e.control && e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                CreateWaypoint(hit.point);
                
                e.Use(); 
            }
        }
    }

    void CreateWaypoint(Vector3 position)
    {
        GameObject p = new GameObject("Point_" + script.waypoints.Count);
        p.transform.position = position;
        p.transform.SetParent(script.transform);

        Undo.RegisterCreatedObjectUndo(p, "Create Waypoint");

        if (script.waypoints.Count > 0)
        {
            Transform lastPoint = script.waypoints[script.waypoints.Count - 1];
            if (lastPoint != null)
            {
                lastPoint.LookAt(p.transform);
                p.transform.rotation = lastPoint.rotation; 
            }
        }

        Undo.RecordObject(script, "Add Waypoint"); 
        script.waypoints.Add(p.transform);
        
        Debug.Log("Đã chấm điểm tại: " + position);
    }
}