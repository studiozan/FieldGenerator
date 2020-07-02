using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class TownGenerator : MonoBehaviour
	{
		void Awake()
		{
			random = new System.Random(seed);

			riverPointsPlacer = new ObjectPlacer("RiverPoints");
			roadPointsPlacer = new ObjectPlacer("RoadPoints");

			GenerateTown();
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space) != false)
			{
				GenerateTown();
			}
		}

		void GenerateTown()
		{
			GenerateRiver();
			GenerateRoad();
		}

		void GenerateRiver()
		{
			ClearRivers();
			riverPoints.Clear();
			int lastMainstreamIndex = 0;
			for (int i0 = 0; i0 < riverBasePoints.Length - 1; ++i0)
			{
				RiverPoint currentPoint = riverBasePoints[i0];
				RiverPoint[] forkedRiverPoints = currentPoint.ForkedRiverPoints;
				if (forkedRiverPoints.Length > 0)
				{
					GenerateForkedRiverRecursively(currentPoint.Point, forkedRiverPoints);
				}
				RiverPoint nextPoint = riverBasePoints[i0 + 1];
				var riverParam = new RiverParameter
				{
					Start = currentPoint.Point,
					End = nextPoint.Point,
					NumberPointBetween = numberOfPoint,
					Width = riverWidth,
					AngleRange = angleRange,
				};
				var river = new River(Instantiate(riverPrefab));
				river.Generate(riverParam, random);
				riverPoints.AddRange(river.Points);
				rivers.Add(river);

				if (i0 != 0)
				{
					River.Join(rivers[lastMainstreamIndex], river);
				}
				lastMainstreamIndex = rivers.Count - 1;
			}

			riverPointsPlacer.PlaceObjects(prefab, riverPoints);
		}

		void GenerateForkedRiverRecursively(Vector3 upstream, RiverPoint[] points)
		{
			Vector3 currentPoint = upstream;
			for (int i0 = 0; i0 < points.Length; ++i0)
			{
				Vector3 nextPoint = points[i0].Point;
				var riverParam = new RiverParameter
				{
					Start = currentPoint,
					End = nextPoint,
					NumberPointBetween = numberOfPoint,
					Width = riverWidth,
					AngleRange = angleRange,
				};
				var river = new River(Instantiate(riverPrefab));
				river.Generate(riverParam, random);
				riverPoints.AddRange(river.Points);
				rivers.Add(river);

				currentPoint = nextPoint;

				RiverPoint[] forkedRiverPoints = points[i0].ForkedRiverPoints;
				if (forkedRiverPoints.Length > 0)
				{
					GenerateForkedRiverRecursively(currentPoint, forkedRiverPoints);
				}
			}
		}

		void ClearRivers()
		{
			for (int i0 = 0; i0 < rivers.Count; ++i0)
			{
				rivers[i0].Destroy();
			}
			rivers.Clear();
		}

		void GenerateRoad()
		{
			GenerateRoadAlongRiver();
			// GenerateGridRoad();
		}

		void GenerateRoadAlongRiver()
		{
			ClearRoadsAlongRiver();
			roadPoints.Clear();
			for (int i0 = 0; i0 < rivers.Count; ++i0)
			{
				River river = rivers[i0];

				var leftRoad = new Road(Instantiate(roadPrefab));
				leftRoad.GenerateAlongRiver(river, RiverSide.kLeftSide, roadWidth, distanceFromRiverToRoad);
				roadPoints.AddRange(leftRoad.Points);
				roadsAlongRiver.Add(leftRoad);

				var rightRoad = new Road(Instantiate(roadPrefab));
				rightRoad.GenerateAlongRiver(river, RiverSide.kRightSide, roadWidth, distanceFromRiverToRoad);
				roadPoints.AddRange(rightRoad.Points);
				roadsAlongRiver.Add(rightRoad);
			}
			roadPointsPlacer.PlaceObjects(prefab, roadPoints);
		}

		void ClearRoadsAlongRiver()
		{
			for (int i0 = 0; i0 < roadsAlongRiver.Count; ++i0)
			{
				roadsAlongRiver[i0].Destroy();
			}
			roadsAlongRiver.Clear();
		}

		void GenerateGridRoad()
		{
			parallelRoads.Clear();
			Vector3 roadStart = roadPoints[0];
			Vector3 roadEnd = roadPoints[roadPoints.Count - 1];
			Vector3 distance = roadEnd - roadStart;
			float roadAngle = Mathf.Atan2(distance.x, distance.z) * Mathf.Rad2Deg;
			Quaternion rotation = Quaternion.Euler(0, roadAngle, 0);
			float length = distance.magnitude;
			float totalWidth = 0;
			var parallelRoadPoints = new List<Vector3>();
			float outermostX = 0;
			Quaternion reverse = Quaternion.Euler(0, -roadAngle, 0);
			for (int i0 = 0; i0 < roadPoints.Count; ++i0)
			{
				Vector3 point = reverse * (roadPoints[i0] - roadStart);
				if (point.x > outermostX)
				{
					outermostX = point.x;
				}
			}
			while (totalWidth + roadWidth + roadSpacing < length)
			{
				parallelRoadPoints.Add(new Vector3(outermostX + roadSpacing, 0, totalWidth + roadWidth / 2));
				totalWidth += roadWidth + roadSpacing;
			}
			if (totalWidth + roadWidth < length)
			{
				parallelRoadPoints.Add(new Vector3(outermostX + roadSpacing, 0, totalWidth + roadWidth / 2));
				totalWidth += roadWidth;
			}
			else
			{
				totalWidth -= roadSpacing;
			}

			float offsetZ = (length - totalWidth) / 2;
			for (int i0 = 0; i0 < parallelRoadPoints.Count; ++i0)
			{
				Vector3 point = parallelRoadPoints[i0];
				point.z += offsetZ;
				point = rotation * point;
				point += roadPoints[0];
				parallelRoadPoints[i0] = point;
			}

			parallelRoads.Add(parallelRoadPoints);
			Vector3 offset = rotation * Vector3.right * (roadSpacing + roadWidth);
			for (int i0 = 1; i0 < numberParallelRoad; ++i0)
			{
				var points = new List<Vector3>(parallelRoadPoints);
				for (int i1 = 0; i1 < points.Count; ++i1)
				{
					points[i1] += offset * i0;
				}
				parallelRoads.Add(points);
			}

			var verticalRoads = new List<List<Vector3>>();
			for (int i0 = 0; i0 < parallelRoads.Count; ++i0)
			{
				List<Vector3> points = parallelRoads[i0];
				for (int i1 = 0; i1 < points.Count; ++i1)
				{
					if (i1 >= verticalRoads.Count)
					{
						verticalRoads.Add(new List<Vector3>());
					}
					List<Vector3> vpoints = verticalRoads[i1];
					vpoints.Add(points[i1]);
					if (i0 == parallelRoads.Count - 1)
					{
						int lastIndex = vpoints.Count - 1;
						Vector3 dist = vpoints[lastIndex] - vpoints[0];
						float x = dist.magnitude + roadWidth / 2;
						var pos = rotation * new Vector3(x, 0, 0);
						pos += vpoints[0];
						vpoints[lastIndex] = pos;
					}
				}
			}

			//++++++++++++++++++++++++++++++++++

			var leadPoints = new List<Vector3>();
			for (int i0 = 0; i0 < verticalRoads.Count; ++i0)
			{
				Vector3 p1 = verticalRoads[i0][0];
				Vector3 p2 = verticalRoads[i0][1];
				float a1 = (p2.z - p1.z) / (p2.x - p1.x);
				float b1 = p1.z - a1 * p1.x;
				for (int i1 = 0; i1 < roadPoints.Count - 1; ++i1)
				{
					Vector3 p3 = roadPoints[i1];
					Vector3 p4 = roadPoints[i1 + 1];
					float a2 = (p4.z - p3.z) / (p4.x - p3.x);
					float b2 = p3.z - a2 * p3.x;

					float x = (b2 - b1) / (a1 - a2);
					float minX = p3.x <= p4.x ? p3.x : p4.x;
					float maxX = p3.x > p4.x ? p3.x : p4.x;
					if (minX <= x && x <= maxX)
					{
						float z = a1 * x + b1;
						leadPoints.Add(new Vector3(x, 0, z));
						break;
					}
				}
			}

			for (int i0 = 0; i0 < leadPoints.Count; ++i0)
			{
				verticalRoads[i0].Insert(0, leadPoints[i0]);
			}

			//++++++++++++++++++++++++++++++++++

			for (int i0 = 0; i0 < gridRoadObjects.Count; ++i0)
			{
				Destroy(gridRoadObjects[i0]);
			}
			gridRoadObjects.Clear();

			for (int i0 = 0; i0 < parallelRoads.Count; ++i0)
			{
				GameObject obj = Instantiate(roadPrefab, transform);
				obj.name = $"Parallel_{i0}";
				MeshFilter meshFilterr = obj.GetComponent<MeshFilter>();
				meshFilterr.sharedMesh = MeshCreator.CreateLineMesh(parallelRoads[i0], roadWidth);
				gridRoadObjects.Add(obj);
			}
			for (int i0 = 0; i0 < verticalRoads.Count; ++i0)
			{
				GameObject obj = Instantiate(roadPrefab, transform);
				obj.name = $"Vertical_{i0}";
				MeshFilter meshFilterr = obj.GetComponent<MeshFilter>();
				meshFilterr.sharedMesh = MeshCreator.CreateLineMesh(verticalRoads[i0], roadWidth);
				gridRoadObjects.Add(obj);
			}

			//--------------------------------------------------------------------------------------

			for (int i0 = 0; i0 < parallelRoads.Count; ++i0)
			{
				if (i0 < placers.Count)
				{
					placers[i0].PlaceObjects(prefab, parallelRoads[i0]);
				}
				else
				{
					var placer = new ObjectPlacer($"ParallelRoadPoints_{i0}");
					placer.PlaceObjects(prefab, parallelRoads[i0]); 
					placers.Add(placer);
				}
			}

			if (placers.Count > parallelRoads.Count)
			{
				for (int i0 = placers.Count - 1; i0 >= parallelRoads.Count; --i0)
				{
					placers[i0].Clear();
					placers.RemoveAt(i0);
				}
			}
		}

		public List<List<Vector3>> GetRoadIntersections()
		{
			return roadIntersections;
		}

		[SerializeField]
		GameObject prefab = default;
		[SerializeField]
		GameObject riverPrefab = default;
		[SerializeField]
		GameObject roadPrefab = default;
		[SerializeField]
		int seed = 0;
		[SerializeField]
		RiverPoint[] riverBasePoints = default;
		[SerializeField]
		int numberOfPoint = 10;
		[SerializeField]
		float angleRange = 60;
		[SerializeField]
		float riverWidth = 10;

		[SerializeField]
		float roadWidth = 4;
		[SerializeField]
		float distanceFromRiverToRoad = 2;
		[SerializeField]
		float roadSpacing = 8;
		[SerializeField]
		int numberParallelRoad = 3;




		System.Random random;

		List<River> rivers = new List<River>();
		List<Vector3> riverPoints = new List<Vector3>();

		List<Road> roadsAlongRiver = new List<Road>();
		List<Vector3> roadPoints = new List<Vector3>();

		List<GameObject> gridRoadObjects = new List<GameObject>();
		List<List<Vector3>> parallelRoads = new List<List<Vector3>>();

		List<List<Vector3>> roadIntersections = new List<List<Vector3>>();

		ObjectPlacer riverPointsPlacer;
		ObjectPlacer roadPointsPlacer;
		List<ObjectPlacer> placers = new List<ObjectPlacer>();
	}
}
