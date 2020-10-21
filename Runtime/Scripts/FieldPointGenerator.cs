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

			maxRoadWidth = 0;
			WeightedValue[] widthCandidaates = parameter.roadWidthCandidates;
			for (int i0 = 0; i0 < widthCandidaates.Length; ++i0)
			{
				float width = widthCandidaates[i0].value;
				if (width > maxRoadWidth)
				{
					maxRoadWidth = width;
				}
			}

			connection.Initialize();
		}

		public IEnumerator Generate()
		{
			lastInterruptionTime = System.DateTime.Now;
			fieldPoints.Clear();
			combination.Clear();

			GenerateRiver();
			GenerateRoad();

			connection.FieldConnectCreate(
				fieldPoints,
				maxRoadWidth + parameter.roadSpacing,
				parameter.chunkSize,
				parameter.numberOfChunk,
				parameter.riverStepSize,
				parameter.sugorokuMergeMulti,
				parameter.sugorokuOffset);

			DetectSurroundedAreas();

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
				MinInitialWidth = parameter.minInitialRiverWidth,
				MaxInitialWidth = parameter.maxInitialRiverWidth,
				AngleRange = parameter.angleRange,
				StepSize = parameter.riverStepSize,
				MinInitialBranchingProbability = parameter.minInitialBranchingProbability,
				MaxInitialBranchingProbability = parameter.maxInitialBranchingProbability,
				MinNumStepWithoutBranching = parameter.minNumStepWithoutBranching,
				MaxNumStepWithoutBranching = parameter.maxNumStepWithoutBranching,
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
				Width = maxRoadWidth,
				DistanceFromRiver = parameter.distanceFromRiver,
				Spacing = parameter.roadSpacing,
			};

			road.Generate(param, river, random);
			fieldPoints.AddRange(road.Points);
		}

		void DetectSurroundedAreas()
		{
			areas.Clear();
			List<FieldConnectPoint> roadConnectedPoints = connection.GetRoadConnectPointList();
			for (int i0 = 0; i0 < roadConnectedPoints.Count; ++i0)
			{
				roadConnectedPoints[i0].Index = i0;
			}

			for (int i0 = 0; i0 < roadConnectedPoints.Count; ++i0)
			{
				FieldConnectPoint point = roadConnectedPoints[i0];
				List<FieldConnectPoint> connectedPoints = point.ConnectionList;

				for (int i1 = 0; i1 < connectedPoints.Count; ++i1)
				{
					var areaPoints = new List<FieldConnectPoint>();
					areaPoints.Add(point);
					FieldConnectPoint currentPoint = connectedPoints[i1];
					areaPoints.Add(currentPoint);
					FieldConnectPoint prevPoint = point;
					bool foundArea = false;
					for (int i2 = 0; i2 < 3; ++i2)
					{
						FieldConnectPoint nextPoint = GetLastClockwisePoint(currentPoint, currentPoint.ConnectionList.FindIndex(p => p.Index == prevPoint.Index));
						if (nextPoint.Index == prevPoint.Index)
						{
							break;
						}

						if (nextPoint.Index == point.Index)
						{
							foundArea = true;
							break;
						}

						areaPoints.Add(nextPoint);
						prevPoint = currentPoint;
						currentPoint = nextPoint;
					}

					if (foundArea != false && areaPoints.Count == 4)
					{
						if (IsExistCombination(areaPoints) == false)
						{
							AddCombination(areaPoints);
							float amountInwardMovement = Mathf.Lerp(parameter.minAmountInwardMovement, parameter.maxAmountInwardMovement, (float)random.NextDouble());
							List<Vector3> innerPoints = InnerArea(areaPoints, amountInwardMovement);
							Vector3 center = CalcCenter(innerPoints);
							List<Vector3> clockwise = SortClockwise(center, innerPoints, 0);
							areas.Add(new SurroundedArea { AreaPoints = clockwise });
						}
					}
				}
			}
		}

		FieldConnectPoint GetLastClockwisePoint(FieldConnectPoint origin, int baseIndex)
		{
			int index = baseIndex;
			float minCross = float.MaxValue;

			List<FieldConnectPoint> connectedPoints = origin.ConnectionList;
			Vector3 baseVec = (connectedPoints[baseIndex].Position - origin.Position).normalized;
			var left = new Vector3(-baseVec.z, 0, baseVec.x);

			for (int i0 = 0; i0 < connectedPoints.Count; ++i0)
			{
				if (i0 != baseIndex)
				{
					FieldConnectPoint point = connectedPoints[i0];
					Vector3 v = (point.Position - origin.Position).normalized;
					float cross = Vector3.Cross(baseVec, v).y;
					if (cross < 0)
					{
						cross = Mathf.Abs(cross);
						if (Vector3.Cross(left, v).y < 0)
						{
							cross = 2 - cross;
						}
					}
					else
					{
						if (Vector3.Cross(left, v).y < 0)
						{
							cross += 2;
						}
						else
						{
							cross = 4 - cross;
						}
					}

					if (cross < minCross)
					{
						minCross = cross;
						index = i0;
					}
				}
			}

			return connectedPoints[index];
		}

		List<Vector3> SortClockwise(Vector3 originPos, List<Vector3> points, int baseIndex)
		{
			var clockwise = new List<Vector3>();

			var rightUp = new List<KeyValuePair<int, float>>();
			var rightDown = new List<KeyValuePair<int, float>>();
			var leftDown = new List<KeyValuePair<int, float>>();
			var leftUp = new List<KeyValuePair<int, float>>();

			Vector3 baseVec = points[baseIndex] - originPos;
			baseVec.Normalize();
			var right = new Vector3(baseVec.z, 0, -baseVec.x);
			var left = new Vector3(-baseVec.z, 0, baseVec.x);
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				if (i0 != baseIndex)
				{
					Vector3 v = points[i0] - originPos;
					v.Normalize();
					float cross1 = Vector3.Cross(baseVec, v).y;
					//右
					if (cross1 >= 0)
					{
						float cross2 = Vector3.Cross(right, v).y;
						if (cross2 <= 0)
						{
							rightUp.Add(new KeyValuePair<int, float>(i0, cross2));
						}
						else
						{
							rightDown.Add(new KeyValuePair<int, float>(i0, cross2));
						}
					}
					//左
					else
					{
						float cross2 = Vector3.Cross(left, v).y;
						if (cross2 <= 0)
						{
							leftDown.Add(new KeyValuePair<int, float>(i0, cross2));
						}
						else
						{
							leftUp.Add(new KeyValuePair<int, float>(i0, cross2));
						}
					}
				}
			}

			rightUp.Sort((a, b) => a.Value.CompareTo(b.Value));
			rightDown.Sort((a, b) => a.Value.CompareTo(b.Value));
			leftDown.Sort((a, b) => a.Value.CompareTo(b.Value));
			leftUp.Sort((a, b) => a.Value.CompareTo(b.Value));

			clockwise.Add(points[baseIndex]);
			clockwise.AddRange(rightUp.ConvertAll<Vector3>(pair => points[pair.Key]));
			clockwise.AddRange(rightDown.ConvertAll<Vector3>(pair => points[pair.Key]));
			clockwise.AddRange(leftDown.ConvertAll<Vector3>(pair => points[pair.Key]));
			clockwise.AddRange(leftUp.ConvertAll<Vector3>(pair => points[pair.Key]));

			return clockwise;
		}

		List<FieldConnectPoint> SortClockwise(Vector3 originPos, List<FieldConnectPoint> points, int baseIndex)
		{
			var clockwise = new List<FieldConnectPoint>();

			var rightUp = new List<KeyValuePair<int, float>>();
			var rightDown = new List<KeyValuePair<int, float>>();
			var leftDown = new List<KeyValuePair<int, float>>();
			var leftUp = new List<KeyValuePair<int, float>>();

			Vector3 baseVec = points[baseIndex].Position - originPos;
			baseVec.Normalize();
			var right = new Vector3(baseVec.z, 0, -baseVec.x);
			var left = new Vector3(-baseVec.z, 0, baseVec.x);
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				if (i0 != baseIndex)
				{
					Vector3 v = points[i0].Position - originPos;
					v.Normalize();
					float cross1 = Vector3.Cross(baseVec, v).y;
					//右
					if (cross1 >= 0)
					{
						float cross2 = Vector3.Cross(right, v).y;
						if (cross2 <= 0)
						{
							rightUp.Add(new KeyValuePair<int, float>(i0, cross2));
						}
						else
						{
							rightDown.Add(new KeyValuePair<int, float>(i0, cross2));
						}
					}
					//左
					else
					{
						float cross2 = Vector3.Cross(left, v).y;
						if (cross2 <= 0)
						{
							leftDown.Add(new KeyValuePair<int, float>(i0, cross2));
						}
						else
						{
							leftUp.Add(new KeyValuePair<int, float>(i0, cross2));
						}
					}
				}
			}

			rightUp.Sort((a, b) => a.Value.CompareTo(b.Value));
			rightDown.Sort((a, b) => a.Value.CompareTo(b.Value));
			leftDown.Sort((a, b) => a.Value.CompareTo(b.Value));
			leftUp.Sort((a, b) => a.Value.CompareTo(b.Value));

			clockwise.Add(points[baseIndex]);
			clockwise.AddRange(rightUp.ConvertAll<FieldConnectPoint>(pair => points[pair.Key]));
			clockwise.AddRange(rightDown.ConvertAll<FieldConnectPoint>(pair => points[pair.Key]));
			clockwise.AddRange(leftDown.ConvertAll<FieldConnectPoint>(pair => points[pair.Key]));
			clockwise.AddRange(leftUp.ConvertAll<FieldConnectPoint>(pair => points[pair.Key]));

			return clockwise;
		}

		Vector3 CalcCenter(List<Vector3> points)
		{
			Vector3 center = Vector3.zero;
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				center += points[i0];
			}
			center /= points.Count;

			return center;
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

		List<Vector3> InnerArea(List<FieldConnectPoint> areaPoints, float amount)
		{
			var innerPoints = new List<Vector3>();

			Vector3 center = CalcCenter(areaPoints);

			for (int i0 = 0; i0 < areaPoints.Count; ++i0)
			{
				Vector3 point = areaPoints[i0].Position;
				Vector3 dir = center - point;
				dir.Normalize();
				dir *= amount;
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
			// return connection.GetRiverConnectPointList();
			return river.Points;
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
			get => river.Width;
		}

		public float RoadWidth
		{
			get => maxRoadWidth;
		}

		public List<SurroundedArea> SurroundedAreas
		{
			get => areas;
		}

		public event System.Action<FieldPointGenerator> OnGenerate;



		public static readonly float kElapsedTimeToInterrupt = 16.7f;



		System.Random random;
		System.DateTime lastInterruptionTime;
		FieldPointParameter parameter;

		River river = new River();
		Road road = new Road();

		List<FieldPoint> fieldPoints = new List<FieldPoint>();
		PointConnection connection = new PointConnection();
		List<SurroundedArea> areas = new List<SurroundedArea>();
		Dictionary<int, HashSet<int>> combination = new Dictionary<int, HashSet<int>>();

		float maxRoadWidth;
	}
}
