using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class TownGenerator : MonoBehaviour
	{
		void Awake()
		{
			random = new System.Random(seed);
			placer = new ObjectPlacer("RiverPoints");

			GenerateTown();
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space) != false)
			{
				GenerateTown();
			}
		}

		void GenerateTown()
		{
			GenerateRiver();
		}

		void GenerateRiver()
		{
			points.Clear();
			Vector3 initialPoint = Vector3.zero;
			initialPoint.x = fieldSize.x * (float)random.NextDouble();
			initialPoint.z = fieldSize.y * (float)random.NextDouble();
			initialAngle = 360 * (float)random.NextDouble();
			if (headwaterIsOutside != false)
			{
				initialAngle %= 180;
				switch (random.Next(4))
				{
					//左
					case 0:
						initialPoint.x = 0;
						break;
					//右
					case 1:
						initialPoint.x = fieldSize.x;
						initialAngle *= -1;
						break;
					//上
					case 2:
						initialPoint.z = fieldSize.y;
						initialAngle += 90;
						break;
					//下
					case 3:
						initialPoint.z = 0;
						initialAngle -= 90;
						break;
				}
			}

			riverRootPoint.Point = initialPoint;
			var riverStep = new Vector3(0, 0, riverStepSize);

			Vector3 initialDir = Quaternion.Euler(0, initialAngle, 0) * riverStep;

			points.Add(riverRootPoint.Point);

			GenerateRiverRecursive(riverRootPoint, initialDir);

			placer.PlaceObjects(prefab, points);
		}

		void GenerateRiverRecursive(RiverPoint riverPoint, Vector3 dir)
		{
			var step = new Vector3(0, 0, riverStepSize);
			RiverPoint prevPoint = riverPoint;
			Vector3 prevDir = dir;
			int numStep = 0;

			while (IsInsideField(prevPoint.Point) != false)
			{
				++numStep;
				float prevAngle = Mathf.Atan2(prevDir.x, prevDir.z) * Mathf.Rad2Deg;
				float angle = angleRange * (float)random.NextDouble() - angleRange / 2;
				angle += prevAngle;

				Vector3 nextDir = Quaternion.Euler(0, angle, 0) * step;
				nextDir = Vector3.Lerp(prevDir, nextDir, (float)random.NextDouble());

				var nextPoint = new RiverPoint();
				nextPoint.Point = prevPoint.Point + nextDir;
				prevPoint.NextPoints.Add(nextPoint);

				points.Add(nextPoint.Point);

				if (numStep >= minNumStepToBranch)
				{
					if (DetectFromPercent(riverBranchingProb) != false)
					{
						numStep = 0;
						float angle2 = angle + (random.Next(2) == 0 ? -60 : 60);
						Debug.Log($"a1:{angle}, a2:{angle2}");
						Vector3 nextDir2 = Quaternion.Euler(0, angle2, 0) * step;
						GenerateRiverRecursive(prevPoint, nextDir2);
					}
				}

				prevDir = nextDir;
				prevPoint = nextPoint;
			}
		}

		public List<List<Vector3>> GetRoadIntersections()
		{
			return roadIntersections;
		}

		bool IsInsideField(Vector3 pos)
		{
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

		[SerializeField]
		GameObject prefab = default;
		[SerializeField]
		GameObject riverPrefab = default;
		[SerializeField]
		GameObject roadPrefab = default;
		[SerializeField]
		int seed = 0;
		[SerializeField]
		Vector2 fieldSize = default;
		[SerializeField]
		bool headwaterIsOutside = true;
		[SerializeField]
		float angleRange = 60;
		[SerializeField]
		float riverBranchingProb = 1.0f;
		[SerializeField]
		int minNumStepToBranch = 10;



		System.Random random;

		RiverPoint riverRootPoint = new RiverPoint();
		float riverStepSize = 10;
		float initialAngle;


		List<List<Vector3>> roadIntersections = new List<List<Vector3>>();

		ObjectPlacer placer;
		List<Vector3> points = new List<Vector3>();
	}
}
