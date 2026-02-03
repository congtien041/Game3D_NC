using UnityEditor;
using UnityEditor.UI; // Kế thừa từ ImageEditor để giữ giao diện chuẩn
using UnityEngine;

[CustomEditor(typeof(FreeformImage))]
public class FreeformImageEditor : ImageEditor
{
    // Các thuộc tính cần chỉnh sửa
    SerializedProperty m_TopLeftOffset;
    SerializedProperty m_TopRightOffset;
    SerializedProperty m_BottomLeftOffset;
    SerializedProperty m_BottomRightOffset;

    protected override void OnEnable()
    {
        base.OnEnable();
        // Lấy tham chiếu đến các biến trong script chính
        m_TopLeftOffset = serializedObject.FindProperty("topLeftOffset");
        m_TopRightOffset = serializedObject.FindProperty("topRightOffset");
        m_BottomLeftOffset = serializedObject.FindProperty("bottomLeftOffset");
        m_BottomRightOffset = serializedObject.FindProperty("bottomRightOffset");
    }

    public override void OnInspectorGUI()
    {
        // Vẽ giao diện mặc định của Image (Source Image, Color, etc.)
        base.OnInspectorGUI();

        // Vẽ thêm phần chỉnh Offset
        serializedObject.Update();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Corner Offsets (Kéo chấm vàng trên Scene)", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(m_TopLeftOffset);
        EditorGUILayout.PropertyField(m_TopRightOffset);
        EditorGUILayout.PropertyField(m_BottomLeftOffset);
        EditorGUILayout.PropertyField(m_BottomRightOffset);

        serializedObject.ApplyModifiedProperties();
    }

    // Hàm này vẽ các nút kéo (Handles) trên màn hình Scene
    protected virtual void OnSceneGUI()
    {
        FreeformImage script = (FreeformImage)target;
        RectTransform rectT = script.rectTransform;

        // Lấy các điểm gốc của RectTransform
        Vector3[] corners = new Vector3[4];
        rectT.GetLocalCorners(corners); 
        // corners[0]=BL, [1]=TL, [2]=TR, [3]=BR

        // Bắt đầu check thay đổi để hỗ trợ Undo/Redo
        EditorGUI.BeginChangeCheck();

        // Vẽ và xử lý kéo thả cho từng góc
        // Lưu ý: Handles hoạt động trong World Space, ta cần chuyển đổi qua lại
        Vector3 newTL = ShowPoint(rectT, corners[1], script.topLeftOffset);
        Vector3 newTR = ShowPoint(rectT, corners[2], script.topRightOffset);
        Vector3 newBL = ShowPoint(rectT, corners[0], script.bottomLeftOffset);
        Vector3 newBR = ShowPoint(rectT, corners[3], script.bottomRightOffset);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(script, "Move Freeform Corner");
            
            // Cập nhật lại giá trị Offset
            script.topLeftOffset = newTL - corners[1];
            script.topRightOffset = newTR - corners[2];
            script.bottomLeftOffset = newBL - corners[0];
            script.bottomRightOffset = newBR - corners[3];

            // Báo cho Editor biết là dữ liệu đã thay đổi để vẽ lại
            script.SetAllDirty();
        }
    }

    // Hàm phụ trợ để vẽ 1 điểm handle
    private Vector3 ShowPoint(RectTransform rectT, Vector3 cornerBaseLocal, Vector3 offset)
    {
        // Chuyển từ Local (Rect) -> World để vẽ Handle
        Vector3 worldPos = rectT.TransformPoint(cornerBaseLocal + offset);

        // Vẽ Handle
        float size = HandleUtility.GetHandleSize(worldPos) * 0.15f;
        Handles.color = Color.yellow;
        Vector3 newWorldPos = Handles.FreeMoveHandle(worldPos, size, Vector3.zero, Handles.SphereHandleCap);

        // Chuyển ngược từ World -> Local để lưu dữ liệu
        return rectT.InverseTransformPoint(newWorldPos);
    }
}