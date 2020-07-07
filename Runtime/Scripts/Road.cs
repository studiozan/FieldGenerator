using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class Road
	{
		public void Generate(RoadParameter parameter, RiverPoint riverRoot, System.Random random)
		{
			this.parameter = parameter;
			this.random = random;

			points.Clear();

			GenerateDistrictRoad();
			GenerateRoadAlongRiver(riverRoot);
			GenerateGridRoad();
		}

		void GenerateDistrictRoad()
		{
			float chunkSize = parameter.ChunkSize;
			Vector2Int numberOfChunk = parameter.NumberOfChunk;

			for (int row = 0; row <= numberOfChunk.y; ++row)
			{
				for (int column = 0; column <= numberOfChunk.x; ++column)
				{
					float x = chunkSize * column;
					float z = chunkSize * row;
					points.Add(new Vector3(x, 0, z));
				}
			}
		}

		void GenerateRoadAlongRiver(RiverPoint riverRoot)
		{
			for (int i0 = 0; i0 < riverRoot.NextPoints.Count; ++i0)
			{
				GenerateRoadAlongRiverRecursive(riverRoot, riverRoot.NextPoints[i0]);
			}
		}

		void GenerateRoadAlongRiverRecursive(RiverPoint currentPoint, RiverPoint nextPoint)
		{
			Vector3 dir = nextPoint.Position - currentPoint.Position;
			float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
			Quaternion rotation = Quaternion.Euler(0, angle, 0);
			float dist = currentPoint.Width / 2 + parameter.DistanceFromRiver + parameter.Width / 2;
			var leftBase = new Vector3(-dist, 0, 0);
			var rightBase = new Vector3(dist, 0, 0);
			Vector3 left = rotation * leftBase + currentPoint.Position;
			Vector3 right = rotation * rightBase + currentPoint.Position;
			points.Add(left);
			points.Add(right);

			if (nextPoint.NextPoints.Count > 0)
			{
				for (int i0 = 0; i0 < nextPoint.NextPoints.Count; ++i0)
				{
					GenerateRoadAlongRiverRecursive(nextPoint, nextPoint.NextPoints[i0]);
				}
			}
			else
			{
				points.Add(rotation * leftBase + nextPoint.Position);
				points.Add(rotation * rightBase + nextPoint.Position);
			}
		}

		void GenerateGridRoad()
		{
		}

		public List<Vector3> Points
		{
			get => points;
		}

		System.Random random;

		List<Vector3> points = new List<Vector3>();
		RoadParameter parameter;
	}
}
