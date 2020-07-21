using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	[System.Serializable]
	public class FieldPointGenerator
	{
		public void Initialize(FieldPointParameter parameter)
		{
			this.parameter = parameter;

			random = new System.Random(parameter.seed);

			connection.Initialize();
		}

		public IEnumerator Generate()
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
					var areaPoints = new List<FieldConnectPoint>();
					if (TryDetectSurroundedAreaRecursive(connectPoints[i1], areaPoints, point.Index, point.Index, 3) != false)
					{
						areaPoints.Add(point);
						if (IsExistCombination(areaPoints) == false)
						{
							AddCombination(areaPoints);
							List<Vector3> innerPoints = InnerArea(areaPoints, parameter.roadWidth * 1.5f);
							Vector3 dir1 = innerPoints[1] - innerPoints[0];
							Vector3 dir2 = innerPoints[2] - innerPoints[1];
							if (Vector3.Cross(dir1, dir2).y < 0)
							{
								innerPoints.Reverse();
							}
							areas.Add(new SurroundedArea { AreaPoints = innerPoints });
						}
					}
				}
			}
		}

		bool TryDetectSurroundedAreaRecursive(FieldConnectPoint point, List<FieldConnectPoint> areaPoints, int targetIndex, int prevIndex, int count)
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
							areaPoints.Add(point);
							return true;
						}

						if (TryDetectSurroundedAreaRecursive(connectPoint, areaPoints, targetIndex, point.Index, count - 1) != false)
						{
							areaPoints.Add(point);
							return true;
						}
					}
				}
			}

			return false;
		}

		Vector3 CalcCenter(List<FieldConnectPoint> points)
		{
			Vector3 center = Vector3.zero;
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				center += points[i0].Position;
			}
			center /= points.Count;

			return center;
		}

		List<Vector3> InnerArea(List<FieldConnectPoint> areaPoints, float width)
		{
			var innerPoints = new List<Vector3>();

			Vector3 center = CalcCenter(areaPoints);

			for (int i0 = 0; i0 < areaPoints.Count; ++i0)
			{
				Vector3 point = areaPoints[i0].Position;
				Vector3 dir = center - point;
				dir.Normalize();
				dir *= width;
				innerPoints.Add(point + dir);
			}

			return innerPoints;
		}

		bool IsExistCombination(List<FieldConnectPoint> points)
		{
			bool isExist = true;;
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				isExist = true;
				FieldConnectPoint point = points[i0];
				HashSet<int> comb;
				if (combination.TryGetValue(point.Index, out comb) != false)
				{
					for (int i1 = 0; i1 < points.Count; ++i1)
					{
						if (i1 != i0)
						{
							FieldConnectPoint p = points[i1];
							if (comb.Contains(p.Index) == false)
							{
								isExist = false;
								break;
							}
						}
					}

					if (isExist != false)
					{
						break;
					}
				}
				else
				{
					isExist = false;
				}
			}

			return isExist;
		}

		void AddCombination(List<FieldConnectPoint> points)
		{
			int key = points[0].Index;
			if (combination.TryGetValue(key, out HashSet<int> set) == false)
			{
				set = new HashSet<int>();
				combination.Add(key, set);
			}

			for (int i0 = 1; i0 < points.Count; ++i0)
			{
				set.Add(points[i0].Index);
			}
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

		public event System.Action<FieldPointGenerator> OnGenerate;



		System.Random random;

		FieldPointParameter parameter;

		River river = new River();
		float riverStepSize = 10;


		Road road = new Road();


		List<FieldPoint> fieldPoints = new List<FieldPoint>();

		PointConnection connection = new PointConnection();

		List<SurroundedArea> areas = new List<SurroundedArea>();

		Dictionary<int, HashSet<int>> combination = new Dictionary<int, HashSet<int>>();
	}
}
