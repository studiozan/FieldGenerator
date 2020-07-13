using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class MeshCreator : MonoBehaviour
	{
		void Awake()
		{
			riverObj = Instantiate(riverPrefab);
			riverMeshFilter = riverObj.GetComponent<MeshFilter>();

			TownGenerator generator = GetComponent<TownGenerator>();
			generator.OnGenerate += () =>
			{
				riverMeshFilter.sharedMesh = CreateRiverMesh(generator.GetRiverConnectPointList(), generator.RiverWidth);
			};
		}

		public Mesh CreateMesh(List<Vector3> vertices, List<int> triangles)
		{
			var mesh = new Mesh();
			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0);

			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
			mesh.RecalculateBounds();

			return mesh;
		}

		public Mesh CreateRiverMesh(List<FieldConnectPoint> riverPoints, float width)
		{
			var vertices = new List<Vector3>();
			var triangles = new List<int>();
			var connectedMap = new Dictionary<int, HashSet<int>>();

			SetPointsIndex(riverPoints);
			CreateRiverVerticesAndIndicesRecursive(riverPoints[0], width, vertices, triangles, connectedMap);

			return CreateMesh(vertices, triangles);
		}

		void CreateRiverVerticesAndIndicesRecursive(FieldConnectPoint point, float width, List<Vector3> vertices, List<int> indices, Dictionary<int, HashSet<int>> connectedMap)
		{
			List<FieldConnectPoint> connectionList = point.ConnectionList;
			int prevLeftIndex = vertices.Count - 2;
			int prevRightIndex = vertices.Count - 1;
			bool isEndPoint = true;
			for (int i0 = 0; i0 < connectionList.Count; ++i0)
			{
				FieldConnectPoint nextPoint = connectionList[i0];
				if (IsConnectedPoint(point.Index, nextPoint.Index, connectedMap) == false)
				{
					isEndPoint = false;
					RegisterConnectedMap(point.Index, nextPoint.Index, connectedMap);
					Vector3 dir = nextPoint.Position - point.Position;
					float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
					Quaternion rotation = Quaternion.Euler(0, angle, 0);
					Vector3 leftVert = rotation * new Vector3(-width / 2, 0, 0) + point.Position;
					Vector3 rightVert = rotation * new Vector3(width / 2, 0, 0) + point.Position;
					vertices.Add(leftVert);
					vertices.Add(rightVert);
					if (prevLeftIndex >= 0)
					{
						int leftIndex = vertices.Count - 2;
						int rightIndex = vertices.Count - 1;
						indices.Add(prevLeftIndex);
						indices.Add(leftIndex);
						indices.Add(prevRightIndex);
						indices.Add(prevRightIndex);
						indices.Add(leftIndex);
						indices.Add(rightIndex);
					}

					CreateRiverVerticesAndIndicesRecursive(nextPoint, width, vertices, indices, connectedMap);
				}
			}

			if (isEndPoint != false && connectionList.Count > 0)
			{
				if (prevLeftIndex >= 0)
				{
					Vector3 dir = vertices[prevRightIndex] - vertices[prevLeftIndex];
					float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
					Quaternion rotation = Quaternion.Euler(0, angle, 0);
					Vector3 leftVert = rotation * new Vector3(0, 0, -width / 2) + point.Position;
					Vector3 rightVert = rotation * new Vector3(0, 0, width / 2) + point.Position;
					vertices.Add(leftVert);
					vertices.Add(rightVert);
					int leftIndex = vertices.Count - 2;
					int rightIndex = vertices.Count - 1;
					indices.Add(prevLeftIndex);
					indices.Add(leftIndex);
					indices.Add(prevRightIndex);
					indices.Add(prevRightIndex);
					indices.Add(leftIndex);
					indices.Add(rightIndex);
				}
			}
		}

		void SetPointsIndex(List<FieldConnectPoint> connectPoints)
		{
			for (int i0 = 0; i0 < connectPoints.Count; ++i0)
			{
				connectPoints[i0].Index = i0;
			}
		}

		bool IsConnectedPoint(int index1, int index2, Dictionary<int, HashSet<int>> connectedMap)
		{
			HashSet<int> connectedSet;
			if (connectedMap.TryGetValue(index1, out connectedSet) != false)
			{
				if (connectedSet.Contains(index2) != false)
				{
					return true;
				}
			}

			return false;
		}

		void RegisterConnectedMap(int index1, int index2, Dictionary<int, HashSet<int>> connectedMap)
		{
			if (connectedMap.TryGetValue(index1, out HashSet<int> connectedSet1) != false)
			{
				connectedSet1.Add(index2);
			}
			else
			{
				var connectedSet = new HashSet<int>();
				connectedSet.Add(index2);
				connectedMap.Add(index1, connectedSet);
			}

			if (connectedMap.TryGetValue(index2, out HashSet<int> connectedSet2) != false)
			{
				connectedSet2.Add(index1);
			}
			else
			{
				var connectedSet = new HashSet<int>();
				connectedSet.Add(index1);
				connectedMap.Add(index2, connectedSet);
			}
		}



		[SerializeField]
		GameObject riverPrefab = default;

		GameObject riverObj;
		MeshFilter riverMeshFilter;
	}
}
