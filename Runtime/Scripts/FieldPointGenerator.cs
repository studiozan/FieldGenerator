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
			fieldPoints.Clear();

			yield return CoroutineUtility.CoroutineCycle( GenerateRiver());
			yield return CoroutineUtility.CoroutineCycle( GenerateRoad());

			connection.FieldConnectCreate(
				fieldPoints,
				maxRoadWidth + parameter.roadSpacing,
				new Vector3( parameter.chunkSize * parameter.numberOfChunk.x, 0f, parameter.chunkSize * parameter.numberOfChunk.y),
				parameter.riverStepSize,
				parameter.sugorokuMergeMulti,
				parameter.sugorokuOffset);

			yield return CoroutineUtility.CoroutineCycle( DetectSurroundedArea());

			OnGenerate?.Invoke(this);
			
			combination.Clear();
		}

		IEnumerator GenerateRiver()
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

			yield return CoroutineUtility.CoroutineCycle( river.Generate(param, random));
			fieldPoints.AddRange(river.Points);
		}

		IEnumerator GenerateRoad()
		{
			var param = new RoadParameter
			{
				NumberOfChunk = parameter.numberOfChunk,
				ChunkSize = parameter.chunkSize,
				Width = maxRoadWidth,
				DistanceFromRiver = parameter.distanceFromRiver,
				Spacing = parameter.roadSpacing,
			};

			yield return CoroutineUtility.CoroutineCycle( road.Generate(param, river, random));
			fieldPoints.AddRange(road.Points);
		}

		IEnumerator DetectSurroundedArea()
		{
			lastInterruptionTime = System.DateTime.Now;

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
					areaPoints.Add(point);
					FieldConnectPoint connectPoint = connectPoints[i1];
					if (TryDetectSurroundedAreaRecursive(connectPoint, areaPoints, point.Index, point.Index, 4) != false)
					{
						if (IsExistCombination(areaPoints) == false)
						{
							AddCombination(areaPoints);
							float amountInwardMovement = Mathf.Lerp(parameter.minAmountInwardMovement, parameter.maxAmountInwardMovement, (float)random.NextDouble());
							List<Vector3> innerPoints = InnerArea(areaPoints, amountInwardMovement);
							Vector3 dir1 = innerPoints[1] - innerPoints[0];
							Vector3 dir2 = innerPoints[2] - innerPoints[1];
							if (Vector3.Cross(dir1, dir2).y < 0)
							{
								innerPoints.Reverse();
							}
							areas.Add(new SurroundedArea { AreaPoints = innerPoints });
						}
					}

					if (System.DateTime.Now.Subtract(lastInterruptionTime).TotalMilliseconds >= kElapsedTimeToInterrupt)
					{
						yield return null;
						lastInterruptionTime = System.DateTime.Now;
					}
				}
			}
		}

		bool TryDetectSurroundedAreaRecursive(FieldConnectPoint point, List<FieldConnectPoint> areaPoints, int targetIndex, int prevIndex, int count)
		{
			bool wasDetected = false;

			--count;
			if (count > 0)
			{
				areaPoints.Add(point);
				List<FieldConnectPoint> connectPoints = point.ConnectionList;
				int baseIndex = connectPoints.FindIndex(p => p.Index == prevIndex);
				List<int> clockwiseIndices = GetClockwiseIndices(point, connectPoints, baseIndex);
				if (clockwiseIndices.Count >= 2)
				{
					FieldConnectPoint nextPoint = connectPoints[clockwiseIndices[clockwiseIndices.Count - 1]];
					if (nextPoint.Index == targetIndex)
					{
						wasDetected = true;
					}
					else
					{
						wasDetected = TryDetectSurroundedAreaRecursive(nextPoint, areaPoints, targetIndex, point.Index, count);
					}
				}
			}

			return wasDetected;
		}

		List<int> GetClockwiseIndices(FieldConnectPoint origin, List<FieldConnectPoint> points, int baseIndex)
		{
			var clockwise = new List<int>();

			var rightUp = new List<KeyValuePair<int, float>>();
			var rightDown = new List<KeyValuePair<int, float>>();
			var leftDown = new List<KeyValuePair<int, float>>();
			var leftUp = new List<KeyValuePair<int, float>>();

			Vector3 baseVec = points[baseIndex].Position - origin.Position;
			baseVec.Normalize();
			var right = new Vector3(baseVec.z, 0, -baseVec.x);
			var left = new Vector3(-baseVec.z, 0, baseVec.x);
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				if (i0 != baseIndex)
				{
					Vector3 v = points[i0].Position - origin.Position;
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

			clockwise.Add(baseIndex);
			clockwise.AddRange(rightUp.ConvertAll<int>(pair => pair.Key));
			clockwise.AddRange(rightDown.ConvertAll<int>(pair => pair.Key));
			clockwise.AddRange(leftDown.ConvertAll<int>(pair => pair.Key));
			clockwise.AddRange(leftUp.ConvertAll<int>(pair => pair.Key));

			return clockwise;
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
