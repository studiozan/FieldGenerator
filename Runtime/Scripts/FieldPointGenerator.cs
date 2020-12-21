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

			DetectSquareArea(roadConnectedPoints);
			DetectPentagonArea(roadConnectedPoints);
		}

		void DetectSquareArea(List<FieldConnectPoint> roadPoints)
		{
			for (int i0 = 0; i0 < roadPoints.Count; ++i0)
			{
				FieldConnectPoint point = roadPoints[i0];
				List<FieldConnectPoint> connectedPoints = point.ConnectionList;

				for (int i1 = 0; i1 < connectedPoints.Count; ++i1)
				{
					FieldConnectPoint connectedPoint = connectedPoints[i1];
					List<FieldConnectPoint> candidates = connectedPoint.ConnectionList;
					for (int i2 = i1 + 1; i2 < connectedPoints.Count; ++i2)
					{
						FieldConnectPoint connectedPoint2 = connectedPoints[i2];
						List<FieldConnectPoint> candidates2 = connectedPoint2.ConnectionList;
						for (int i3 = 0; i3 < candidates.Count; ++i3)
						{
							FieldConnectPoint candidate = candidates[i3];
							if (candidate.Index != point.Index)
							{
								if (candidates2.Contains(candidate) != false)
								{
									var areaPoints = new List<FieldConnectPoint>
									{
										point, connectedPoint, candidate, connectedPoint2,
									};
									if (IsExistCombination(areaPoints) == false)
									{
										AddCombination(areaPoints);
										int disconnectionIndex = candidates.FindIndex(p => p.Index == connectedPoint2.Index);
										if (disconnectionIndex >= 0)
										{
											connectedPoint.Disconnection(disconnectionIndex);
											disconnectionIndex = candidates2.FindIndex(p => p.Index == connectedPoint.Index);
											if (disconnectionIndex >= 0)
											{
												connectedPoint2.Disconnection(disconnectionIndex);
											}
										}
										disconnectionIndex = connectedPoints.FindIndex(p => p.Index == candidate.Index);
										if (disconnectionIndex >= 0)
										{
											point.Disconnection(disconnectionIndex);
											disconnectionIndex = candidate.ConnectionList.FindIndex(p => p.Index == point.Index);
											if (disconnectionIndex >= 0)
											{
												candidate.Disconnection(disconnectionIndex);
											}
										}

										AddArea(areaPoints);
									}
									break;
								}
							}
						}
					}
				}
			}
		}

		void DetectPentagonArea(List<FieldConnectPoint> roadPoints)
		{
			for (int i0 = 0; i0 < roadPoints.Count; ++i0)
			{
				FieldConnectPoint point = roadPoints[i0];
				List<FieldConnectPoint> connectedPoints = point.ConnectionList;

				for (int i1 = 0; i1 < connectedPoints.Count; ++i1)
				{
					FieldConnectPoint connectedPoint = connectedPoints[i1];
					List<FieldConnectPoint> candidates = connectedPoint.ConnectionList;
					for (int i2 = i1 + 1; i2 < connectedPoints.Count; ++i2)
					{
						FieldConnectPoint connectedPoint2 = connectedPoints[i2];
						List<FieldConnectPoint> candidates2 = connectedPoint2.ConnectionList;
						if (candidates.Contains(connectedPoint2) == false)
						{
							bool detected = false;
							for (int i3 = 0; i3 < candidates.Count; ++i3)
							{
								FieldConnectPoint candidate = candidates[i3];
								if (candidate.Index != point.Index)
								{
									for (int i4 = 0; i4 < candidate.ConnectionList.Count; ++i4)
									{
										FieldConnectPoint candidate2 = candidate.ConnectionList[i4];
										if (candidate2.Index != point.Index)
										{
											if (candidates2.Contains(candidate2) != false)
											{
												if (IsExistCombination(new[] { point, connectedPoint, candidate, candidate2 }) == false &&
													IsExistCombination(new[] { point, connectedPoint, candidate, connectedPoint2 }) == false &&
													IsExistCombination(new[] { point, connectedPoint, candidate2, connectedPoint2 }) == false &&
													IsExistCombination(new[] { point, candidate, candidate2, connectedPoint2 }) == false &&
													IsExistCombination(new[] { connectedPoint, candidate, candidate2, connectedPoint2 }) == false)
												{
													var pentagonAreaPoints = new List<FieldConnectPoint>() { point, connectedPoint, candidate, candidate2, connectedPoint2 };
													if (IsExistCombination(pentagonAreaPoints) == false)
													{
														AddCombination(pentagonAreaPoints);
														List<FieldConnectPoint> squarePoints = ConvertPentagonToSquare(pentagonAreaPoints);
														AddArea(squarePoints);
														detected = true;
														break;
													}
												}
											}
										}
									}
									if (detected != false)
									{
										break;
									}
								}
							}
						}
					}
				}
			}
		}

		void AddArea(List<FieldConnectPoint> squarePoints)
		{
			float amountInwardMovement = Mathf.Lerp(parameter.minAmountInwardMovement, parameter.maxAmountInwardMovement, (float)random.NextDouble());
			List<Vector3> areaPoints = InnerArea(squarePoints, amountInwardMovement);
			Vector3 center = CalcCenter(areaPoints);
			areaPoints = SortClockwise(center, areaPoints, 0);
			areas.Add(new SurroundedArea { AreaPoints = areaPoints });
		}

		List<FieldConnectPoint> ConvertPentagonToSquare(IReadOnlyList<FieldConnectPoint> pentagonPoints)
		{
			float minDiagonalLength = float.MaxValue;
			int minDiagonalIndex = 0;
			for (int i0 = 0; i0 < pentagonPoints.Count; ++i0)
			{
				Vector3 start = pentagonPoints[i0].Position;
				Vector3 end = pentagonPoints[(i0 + 2) % pentagonPoints.Count].Position;
				float length = (end - start).sqrMagnitude;
				if (length < minDiagonalLength)
				{
					minDiagonalLength = length;
					minDiagonalIndex = (i0 + 1) % pentagonPoints.Count;
				}
			}

			var squarePoints = new List<FieldConnectPoint>();
			for (int i0 = 0; i0 < pentagonPoints.Count; ++i0)
			{
				if (i0 != minDiagonalIndex)
				{
					squarePoints.Add(pentagonPoints[i0]);
				}
			}

			return squarePoints;
		}

		List<Vector3> SortClockwise(Vector3 originPos, List<Vector3> points, int baseIndex)
		{
			var clockwise = new List<Vector3>();

			var angles = new List<float>();
			Vector3 basePoint= points[baseIndex];
			clockwise.Add(basePoint);
			Vector3 baseDir = basePoint - originPos;
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				if (i0 != baseIndex)
				{
					Vector3 point = points[i0];
					Vector3 dir = point - originPos;
					float angle = -Vector2.SignedAngle(new Vector2(baseDir.x, baseDir.z), new Vector2(dir.x, dir.z));
					if (angle < 0)
					{
						angle += 360;
					}
					int insertIndex = 0;
					while (insertIndex < angles.Count)
					{
						if (angle < angles[insertIndex])
						{
							break;
						}

						++insertIndex;
					}
					angles.Insert(insertIndex, angle);
					clockwise.Insert(insertIndex + 1, point);
				}
			}

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

		bool IsExistCombination(IReadOnlyList<FieldConnectPoint> points)
		{
			bool isExist = true;
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

		public RiverGenerator River
		{
			get => river;
		}

		public RoadGenerator Road
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

		RiverGenerator river = new RiverGenerator();
		RoadGenerator road = new RoadGenerator();

		List<FieldPoint> fieldPoints = new List<FieldPoint>();
		PointConnection connection = new PointConnection();
		List<SurroundedArea> areas = new List<SurroundedArea>();
		Dictionary<int, HashSet<int>> combination = new Dictionary<int, HashSet<int>>();

		float maxRoadWidth;
	}
}
