using UnityEngine;
using UnityEngine.Rendering;

public class Skids : MonoBehaviour {

    public Material Material;

	public int MaxSkidPartCount = 2048; 
    public float SkidWidth = 0.25f; 
    public float OfsetFromHitPoint = 0.03f;  //meters from ground.
    public float Details = 0.25f; //Small is better.

	class MarkSection {
		public Vector3 Pos = Vector3.zero;
		public Vector3 Normal = Vector3.zero;
		public Vector4 Tangent = Vector4.zero;
		public Vector3 Posl = Vector3.zero;
		public Vector3 Posr = Vector3.zero;
		public byte Intensity;
		public int LastIndex;
	};

	int markIndex;
	MarkSection[] skidmarks;
	Mesh marksMesh;
	MeshRenderer mr;
	MeshFilter mf;

	Vector3[] vertices;
	Vector3[] normals;
	Vector4[] tangents;
	Color32[] colors;
	Vector2[] uvs;
	int[] triangles;

	bool meshUpdated;
	bool haveSetBounds;

	protected void Start() {
		skidmarks = new MarkSection[MaxSkidPartCount];
		for (int i = 0; i < MaxSkidPartCount; i++) {
			skidmarks[i] = new MarkSection();
		}

		mf = GetComponent<MeshFilter>();
		mr = GetComponent<MeshRenderer>();
		if (mr == null) {
			mr = gameObject.AddComponent<MeshRenderer>();
		}
		marksMesh = new Mesh();
		marksMesh.MarkDynamic();
		if (mf == null) {
			mf = gameObject.AddComponent<MeshFilter>();
		}
		mf.sharedMesh = marksMesh;

		vertices = new Vector3[MaxSkidPartCount * 4];
		normals = new Vector3[MaxSkidPartCount * 4];
		tangents = new Vector4[MaxSkidPartCount * 4];
		colors = new Color32[MaxSkidPartCount * 4];
		uvs = new Vector2[MaxSkidPartCount * 4];
		triangles = new int[MaxSkidPartCount * 6];

		mr.shadowCastingMode = ShadowCastingMode.Off;
		mr.receiveShadows = false;
		mr.material = Material;
		mr.lightProbeUsage = LightProbeUsage.Off;
	}

	protected void LateUpdate() {
		if (!meshUpdated) return;
		meshUpdated = false;

		marksMesh.vertices = vertices;
		marksMesh.normals = normals;
		marksMesh.tangents = tangents;
		marksMesh.triangles = triangles;
		marksMesh.colors32 = colors;
		marksMesh.uv = uvs;

		if (!haveSetBounds) {
			marksMesh.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(10000, 10000, 10000));
			haveSetBounds = true;
		}

		mf.sharedMesh = marksMesh;
	}

	public int AddSkidMark(Vector3 pos, Vector3 normal, float intensity, int lastIndex, float MarkWidthRate) {
        float TempMarkWidth = SkidWidth * MarkWidthRate;
        if (intensity > 1) intensity = 1.0f;
		else if (intensity < 0) return -1;

		if (lastIndex > 0) {
			float sqrMagnitude = (pos - skidmarks[lastIndex].Pos).sqrMagnitude;
			if (sqrMagnitude < Details * Details) return lastIndex;
		}

		MarkSection curSection = skidmarks[markIndex];

		curSection.Pos = pos + normal * OfsetFromHitPoint;
		curSection.Normal = normal;
		curSection.Intensity = (byte)(intensity * 255f);
		curSection.LastIndex = lastIndex;

		if (lastIndex != -1) {
			MarkSection lastSection = skidmarks[lastIndex];
			Vector3 dir = (curSection.Pos - lastSection.Pos);
			Vector3 xDir = Vector3.Cross(dir, normal).normalized;

			curSection.Posl = curSection.Pos + xDir * TempMarkWidth * 0.5f;
			curSection.Posr = curSection.Pos - xDir * TempMarkWidth * 0.5f;
			curSection.Tangent = new Vector4(xDir.x, xDir.y, xDir.z, 1);

			if (lastSection.LastIndex == -1) {
				lastSection.Tangent = curSection.Tangent;
				lastSection.Posl = curSection.Pos + xDir * TempMarkWidth * 0.5f;
				lastSection.Posr = curSection.Pos - xDir * TempMarkWidth * 0.5f;
			}
		}

		UpdateSkidmarksMesh();

		int curIndex = markIndex;
		markIndex = ++markIndex % MaxSkidPartCount;

		return curIndex;
	}

	void UpdateSkidmarksMesh() {
		MarkSection curr = skidmarks[markIndex];

		// Nothing to connect to yet
		if (curr.LastIndex == -1) return;

		MarkSection last = skidmarks[curr.LastIndex];
		vertices[markIndex * 4 + 0] = last.Posl;
		vertices[markIndex * 4 + 1] = last.Posr;
		vertices[markIndex * 4 + 2] = curr.Posl;
		vertices[markIndex * 4 + 3] = curr.Posr;

		normals[markIndex * 4 + 0] = last.Normal;
		normals[markIndex * 4 + 1] = last.Normal;
		normals[markIndex * 4 + 2] = curr.Normal;
		normals[markIndex * 4 + 3] = curr.Normal;

		tangents[markIndex * 4 + 0] = last.Tangent;
		tangents[markIndex * 4 + 1] = last.Tangent;
		tangents[markIndex * 4 + 2] = curr.Tangent;
		tangents[markIndex * 4 + 3] = curr.Tangent;

		colors[markIndex * 4 + 0] = new Color32(0, 0, 0, last.Intensity);
		colors[markIndex * 4 + 1] = new Color32(0, 0, 0, last.Intensity);
		colors[markIndex * 4 + 2] = new Color32(0, 0, 0, curr.Intensity);
		colors[markIndex * 4 + 3] = new Color32(0, 0, 0, curr.Intensity);

		uvs[markIndex * 4 + 0] = new Vector2(0, 0);
		uvs[markIndex * 4 + 1] = new Vector2(1, 0);
		uvs[markIndex * 4 + 2] = new Vector2(0, 1);
		uvs[markIndex * 4 + 3] = new Vector2(1, 1);

		triangles[markIndex * 6 + 0] = markIndex * 4 + 0;
		triangles[markIndex * 6 + 2] = markIndex * 4 + 1;
		triangles[markIndex * 6 + 1] = markIndex * 4 + 2;

		triangles[markIndex * 6 + 3] = markIndex * 4 + 2;
		triangles[markIndex * 6 + 5] = markIndex * 4 + 1;
		triangles[markIndex * 6 + 4] = markIndex * 4 + 3;

		meshUpdated = true;
	}
}