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
			Vector2 fieldSize = parameter.FieldSize;
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
			step = new Vector3(0, 0, parameter.StepSize);

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
			float bend = bendability;
			float angleRange = parameter.AngleRange;

			while (IsInsideField(prevPoint.Position) != false)
			{
				++numStep;
				float prevAngle = Mathf.Atan2(prevDir.x, prevDir.z) * Mathf.Rad2Deg;
				float angle = angleRange * (float)random.NextDouble() - angleRange / 2;
				angle += prevAngle;

				Vector3 nextDir = Quaternion.Euler(0, angle, 0) * step;
				bend *= (1.0f - parameter.BendabilityAttenuation);
				nextDir = Vector3.Lerp(prevDir, nextDir, bend);

				var nextPoint = new RiverPoint();
				nextPoint.Position = prevPoint.Position + nextDir;
				nextPoint.Width = parameter.Width;
				prevPoint.NextPoints.Add(nextPoint);

				var point = new FieldPoint
				{
					Position = nextPoint.Position,
					Type = PointType.kRiver,
				};
				points.Add(point);

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
		}

		bool IsInsideField(Vector3 pos)
		{
			Vector2 fieldSize = parameter.FieldSize;
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

		List<FieldPoint> points = new List<FieldPoint>();

		System.Random random;
		RiverPoint rootPoint;
		RiverParameter parameter;
		Vector3 step;
	}
}
