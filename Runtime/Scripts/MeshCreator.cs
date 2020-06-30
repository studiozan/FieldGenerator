using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshCreator
{
	public static Mesh CreateLineMesh(List<Vector3> points, float width)
	{
		var mesh = new Mesh();
		var vertices = new List<Vector3>();
		var triangles = new List<int>();

		//頂点
		float halfWidth = width / 2;
		Vector3 left = new Vector3(-halfWidth, 0, 0);
		Vector3 right = new Vector3(halfWidth, 0, 0);
		Vector3 dist = points[points.Count - 1] - points[0];
		float angle = Mathf.Atan2(dist.x, dist.z) * Mathf.Rad2Deg;
		Quaternion rotation = Quaternion.Euler(0, angle, 0);
		for (int i0 = 0; i0 < points.Count; ++i0)
		{
			Vector3 point = points[i0];
			vertices.Add(rotation * left + point);
			vertices.Add(rotation * right + point);
		}
		mesh.SetVertices(vertices);

		//インデックス
		for (int i0 = 0; i0 < points.Count - 1; ++i0)
		{
			triangles.Add(2 * i0 + 0);
			triangles.Add(2 * i0 + 2);
			triangles.Add(2 * i0 + 1);
			triangles.Add(2 * i0 + 1);
			triangles.Add(2 * i0 + 2);
			triangles.Add(2 * i0 + 3);
		}
		mesh.SetTriangles(triangles, 0);
		
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		mesh.RecalculateBounds();

		return mesh;
	}
}
