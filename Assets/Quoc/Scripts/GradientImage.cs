using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GradientImage : BaseMeshEffect
{
    public Color leftColor = Color.white;
    public Color rightColor = Color.black;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        UIVertex vertex = new UIVertex();

        float leftX = 0f;
        float rightX = 0f;

        // Lấy biên trái phải
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            leftX = Mathf.Min(leftX, vertex.position.x);
            rightX = Mathf.Max(rightX, vertex.position.x);
        }

        float width = rightX - leftX;

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            float normalizedX = (vertex.position.x - leftX) / width;

            vertex.color = Color.Lerp(leftColor, rightColor, normalizedX);

            vh.SetUIVertex(vertex, i);
        }
    }
}