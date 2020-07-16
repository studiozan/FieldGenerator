using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class TownGenerator : MonoBehaviour
	{
		public void Initialize()
		{
			random = new System.Random(seed);

			connection.Initialize();

			riverPointsPlacer = new ObjectPlacer("RiverPoints");
			districtRoadPointPlacer = new ObjectPlacer("DistrictRoadPoints");
			roadAlongRiverPointPlacer = new ObjectPlacer("RoadAlongRiverPointPlacer");
			gridRoadPointPlacer = new ObjectPlacer("GridRoadPointPlacer");
		}

		public IEnumerator GenerateTown()
		{
			fieldPoints.Clear();

			GenerateRiver();
			GenerateRoad();

			float interval = Mathf.Max(roadWidth + roadSpacing, riverStepSize);
			connection.FieldConnectCreate(fieldPoints, interval);

			riverPointsPlacer.PlaceObjects(riverPointPrefab, river.Points);
			districtRoadPointPlacer.PlaceObjects(districtRoadPointPrefab, road.DistrictRoadPoints);
			roadAlongRiverPointPlacer.PlaceObjects(roadAlongRiverPointPrefab, road.RoadAlongRiverPoints);
			gridRoadPointPlacer.PlaceObjects(gridRoadPointPrefab, road.GridRoadPoints);

			DetectSurroundedArea();

			OnGenerate?.Invoke(this);

			yield return null;
		}

		void GenerateRiver()
		{
			var parameter = new RiverParameter
			{
				ChunkSize = chunkSize,
				NumberOfChunk = numberOfChunk,
				HeadwaterIsOutside = headwaterIsOutside,
				Width = riverWidth,
				AngleRange = angleRange,
				StepSize = riverStepSize,
				BranchingProbability = riverBranchingProb,
				MinNumStepToBranch = minNumStepToBranch,
				BendabilityAttenuation = bendabilityAttenuation,
			};

			river.Generate(parameter, random);
			fieldPoints.AddRange(river.Points);
		}

		void GenerateRoad()
		{
			var parameter = new RoadParameter
			{
				NumberOfChunk = numberOfChunk,
				ChunkSize = chunkSize,
				Width = roadWidth,
				DistanceFromRiver = distanceFromRiver,
				Spacing = roadSpacing,
			};

			road.Generate(parameter, river, random);
			fieldPoints.AddRange(road.Points);
		}

		void DetectSurroundedArea()
		{
			List<FieldConnectPoint> roadConnectPoints = connection.GetRoadConnectPointList();
			for (int i0 = 0; i0 < roadConnectPoints.Count; ++i0)
			{
				roadConnectPoints[i0].Index = i0;
			}

			for (int i0 = 0; i0 < roadConnectPoints.Count; ++i0)
			{
				FieldConnectPoint point = roadConnectPoints[i0];
				List<FieldConnectPoint> connectPoints = point.ConnectionList;
				for (int i1 = 0; i1 < connectPoints.Count; ++i1)
				{
					var areaPoints = new List<Vector3>();
					if (TryDetectSurroundedAreaRecursive(connectPoints[i1], areaPoints, point.Index, point.Index, 3) != false)
					{
						areaPoints.Add(point.Position);
						Vector3 dir1 = areaPoints[1] - areaPoints[0];
						Vector3 dir2 = areaPoints[2] - areaPoints[1];
						if (Vector3.Cross(dir1, dir2).y < 0)
						{
							areaPoints.Reverse();
						}
						areas.Add(new SurroundedArea { AreaPoints = areaPoints });
					}
				}
			}
		}

		bool TryDetectSurroundedAreaRecursive(FieldConnectPoint point, List<Vector3> areaPoints, int targetIndex, int prevIndex, int count)
		{
			if (count > 0)
			{
				List<FieldConnectPoint> connectPoints = point.ConnectionList;
				for (int i0 = 0; i0 < connectPoints.Count; ++i0)
				{
					FieldConnectPoint connectPoint = connectPoints[i0];
					int connectIndex = connectPoint.Index;
					if (connectIndex != prevIndex)
					{
						if (connectIndex == targetIndex)
						{
							areaPoints.Add(point.Position);
							return true;
						}

						if (TryDetectSurroundedAreaRecursive(connectPoint, areaPoints, targetIndex, point.Index, count - 1) != false)
						{
							areaPoints.Add(point.Position);
							return true;
						}
					}
				}
			}

			return false;
		}

		float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
		{
			Vector3 ab = b - a;
			Vector3 av = value - a;
			return Vector3.Dot(av, ab) / Vector3.Dot(ab, ab);
		}

		public List<FieldPoint> GetFieldPoints()
		{
			return fieldPoints;
		}

		public List<FieldConnectPoint> GetRiverConnectPointList()
		{
			return connection.GetRiverConnectPointList();
		}

		public List<FieldConnectPoint> GetRoadConnectPointList()
		{
			return connection.GetRoadConnectPointList();
		}

		public List<FieldConnectPoint> GetSugorokuConnectPointList()
		{
			return connection.GetSugorokuConnectPointList();
		}

		public float RiverWidth
		{
			get => riverWidth;
		}

		public float RoadWidth
		{
			get => roadWidth;
		}

		public List<SurroundedArea> SurroundedAreas
		{
			get => areas;
		}

		public event System.Action<TownGenerator> OnGenerate;



		[SerializeField]
		GameObject riverPointPrefab = default;
		[SerializeField]
		GameObject districtRoadPointPrefab = default;
		[SerializeField]
		GameObject roadAlongRiverPointPrefab = default;
		[SerializeField]
		GameObject gridRoadPointPrefab = default;

		[SerializeField]
		int seed = 0;
		[SerializeField]
		float chunkSize = 100;
		[SerializeField]
		Vector2Int numberOfChunk = new Vector2Int(10, 10);
		[SerializeField]
		bool headwaterIsOutside = true;
		[SerializeField]
		float riverWidth = 10;
		[SerializeField]
		float angleRange = 60;
		[SerializeField]
		float riverBranchingProb = 1.0f;
		[SerializeField]
		int minNumStepToBranch = 10;
		[SerializeField]
		float bendabilityAttenuation = 0.01f;

		[SerializeField]
		float roadWidth = 4;
		[SerializeField]
		float distanceFromRiver = 2;
		[SerializeField]
		float roadSpacing = 20;



		System.Random random;

		River river = new River();
		float riverStepSize = 10;


		Road road = new Road();


		List<FieldPoint> fieldPoints = new List<FieldPoint>();

		PointConnection connection = new PointConnection();

		ObjectPlacer riverPointsPlacer;
		ObjectPlacer districtRoadPointPlacer;
		ObjectPlacer roadAlongRiverPointPlacer;
		ObjectPlacer gridRoadPointPlacer;

		List<SurroundedArea> areas = new List<SurroundedArea>();
	}
}
