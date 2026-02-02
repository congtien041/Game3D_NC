using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Freeform Image")]
public class FreeformImage : Image
{
    // Các offset cho 4 góc (khoảng cách lệch so với góc chuẩn)
    public Vector2 topLeftOffset;
    public Vector2 topRightOffset;
    public Vector2 bottomLeftOffset;
    public Vector2 bottomRightOffset;

    // Biến lưu vị trí đỉnh để tính toán va chạm (Raycast)
    private Vector2 _mBL, _mTL, _mTR, _mBR;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = rectTransform.rect;

        // Tính toán vị trí các đỉnh dựa trên Rect gốc + Offset
        _mBL = new Vector2(r.xMin, r.yMin) + bottomLeftOffset;
        _mTL = new Vector2(r.xMin, r.yMax) + topLeftOffset;
        _mTR = new Vector2(r.xMax, r.yMax) + topRightOffset;
        _mBR = new Vector2(r.xMax, r.yMin) + bottomRightOffset;

        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;

        // Add 4 đỉnh vào mesh
        vert.position = _mBL; vert.uv0 = new Vector2(0, 0); vh.AddVert(vert); // 0
        vert.position = _mTL; vert.uv0 = new Vector2(0, 1); vh.AddVert(vert); // 1
        vert.position = _mTR; vert.uv0 = new Vector2(1, 1); vh.AddVert(vert); // 2
        vert.position = _mBR; vert.uv0 = new Vector2(1, 0); vh.AddVert(vert); // 3

        // Tạo 2 tam giác
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }

    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out localPoint);
        
        // Kiểm tra điểm nằm trong 1 trong 2 tam giác
        return IsPointInTriangle(localPoint, _mBL, _mTL, _mTR) || 
               IsPointInTriangle(localPoint, _mTR, _mBR, _mBL);
    }

    private bool IsPointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
        var t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;
        var A = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;
        return A < 0 ? (s <= 0 && s + t >= A) : (s >= 0 && s + t <= A);
    }
}