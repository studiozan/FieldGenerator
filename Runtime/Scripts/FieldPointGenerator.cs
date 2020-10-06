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
					FieldConnectPoint connectedPoint = connectedPoints[i1];
					List<FieldConnectPoint> connectedPoints2 = connectedPoint.ConnectionList;

					for (int i2 = i1 + 1; i2 < connectedPoints.Count; ++i2)
					{
						FieldConnectPoint connectedPoint2 = connectedPoints[i2];
						List<FieldConnectPoint> connectedPoints3 = connectedPoint2.ConnectionList;
						FieldConnectPoint connectedPoint3 = connectedPoints3.Find(p => connectedPoints2.Find(p2 => p2.Index != point.Index && p2.Index == p.Index) != null);
						if (connectedPoint3 != null)
						{
							var areaPoints = new List<FieldConnectPoint>
							{
								point, connectedPoint, connectedPoint2, connectedPoint3,
							};

							if (IsExistCombination(areaPoints) == false)
							{
								AddCombination(areaPoints);
								Vector3 center = CalcCenter(areaPoints);
								List<FieldConnectPoint> clockwise = SortClockwise(center, areaPoints, 0);
								if (clockwise[0].ConnectionList.Contains(clockwise[2]) == false &&
									clockwise[1].ConnectionList.Contains(clockwise[3]) == false)
								{
									float amountInwardMovement = Mathf.Lerp(parameter.minAmountInwardMovement, parameter.maxAmountInwardMovement, (float)random.NextDouble());
									List<Vector3> innerPoints = InnerArea(areaPoints, amountInwardMovement);
									Vector3 center2 = CalcCenter(innerPoints);
									List<Vector3> clockwise2 = SortClockwise(center2, innerPoints, 0);
									areas.Add(new SurroundedArea { AreaPoints = clockwise2 });
								}
							}
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
