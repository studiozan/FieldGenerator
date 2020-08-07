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
			vertices.Clear();

			Vector3 initialPosition = DecideInitialPosition();
			float initialAngle = DecideInitialAngle(initialPosition);

			rootPoint = new RiverPoint();
			rootPoint.Position = initialPosition;
			rootPoint.Width = width;
			var step = new Vector3(0, 0, parameter.StepSize);

			Vector3 initialDir = Quaternion.Euler(0, initialAngle, 0) * step;

			var point = new FieldPoint
			{
				Position = rootPoint.Position,
				Type = PointType.kRiver,
			};
			points.Add(point);

			minAngleForBranching = Mathf.Atan2(width * 0.5f, parameter.StepSize) * Mathf.Rad2Deg * 2;
			canBranch = parameter.AngleRange >= minAngleForBranching;

			yield return GenerateRiverRecursive(rootPoint, initialDir, 1);
		}

		IEnumerator GenerateRiverRecursive(RiverPoint riverPoint, Vector3 dir, float bendability)
		{
			RiverPoint currentPoint = riverPoint;
			Vector3 nextDir = dir;
			int numStep = 0;
			int totalStep = 0;
			float bend = bendability;
			float angleRange = parameter.AngleRange;
			float halfWidth = width * 0.5f;
			var step = new Vector3(0, 0, parameter.StepSize);

			while (IsInsideField(currentPoint.Position) != false)
			{
				++numStep;
				++totalStep;

				var nextPoint = new RiverPoint();
				nextPoint.Position = currentPoint.Position + nextDir;
				nextPoint.Width = width;
				nextPoint.PrevPoint = currentPoint;

				float angle = Mathf.Atan2(nextDir.x, nextDir.z) * Mathf.Rad2Deg;

				Vector3 normDir = nextDir.normalized;
				var leftBase = new Vector3(-normDir.z, 0, normDir.x) * halfWidth;
				var rightBase = new Vector3(normDir.z, 0, -normDir.x) * halfWidth;
				Vector3 left = leftBase + currentPoint.Position;
				Vector3 right = rightBase + currentPoint.Position;
				Vector3 left2 = leftBase + nextPoint.Position;
				Vector3 right2 = rightBase + nextPoint.Position;

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
						yield return GenerateRiverRecursive(currentPoint, nextDir2, bend);
					}
				}

				currentPoint.NextPoints.Add(nextPoint);

				var point = new FieldPoint
				{
					Position = nextPoint.Position,
					Type = PointType.kRiver,
				};
				points.Add(point);

				vertices.Add(left);
				vertices.Add(right);

				float nextAngle = angle + angleRange * (float)random.NextDouble() - angleRange * 0.5f;
				bend *= (1.0f - parameter.BendabilityAttenuation);
				nextAngle = Mathf.Lerp(angle, nextAngle, bend);
				nextDir = Quaternion.Euler(0, nextAngle, 0) * step;

				currentPoint = nextPoint;
			}

			if (totalStep != 0)
			{
				Vector3 normDir = nextDir.normalized;
				Vector3 left = new Vector3(-normDir.z, 0, normDir.x) * halfWidth + currentPoint.Position;
				Vector3 right = new Vector3(normDir.z, 0, -normDir.x) * halfWidth + currentPoint.Position;
				vertices.Add(left);
				vertices.Add(right);
			}
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

		Vector2Int GetChunk(Vector3 pos)
		{
			float chunkSize = parameter.ChunkSize;
			int x = (int)(pos.x / chunkSize);
			int y = (int)(pos.z / chunkSize);
			return new Vector2Int(x, y);
		}

		float CalcBranchingProbability()
		{
			return Mathf.Lerp(parameter.MinInitialBranchingProbability, parameter.MaxInitialBranchingProbability, (float)random.NextDouble());
		}

		int CalcNumStepWithoutBranching()
		{
			return random.Next(parameter.MinNumStepWithoutBranching, parameter.MaxNumStepWithoutBranching + 1);
		}

		public List<FieldPoint> Points
		{
			get => points;
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
		RiverPoint rootPoint;
		RiverParameter parameter;
		List<Vector3> vertices = new List<Vector3>();
		float width;
		float branchingProbability;
		int numStepWithoutBranching;

		bool canBranch;
		float minAngleForBranching;
	}
}
