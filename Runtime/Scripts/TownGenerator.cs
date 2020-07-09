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

			connection.Initialize();

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
			fieldPoints.Clear();

			GenerateRiver();
			GenerateRoad();

			float interval = Mathf.Max(roadWidth + roadSpacing, riverStepSize);
			connection.FieldConnectCreate(fieldPoints, interval);

			riverPointsPlacer.PlaceObjects(prefab, connection.GetRiverConnectPointList());
			roadPointsPlacer.PlaceObjects(prefab, connection.GetRoadConnectPointList());
		}

		void GenerateRiver()
		{
			var parameter = new RiverParameter
			{
				ChunkSize = chunkSize,
				NumberOfChunk = numberOfChunk,
				HeadwaterIsOutside = headwaterIsOutside,
				Width = riverWidth,
				AngleRange = angleRange,
				StepSize = riverStepSize,
				BranchingProbability = riverBranchingProb,
				MinNumStepToBranch = minNumStepToBranch,
				BendabilityAttenuation = bendabilityAttenuation,
			};

			river.Generate(parameter, random);
			fieldPoints.AddRange(river.Points);
		}

		void GenerateRoad()
		{
			var parameter = new RoadParameter
			{
				NumberOfChunk = numberOfChunk,
				ChunkSize = chunkSize,
				Width = roadWidth,
				DistanceFromRiver = distanceFromRiver,
				Spacing = roadSpacing,
			};

			road.Generate(parameter, river.RootPoint, random);
			fieldPoints.AddRange(road.Points);
		}

		public List<FieldPoint> GetFieldPoints()
		{
			return fieldPoints;
		}

		public List<FieldConnectPoint> GetRiverConnectPointList()
		{
			return connection.GetRiverConnectPointList();
		}

		public List<FieldConnectPoint> GetRoadConnectPointList()
		{
			return connection.GetRoadConnectPointList();
		}

		public List<FieldConnectPoint> GetSugorokuConnectPointList()
		{
			return connection.GetSugorokuConnectPointList();
		}



		[SerializeField]
		GameObject prefab = default;

		[SerializeField]
		int seed = 0;
		[SerializeField]
		float chunkSize = 100;
		[SerializeField]
		Vector2Int numberOfChunk = new Vector2Int(10, 10);
		[SerializeField]
		bool headwaterIsOutside = true;
		[SerializeField]
		float riverWidth = 10;
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
		[SerializeField]
		float roadSpacing = 20;



		System.Random random;

		River river = new River();
		float riverStepSize = 10;


		Road road = new Road();


		List<FieldPoint> fieldPoints = new List<FieldPoint>();

		PointConnection connection = new PointConnection();

		ObjectPlacer riverPointsPlacer;
		ObjectPlacer roadPointsPlacer;
	}
}
