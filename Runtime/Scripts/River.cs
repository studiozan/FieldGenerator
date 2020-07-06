using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class River
	{
		public River(GameObject riverObject)
		{
			gameObject = riverObject;
			meshFilter = gameObject.GetComponent<MeshFilter>();
		}

		public void Generate(RiverParameter parameter, System.Random random)
		{
			points.Clear();

			Vector3 startPoint = parameter.Start;
			Vector3 endPoint = parameter.End;
			int numberPointBetween = parameter.NumberPointBetween;
			float angleRange = parameter.AngleRange;

			points.Add(startPoint);
			Vector3 distance = endPoint - startPoint;
			float riverAngle = Mathf.Atan2(distance.x, distance.z) * Mathf.Rad2Deg;
			Quaternion rotation = Quaternion.Euler(0, riverAngle, 0);
			Vector3 zeroDegEndPoint = Quaternion.Euler(0, -riverAngle, 0) * distance;
			float pointDistance = zeroDegEndPoint.z / (numberPointBetween + 1);
			Vector3 lastZeroDegPoint = Vector3.zero;
			for (int i0 = 0; i0 < numberPointBetween; ++i0)
			{
				float angle = (float)random.NextDouble() * angleRange - angleRange / 2;
				if (i0 != 0)
				{
					Vector3 dist = zeroDegEndPoint - lastZeroDegPoint;
					angle += Mathf.Atan2(dist.x, dist.z) * Mathf.Rad2Deg;
				}
				Vector3 pos = Quaternion.Euler(0, angle, 0) * Vector3.forward;
				pos *= pointDistance / pos.z;
				pos += lastZeroDegPoint;
				lastZeroDegPoint = pos;
				pos = rotation * pos;
				pos += startPoint;
				points.Add(pos);
			}
			points.Add(endPoint);

			width = parameter.Width;
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

		public void SetRoadAlongRiver(Road leftSide, Road rightSide)
		{
			leftRoad = leftSide;
			rightRoad = rightSide;
		}

		public static void Join(River upstream, River downstream)
		{
			List<Vector3> usVertices = upstream.vertices;
			List<Vector3> dsVertices = downstream.vertices;
			Vector3 leftMidpoint = Vector3.Lerp(usVertices[usVertices.Count - 2], dsVertices[0], 0.5f);
			Vector3 rightMidpoint = Vector3.Lerp(usVertices[usVertices.Count - 1], dsVertices[1], 0.5f);
			usVertices[usVertices.Count - 2] = dsVertices[0] = leftMidpoint;
			usVertices[usVertices.Count - 1] = dsVertices[1] = rightMidpoint;

			upstream.meshFilter.sharedMesh = MeshCreator.CreateMesh(usVertices, upstream.triangles);
			downstream.meshFilter.sharedMesh = MeshCreator.CreateMesh(dsVertices, downstream.triangles);
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

		public Road LeftRoad
		{
			get => leftRoad;
		}

		public Road RightRoad
		{
			get => rightRoad;
		}

		public List<River> Downstreams
		{
			get => downstreams;
		}

		GameObject gameObject;
		MeshFilter meshFilter;
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector3> points = new List<Vector3>();
		float width;

		Road leftRoad;
		Road rightRoad;
		List<River> downstreams = new List<River>();
	}
}
