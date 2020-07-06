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
			riverPointsPlacer = new ObjectPlacer("RiverPoints");
			roadPointsPlacer = new ObjectPlacer("RoadPoints");

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
			GenerateRoad();
		}

		void GenerateRiver()
		{
			var parameter = new RiverParameter
			{
				Prefab = riverPrefab,
				FieldSize = fieldSize,
				HeadwaterIsOutside = headwaterIsOutside,
				Width = width,
				AngleRange = angleRange,
				StepSize = riverStepSize,
				BranchingProbability = riverBranchingProb,
				MinNumStepToBranch = minNumStepToBranch,
				BendabilityAttenuation = bendabilityAttenuation,
			};

			river.Generate(parameter, random);

			riverPointsPlacer.PlaceObjects(prefab, river.Points);
		}

		void GenerateRoad()
		{
			roadPoints.Clear();
			RiverPoint riverRoot = river.RootPoint;
			for (int i0 = 0; i0 < riverRoot.NextPoints.Count; ++i0)
			{
				GenerateRoadRecursive(riverRoot, riverRoot.NextPoints[i0]);
			}
			roadPointsPlacer.PlaceObjects(prefab, roadPoints);
		}

		void GenerateRoadRecursive(RiverPoint currentPoint, RiverPoint nextPoint)
		{
			Vector3 dir = nextPoint.Point - currentPoint.Point;
			float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
			Quaternion rotation = Quaternion.Euler(0, angle, 0);
			float dist = currentPoint.Width / 2 + distanceFromRiver + roadWidth / 2;
			var leftBase = new Vector3(-dist, 0, 0);
			var rightBase = new Vector3(dist, 0, 0);
			Vector3 left = rotation * leftBase + currentPoint.Point;
			Vector3 right = rotation * rightBase + currentPoint.Point;
			roadPoints.Add(left);
			roadPoints.Add(right);

			if (nextPoint.NextPoints.Count > 0)
			{
				for (int i0 = 0; i0 < nextPoint.NextPoints.Count; ++i0)
				{
					GenerateRoadRecursive(nextPoint, nextPoint.NextPoints[i0]);
				}
			}
			else
			{
				roadPoints.Add(rotation * leftBase + nextPoint.Point);
				roadPoints.Add(rotation * rightBase + nextPoint.Point);
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
		float width = 10;
		[SerializeField]
		float angleRange = 60;
		[SerializeField]
		float riverBranchingProb = 1.0f;
		[SerializeField]
		int minNumStepToBranch = 10;
		[SerializeField]
		float bendabilityAttenuation = 0.01f;

		[SerializeField]
		float roadWidth = 4;
		[SerializeField]
		float distanceFromRiver = 2;



		System.Random random;

		River river = new River();
		float riverStepSize = 10;


		List<Vector3> roadPoints = new List<Vector3>();


		List<List<Vector3>> roadIntersections = new List<List<Vector3>>();

		ObjectPlacer riverPointsPlacer;
		ObjectPlacer roadPointsPlacer;
	}
}
