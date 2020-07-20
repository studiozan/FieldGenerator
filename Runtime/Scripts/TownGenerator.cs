using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	[System.Serializable]
	public class TownGenerator
	{
		public void Initialize(TownParameter parameter)
		{
			this.parameter = parameter;

			random = new System.Random(parameter.seed);

			connection.Initialize();
		}

		public IEnumerator GenerateTown()
		{
			fieldPoints.Clear();

			GenerateRiver();
			GenerateRoad();

			connection.FieldConnectCreate(fieldPoints, parameter.roadWidth + parameter.roadSpacing, riverStepSize);

			DetectSurroundedArea();

			OnGenerate?.Invoke(this);

			yield break;
		}

		void GenerateRiver()
		{
			var param = new RiverParameter
			{
				ChunkSize = parameter.chunkSize,
				NumberOfChunk = parameter.numberOfChunk,
				HeadwaterIsOutside = parameter.headwaterIsOutside,
				Width = parameter.riverWidth,
				AngleRange = parameter.angleRange,
				StepSize = riverStepSize,
				BranchingProbability = parameter.branchingProbability,
				MinNumStepToBranch = parameter.minNumStepToBranch,
				BendabilityAttenuation = parameter.bendabilityAttenuation,
			};

			river.Generate(param, random);
			fieldPoints.AddRange(river.Points);
		}

		void GenerateRoad()
		{
			var param = new RoadParameter
			{
				NumberOfChunk = parameter.numberOfChunk,
				ChunkSize = parameter.chunkSize,
				Width = parameter.roadWidth,
				DistanceFromRiver = parameter.distanceFromRiver,
				Spacing = parameter.roadSpacing,
			};

			road.Generate(param, river, random);
			fieldPoints.AddRange(road.Points);
		}

		void DetectSurroundedArea()
		{
			areas.Clear();
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
						areaPoints = InnerArea(areaPoints, parameter.roadWidth * 1.5f);
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

		List<Vector3> InnerArea(List<Vector3> areaPoints, float width)
		{
			var innerPoints = new List<Vector3>();

			Vector3 center = Vector3.zero;
			for (int i0 = 0; i0 < areaPoints.Count; ++i0)
			{
				center += areaPoints[i0];
			}
			center /= areaPoints.Count;

			for (int i0 = 0; i0 < areaPoints.Count; ++i0)
			{
				Vector3 point = areaPoints[i0];
				Vector3 dir = center - point;
				dir.Normalize();
				dir *= width;
				innerPoints.Add(point + dir);
			}

			return innerPoints;
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

		public River River
		{
			get => river;
		}

		public Road Road
		{
			get => road;
		}

		public float RiverWidth
		{
			get => parameter.riverWidth;
		}

		public float RoadWidth
		{
			get => parameter.roadWidth;
		}

		public List<SurroundedArea> SurroundedAreas
		{
			get => areas;
		}

		public event System.Action<TownGenerator> OnGenerate;



		System.Random random;

		TownParameter parameter;

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
