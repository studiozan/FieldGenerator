using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class River
	{
		public IEnumerator Generate(RiverParameter parameter, System.Random random)
		{
			lastInterruptionTime = System.DateTime.Now;

			this.parameter = parameter;
			this.random = random;
			width = Mathf.Lerp(parameter.MinInitialWidth, parameter.MaxInitialWidth, (float)random.NextDouble());
			branchingProbability = CalcBranchingProbability();
			numStepWithoutBranching = CalcNumStepWithoutBranching();

			points.Clear();
			connectedPoints.Clear();
			vertices.Clear();
			pointMap.Clear();

			Vector3 initialPosition = DecideInitialPosition();
			float initialAngle = DecideInitialAngle(initialPosition);

			rootPoint = new RiverPoint();
			rootPoint.Position = initialPosition;
			rootPoint.Width = width;
			var step = new Vector3(0, 0, parameter.StepSize);

			Vector3 initialDir = Quaternion.Euler(0, initialAngle, 0) * step;

			points.Add(ToFieldPoint(rootPoint));
			AddToPointMap(rootPoint);

			minAngleForBranching = Mathf.Atan2(width * 0.5f, parameter.StepSize) * Mathf.Rad2Deg * 2;
			canBranch = parameter.AngleRange >= minAngleForBranching;

			yield return CoroutineUtility.CoroutineCycle( GenerateRiverRecursive(rootPoint, initialDir, 1));
			ConnectPoints();
		}

		IEnumerator GenerateRiverRecursive(RiverPoint riverPoint, Vector3 dir, float bendability)
		{
			RiverPoint currentPoint = riverPoint;
			Vector3 currentPos = currentPoint.Position;
			Vector3 nextDir = dir;
			int numStep = 0;
			int totalStep = 0;
			float bend = bendability;
			float angleRange = parameter.AngleRange;
			float halfWidth = width * 0.5f;
			var step = new Vector3(0, 0, parameter.StepSize);

			while (IsInsideField(currentPos) != false)
			{
				++numStep;
				++totalStep;

				Vector3 nextPos = currentPos + nextDir;
				Vector2Int currentChunk = GetChunk(currentPos);
				Vector2Int nextChunk = GetChunk(nextPos);

				bool isIntersecting = false;
				RiverPoint absorptionPoint = null;
				if (TryGetAbsorptionPoint(currentPos, nextPos, currentChunk, parameter.StepSize * 0.5f, out isIntersecting, out absorptionPoint) != false)
				{
					List<RiverPoint> nextPoints = currentPoint.NextPoints;
					if (nextPoints.Contains(absorptionPoint) == false)
					{
						nextPoints.Add(absorptionPoint);
						points.Add(ToFieldPoint(absorptionPoint));

						Vector3 normDir = (absorptionPoint.Position - currentPos).normalized;
						var leftBase = new Vector3(-normDir.z, 0, normDir.x) * halfWidth;
						var rightBase = new Vector3(normDir.z, 0, -normDir.x) * halfWidth;
						Vector3 left = leftBase + currentPos;
						Vector3 right = rightBase + currentPos;

						vertices.Add(left);
						vertices.Add(right);
					}

					List<RiverPoint> prevPoints = absorptionPoint.PrevPoints;
					if (prevPoints.Contains(currentPoint) == false)
					{
						prevPoints.Add(currentPoint);
					}

					currentPoint = absorptionPoint;
					currentPos = currentPoint.Position;

					if (System.DateTime.Now.Subtract(lastInterruptionTime).TotalMilliseconds >= FieldPointGenerator.kElapsedTimeToInterrupt)
					{
						yield return null;
						lastInterruptionTime = System.DateTime.Now;
					}
				}
				else
				{
					if (currentChunk != nextChunk &&
						TryGetAbsorptionPoint(currentPos, nextPos, nextChunk, parameter.StepSize * 0.5f, out isIntersecting, out absorptionPoint) != false)
					{
						List<RiverPoint> nextPoints = currentPoint.NextPoints;
						if (nextPoints.Contains(absorptionPoint) == false)
						{
							nextPoints.Add(absorptionPoint);
							points.Add(ToFieldPoint(absorptionPoint));

							Vector3 normDir = (absorptionPoint.Position - currentPos).normalized;
							var leftBase = new Vector3(-normDir.z, 0, normDir.x) * halfWidth;
							var rightBase = new Vector3(normDir.z, 0, -normDir.x) * halfWidth;
							Vector3 left = leftBase + currentPos;
							Vector3 right = rightBase + currentPos;

							vertices.Add(left);
							vertices.Add(right);
						}

						List<RiverPoint> prevPoints = absorptionPoint.PrevPoints;
						if (prevPoints.Contains(currentPoint) == false)
						{
							prevPoints.Add(currentPoint);
						}

						currentPoint = absorptionPoint;
						currentPos = currentPoint.Position;

						if (System.DateTime.Now.Subtract(lastInterruptionTime).TotalMilliseconds >= FieldPointGenerator.kElapsedTimeToInterrupt)
						{
							yield return null;
							lastInterruptionTime = System.DateTime.Now;
						}
					}
					else
					{
						var nextPoint = new RiverPoint();
						nextPoint.Position = nextPos;
						nextPoint.Width = width;
						nextPoint.PrevPoints.Add(currentPoint);

						float angle = Mathf.Atan2(nextDir.x, nextDir.z) * Mathf.Rad2Deg;

						Vector3 normDir = nextDir.normalized;
						var leftBase = new Vector3(-normDir.z, 0, normDir.x) * halfWidth;
						var rightBase = new Vector3(normDir.z, 0, -normDir.x) * halfWidth;
						Vector3 left = leftBase + currentPos;
						Vector3 right = rightBase + currentPos;

						if (System.DateTime.Now.Subtract(lastInterruptionTime).TotalMilliseconds >= FieldPointGenerator.kElapsedTimeToInterrupt)
						{
							yield return null;
							lastInterruptionTime = System.DateTime.Now;
						}

						if (canBranch != false && numStep >= numStepWithoutBranching)
						{
							if (DetectFromPercent(branchingProbability) != false)
							{
								numStep = 0;
								branchingProbability = CalcBranchingProbability();
								numStepWithoutBranching = CalcNumStepWithoutBranching();
								float angle2 = angle + Mathf.Lerp(minAngleForBranching, angleRange, (float)random.NextDouble()) * (random.Next(2) == 0 ? -1 : 1);
								Vector3 nextDir2 = Quaternion.Euler(0, angle2, 0) * step;
								yield return CoroutineUtility.CoroutineCycle( GenerateRiverRecursive(currentPoint, nextDir2, bend));
							}
						}

						currentPoint.NextPoints.Add(nextPoint);

						points.Add(ToFieldPoint(nextPoint));
						AddToPointMap(nextPoint);

						vertices.Add(left);
						vertices.Add(right);

						float nextAngle = angle + angleRange * (float)random.NextDouble() - angleRange * 0.5f;
						bend *= (1.0f - parameter.BendabilityAttenuation);
						nextAngle = Mathf.Lerp(angle, nextAngle, bend);
						nextDir = Quaternion.Euler(0, nextAngle, 0) * step;

						currentPoint = nextPoint;
						currentPos = currentPoint.Position;
					}
				}
			}

			if (totalStep != 0)
			{
				Vector3 normDir = nextDir.normalized;
				Vector3 left = new Vector3(-normDir.z, 0, normDir.x) * halfWidth + currentPos;
				Vector3 right = new Vector3(normDir.z, 0, -normDir.x) * halfWidth + currentPos;
				vertices.Add(left);
				vertices.Add(right);
			}
		}

		bool TryGetAbsorptionPoint(Vector3 s1, Vector3 e1, Vector2Int chunk, float distance, out bool isIntersecting, out RiverPoint absorptionPoint)
		{
			bool foundPoint = false;
			isIntersecting = false;
			absorptionPoint = null;

			var nearestPoint = new KeyValuePair<float, RiverPoint>(float.MaxValue, null);

			if (pointMap.TryGetValue(chunk, out List<RiverPoint> pointsInChunk) != false)
			{
				for (int i0 = 0; i0 < pointsInChunk.Count; ++i0)
				{
					RiverPoint currentPoint = pointsInChunk[i0];
					Vector3 currentPos = currentPoint.Position;
					var intersection = new Vector3();

					List<RiverPoint> prevPoints = currentPoint.PrevPoints;
					for (int i1 = 0; i1 < prevPoints.Count; ++i1)
					{
						RiverPoint prevPoint = prevPoints[i1];
						Vector3 prevPos = prevPoint.Position;
						if (IsIntersectingLineSegment(s1, e1, prevPos, currentPos) != false)
						{
							isIntersecting = true;
							foundPoint = true;
							TryGetIntersection(s1, e1, prevPos, currentPos, out intersection);
							float dist1 = (prevPos - intersection).sqrMagnitude;
							float dist2 = (currentPos - intersection).sqrMagnitude;
							absorptionPoint = dist1 < dist2 ? prevPoint : currentPoint;
							break;
						}
						else
						{
							float dist1 = (prevPos - e1).sqrMagnitude;
							float dist2 = (currentPos - e1).sqrMagnitude;
							if (dist1 < nearestPoint.Key)
							{
								nearestPoint = new KeyValuePair<float, RiverPoint>(dist1, prevPoint);
							}
							if (dist2 < nearestPoint.Key)
							{
								nearestPoint = new KeyValuePair<float, RiverPoint>(dist2, currentPoint);
							}
						}
					}


					List<RiverPoint> nextPoints = currentPoint.NextPoints;
					for (int i1 = 0; i1 < nextPoints.Count; ++i1)
					{
						RiverPoint nextPoint = nextPoints[i1];
						Vector3 nextPos = nextPoint.Position;
						if (IsIntersectingLineSegment(s1, e1, currentPos, nextPos) != false)
						{
							isIntersecting = true;
							foundPoint = true;
							TryGetIntersection(s1, e1, currentPos, nextPos, out intersection);
							float dist1 = (currentPos - intersection).sqrMagnitude;
							float dist2 = (nextPos - intersection).sqrMagnitude;
							absorptionPoint = dist1 < dist2 ? currentPoint : nextPoint;
							break;
						}
						else
						{
							float dist1 = (currentPos - e1).sqrMagnitude;
							float dist2 = (nextPos - e1).sqrMagnitude;
							if (dist1 < nearestPoint.Key)
							{
								nearestPoint = new KeyValuePair<float, RiverPoint>(dist1, currentPoint);
							}
							if (dist2 < nearestPoint.Key)
							{
								nearestPoint = new KeyValuePair<float, RiverPoint>(dist2, nextPoint);
							}
						}
					}

					if (isIntersecting != false)
					{
						break;
					}
				}
			}

			if (isIntersecting == false)
			{
				if (nearestPoint.Key < distance * distance)
				{
					absorptionPoint = nearestPoint.Value;
					foundPoint = true;
				}
			}

			return foundPoint;
		}

		bool IsIntersectingLineSegment(Vector3 s1, Vector3 e1, Vector3 s2, Vector3 e2)
		{
			bool isIntersecting = false;

			Vector3 dir = e1 - s1;
			Vector3 v1 = s2 - s1;
			Vector3 v2 = e2 - s1;

			float cross1 = Vector3.Cross(dir, v1).y;
			float cross2 = Vector3.Cross(dir, v2).y;
			if (cross1 * cross2 < 0)
			{
				dir = e2 - s2;
				v1 = s1 - s2;
				v2 = e1 - s2;

				cross1 = Vector3.Cross(dir, v1).y;
				cross2 = Vector3.Cross(dir, v2).y;
				if (cross1 * cross2 < 0)
				{
					isIntersecting = true;
				}
			}

			return isIntersecting;
		}

		bool TryGetIntersection(Vector3 s1, Vector3 e1, Vector3 s2, Vector3 e2, out Vector3 intersection)
		{
			bool isIntersecting = false;
			intersection = Vector3.zero;

			Vector3 v1 = e1 - s1;
			Vector3 v2 = s2 - s1;
			Vector3 v3 = s1 - e2;
			Vector3 v4 = e2 - s2;

			float area1 = Vector3.Cross(v1, v2).y * 0.5f;
			float area2 = Vector3.Cross(v1, v3).y * 0.5f;
			float area = area1 + area2;

			if (Mathf.Approximately(area, 0) == false)
			{
				isIntersecting = true;
				intersection = s2 + v4 * area1 / area;
			}

			return isIntersecting;
		}

		Vector3 DecideInitialPosition()
		{
			var initPos = new Vector3();

			float chunkSize = parameter.ChunkSize;
			Vector2 numberOfChunk = parameter.NumberOfChunk;
			var fieldSize = new Vector2(chunkSize * numberOfChunk.x, chunkSize * numberOfChunk.y);

			initPos.x = fieldSize.x * (float)random.NextDouble();
			initPos.z = fieldSize.y * (float)random.NextDouble();

			if (parameter.HeadwaterIsOutside != false)
			{
				switch (random.Next(4))
				{
					//始点が左
					case 0:
						initPos.x = 0;
						break;
					//始点が右
					case 1:
						initPos.x = fieldSize.x;
						break;
					//始点が上
					case 2:
						initPos.z = fieldSize.y;
						break;
					//始点が下
					case 3:
						initPos.z = 0;
						break;
				}
			}

			return initPos;
		}

		float DecideInitialAngle(Vector3 initialPosition)
		{
			float initAngle = 0;

			float chunkSize = parameter.ChunkSize;
			Vector2 numberOfChunk = parameter.NumberOfChunk;
			var fieldSize = new Vector2(chunkSize * numberOfChunk.x, chunkSize * numberOfChunk.y);

			if (parameter.HeadwaterIsOutside != false)
			{
				var center = new Vector3(fieldSize.x * 0.5f, 0, fieldSize.y * 0.5f);
				Vector3 dir = center - initialPosition;
				initAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
			}
			else
			{
				initAngle = 360 * (float)random.NextDouble();
			}

			return initAngle;
		}

		bool IsInsideField(Vector3 pos)
		{
			float chunkSize = parameter.ChunkSize;
			Vector2Int numberOfChunk = parameter.NumberOfChunk;
			var fieldSize = new Vector2(chunkSize * numberOfChunk.x, chunkSize * numberOfChunk.y);
			return pos.x >= 0 && pos.x <= fieldSize.x && pos.z >= 0 && pos.z <= fieldSize.y;
		}

		bool DetectFromPercent(float percent)
		{
			int numDigit = 0;
			string percentString = percent.ToString();
			if (percentString.IndexOf(".") > 0)
			{
				numDigit = percentString.Split('.')[1].Length;
			}

			int rate = (int)Mathf.Pow(10, numDigit);
			int maxValue = 100 * rate;
			int border = (int)(percent * rate);

			return random.Next(0, maxValue) < border;
		}

		public bool Covers(Vector3 pos)
		{
			bool isInside = false;

			for (int i0 = 0; i0 < points.Count - 1; ++i0)
			{
				int baseIndex = i0 * 2;
				Vector3 v1 = vertices[baseIndex];
				Vector3 v2 = vertices[baseIndex + 2];
				Vector3 v3 = vertices[baseIndex + 3];
				Vector3 v4 = vertices[baseIndex + 1];

				if (IsInsideQuadrangle(v1, v2, v3, v4, pos) != false)
				{
					isInside = true;
					break;
				}
			}

			return isInside;
		}

		bool IsInsideQuadrangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 pos)
		{
			bool isInside = true;

			Vector3[] verts = { v1, v2, v3, v4 };
			float sign = 0;
			for (int i0 = 0; i0 < verts.Length; ++i0)
			{
				Vector3 v = verts[(i0 + 1) % verts.Length] - verts[i0];
				Vector3 p = pos - verts[i0];
				float cross = CrossVec2(v.x, v.z, p.x, p.z);
				cross = Mathf.Approximately(cross, 0) != false ? 0 : cross;
				if (Mathf.Approximately(sign, 0) != false)
				{
					sign = cross;
				}
				else
				{
					if (sign * cross < 0)
					{
						isInside = false;
						break;
					}
				}
			}

			return isInside;
		}

		float CrossVec2(float x1, float y1, float x2, float y2)
		{
			return x1 * y2 - y1 * x2;
		}

		void AddToPointMap(RiverPoint point)
		{
			Vector2Int chunk = GetChunk(point.Position);
			List<RiverPoint> pointsInChunk;
			if (pointMap.TryGetValue(chunk, out pointsInChunk) == false)
			{
				pointsInChunk = new List<RiverPoint>();
				pointMap.Add(chunk, pointsInChunk);
			}
			if (pointsInChunk.Contains(point) == false)
			{
				pointsInChunk.Add(point);
			}
		}

		Vector2Int GetChunk(Vector3 position)
		{
			var chunk = new Vector2Int();

			float chunkSize = parameter.ChunkSize;
			chunk.x = Mathf.FloorToInt(position.x / chunkSize);
			chunk.y = Mathf.FloorToInt(position.z / chunkSize);

			return chunk;
		}

		FieldPoint ToFieldPoint(RiverPoint riverPoint)
		{
			return new FieldPoint { Position = riverPoint.Position, Type = PointType.kRiver };
		}

		FieldConnectPoint ToFieldConnectPoint(RiverPoint riverPoint)
		{
			var point = new FieldConnectPoint();
			point.Initialize(riverPoint.Position, PointType.kRiver);
			return point;
		}

		float CalcBranchingProbability()
		{
			return Mathf.Lerp(parameter.MinInitialBranchingProbability, parameter.MaxInitialBranchingProbability, (float)random.NextDouble());
		}

		int CalcNumStepWithoutBranching()
		{
			return random.Next(parameter.MinNumStepWithoutBranching, parameter.MaxNumStepWithoutBranching + 1);
		}

		void ConnectPoints()
		{
			var connectPointMap = new Dictionary<RiverPoint, FieldConnectPoint>();
			FieldConnectPoint point = ToFieldConnectPoint(rootPoint);
			connectedPoints.Add(point);
			connectPointMap.Add(rootPoint, point);
			List<RiverPoint> nextPoints = rootPoint.NextPoints;
			for (int i0 = 0; i0 < nextPoints.Count; ++i0)
			{
				ConnectPointsRecursive(point, nextPoints[i0], connectPointMap);
			}
		}

		void ConnectPointsRecursive(FieldConnectPoint prevPoint, RiverPoint currentPoint, Dictionary<RiverPoint, FieldConnectPoint> connectPointMap)
		{
			if (connectPointMap.TryGetValue(currentPoint, out FieldConnectPoint point) == false)
			{
				point = ToFieldConnectPoint(currentPoint);
				connectedPoints.Add(point);
				connectPointMap.Add(currentPoint, point);

				prevPoint.SetConnection(point);
				point.SetConnection(prevPoint);

				List<RiverPoint> nextPoints = currentPoint.NextPoints;
				for (int i0 = 0; i0 < nextPoints.Count; ++i0)
				{
					ConnectPointsRecursive(point, nextPoints[i0], connectPointMap);
				}
			}
			else
			{
				prevPoint.SetConnection(point);
				point.SetConnection(prevPoint);
			}
		}

		public List<FieldPoint> Points
		{
			get => points;
		}

		public List<FieldConnectPoint> ConnectedPoints
		{
			get => connectedPoints;
		}

		public RiverPoint RootPoint
		{
			get => rootPoint;
		}

		public float Width
		{
			get => width;
		}

		System.Random random;

		System.DateTime lastInterruptionTime;

		List<FieldPoint> points = new List<FieldPoint>();
		List<FieldConnectPoint> connectedPoints = new List<FieldConnectPoint>();
		RiverPoint rootPoint;
		RiverParameter parameter;
		List<Vector3> vertices = new List<Vector3>();

		Dictionary<Vector2Int, List<RiverPoint>> pointMap = new Dictionary<Vector2Int, List<RiverPoint>>();

		float width;
		float branchingProbability;
		int numStepWithoutBranching;

		bool canBranch;
		float minAngleForBranching;
	}
}
