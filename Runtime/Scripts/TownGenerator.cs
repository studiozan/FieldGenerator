﻿using System.Collections;
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
				FieldSize = new Vector2(chunkSize * numberOfChunk.x, chunkSize * numberOfChunk.y),
				HeadwaterIsOutside = headwaterIsOutside,
				Width = riverWidth,
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
			var parameter = new RoadParameter
			{
				NumberOfChunk = numberOfChunk,
				ChunkSize = chunkSize,
				Width = roadWidth,
				DistanceFromRiver = distanceFromRiver,
				Spacing = roadSpacing,
			};

			road.Generate(parameter, river.RootPoint, random);

			roadPointsPlacer.PlaceObjects(prefab, road.Points);
		}

		public List<FieldPoint> GetFieldPoints()
		{
			return fieldPoints;
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

		ObjectPlacer riverPointsPlacer;
		ObjectPlacer roadPointsPlacer;
	}
}
