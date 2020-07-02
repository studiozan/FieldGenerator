using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class Road
	{
		public Road(GameObject roadObject)
		{
			gameObject = roadObject;
			meshFilter = gameObject.GetComponent<MeshFilter>();
		}

		public void GenerateAlongRiver(River river, RiverSide riverSide, float width, float distanceFromRiver)
		{
			points.Clear();
			List<Vector3> riverPoints = river.Points;
			float riverWidth = river.Width;
			Vector3 riverDistance = riverPoints[riverPoints.Count - 1] - riverPoints[0];
			float riverAngle = Mathf.Atan2(riverDistance.x, riverDistance.z) * Mathf.Rad2Deg;

			float x = riverWidth / 2 + distanceFromRiver + width / 2;
			System.Func<float, float, bool> isOutside;
			if (riverSide == RiverSide.kLeftSide)
			{
				isOutside = (a1, a2) => a1 < a2;
				x *= -1;
			}
			else
			{
				isOutside = (a1, a2) => a1 > a2;
			}

			Vector3 basePoint = new Vector3(x, 0, 0);
			basePoint = Quaternion.Euler(0, riverAngle, 0) * basePoint;

			Quaternion reverse = Quaternion.Euler(0, -riverAngle, 0);
			points.Add(basePoint + riverPoints[0]);
			int index = 0;
			while (index < riverPoints.Count - 1)
			{
				int relativeIndex = 1;
				if (index != riverPoints.Count - 2)
				{
					Vector3 dist1 = riverPoints[index + 1] - riverPoints[index];
					dist1 = reverse * dist1;
					Vector3 dist2 = riverPoints[index + 2] - riverPoints[index];
					dist2 = reverse * dist2;
					float angle1 = Mathf.Atan2(dist1.x, dist1.z) * Mathf.Rad2Deg;
					float angle2 = Mathf.Atan2(dist2.x, dist2.z) * Mathf.Rad2Deg;
					relativeIndex = isOutside(angle1, angle2) ? 1 : 2;
				}
				points.Add(basePoint + riverPoints[index + relativeIndex]);
				index += relativeIndex;
			}

			Mesh mesh = MeshCreator.CreateLineMesh(points, width);
			mesh.GetVertices(vertices);
			mesh.GetIndices(triangles, 0);
			meshFilter.sharedMesh = mesh;
		}

		public void Destroy()
		{
			Object.Destroy(gameObject);
			gameObject = null;
			meshFilter = null;
			points.Clear();
			width = 0;
		}

		static float CrossVec2(float x1, float y1, float x2, float y2)
		{
			return x1 * y2 - y1 * x2;
		}

		public GameObject GameObject
		{
			get => gameObject;
		}

		public List<Vector3> Points
		{
			get => points;
		}

		public float Width
		{
			get => width;
		}

		GameObject gameObject;
		MeshFilter meshFilter;
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector3> points = new List<Vector3>();
		float width;
	}
}
