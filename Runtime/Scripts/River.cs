using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class River
	{
		public void Generate(RiverParameter parameter, System.Random random)
		{
			this.parameter = parameter;
			this.random = random;
			points.Clear();
			vertices.Clear();
			float chunkSize = parameter.ChunkSize;
			Vector2 numberOfChunk = parameter.NumberOfChunk;
			var fieldSize = new Vector2(chunkSize * numberOfChunk.x, chunkSize * numberOfChunk.y);
			Vector3 initialPoint = Vector3.zero;
			initialPoint.x = fieldSize.x * (float)random.NextDouble();
			initialPoint.z = fieldSize.y * (float)random.NextDouble();
			float initialAngle;
			if (parameter.HeadwaterIsOutside != false)
			{
				switch (random.Next(4))
				{
					//始点が左
					case 0:
						initialPoint.x = 0;
						break;
					//始点が右
					case 1:
						initialPoint.x = fieldSize.x;
						break;
					//始点が上
					case 2:
						initialPoint.z = fieldSize.y;
						break;
					//始点が下
					case 3:
						initialPoint.z = 0;
						break;
				}

				var center = new Vector3(fieldSize.x / 2, 0, fieldSize.y / 2);
				Vector3 dirToCenter = center - initialPoint;
				initialAngle = Mathf.Atan2(dirToCenter.x, dirToCenter.z) * Mathf.Rad2Deg;
			}
			else
			{
				initialAngle = 360 * (float)random.NextDouble();
			}

			rootPoint = new RiverPoint();
			rootPoint.Position = initialPoint;
			rootPoint.Width = parameter.Width;
			var step = new Vector3(0, 0, parameter.StepSize);

			Vector3 initialDir = Quaternion.Euler(0, initialAngle, 0) * step;

			var point = new FieldPoint
			{
				Position = rootPoint.Position,
				Type = PointType.kRiver,
			};
			points.Add(point);

			GenerateRiverRecursive(rootPoint, initialDir, 1);
		}

		void GenerateRiverRecursive(RiverPoint riverPoint, Vector3 dir, float bendability)
		{
			RiverPoint prevPoint = riverPoint;
			Vector3 prevDir = dir;
			int numStep = 0;
			int totalStep = 0;
			float bend = bendability;
			float angleRange = parameter.AngleRange;
			float width = parameter.Width;

			Quaternion nextRotation = Quaternion.identity;

			while (IsInsideField(prevPoint.Position) != false)
			{
				++numStep;
				++totalStep;
				float prevAngle = Mathf.Atan2(prevDir.x, prevDir.z) * Mathf.Rad2Deg;
				float angle = angleRange * (float)random.NextDouble() - angleRange / 2;
				angle += prevAngle;

				var step = new Vector3(0, 0, parameter.StepSize);
				Vector3 nextDir = Quaternion.Euler(0, angle, 0) * step;
				bend *= (1.0f - parameter.BendabilityAttenuation);
				nextDir = Vector3.Lerp(prevDir, nextDir, bend);

				var nextPoint = new RiverPoint();
				nextPoint.Position = prevPoint.Position + nextDir;
				nextPoint.Width = width;
				prevPoint.NextPoints.Add(nextPoint);

				var point = new FieldPoint
				{
					Position = nextPoint.Position,
					Type = PointType.kRiver,
				};
				points.Add(point);

				//vertices---------------------------
				float nextAngle = Mathf.Atan2(nextDir.x, nextDir.z) * Mathf.Rad2Deg;
				nextRotation = Quaternion.Euler(0, nextAngle, 0);
				Vector3 left = nextRotation * new Vector3(-width / 2, 0, 0) + prevPoint.Position;
				Vector3 right = nextRotation * new Vector3(width / 2, 0, 0) + prevPoint.Position;
				vertices.Add(left);
				vertices.Add(right);
				//-----------------------------------

				if (numStep >= parameter.MinNumStepToBranch)
				{
					if (DetectFromPercent(parameter.BranchingProbability) != false)
					{
						numStep = 0;
						float angle2 = angle + angleRange / 2 * (random.Next(2) == 0 ? -1 : 1);
						Vector3 nextDir2 = Quaternion.Euler(0, angle2, 0) * step;
						GenerateRiverRecursive(prevPoint, nextDir2, bend);
					}
				}

				prevDir = nextDir;
				prevPoint = nextPoint;
			}

			//vertices---------------------------
			if (totalStep != 0)
			{
				Vector3 left = nextRotation * new Vector3(-width / 2, 0, 0) + prevPoint.Position;
				Vector3 right = nextRotation * new Vector3(width / 2, 0, 0) + prevPoint.Position;
				vertices.Add(left);
				vertices.Add(right);
			}
			//-----------------------------------
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

		public bool Contain(Vector3 pos)
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
			get => parameter.Width;
		}

		System.Random random;

		List<FieldPoint> points = new List<FieldPoint>();
		RiverPoint rootPoint;
		RiverParameter parameter;

		List<Vector3> vertices = new List<Vector3>();
	}
}
