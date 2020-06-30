using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownGenerator : MonoBehaviour
{
	void Awake()
	{
		riverPointsPlacer = new ObjectPlacer("RiverPoints");
		roadPointsPlacer = new ObjectPlacer("RoadPoints");

		riverObject = new GameObject("River");
		riverObject.transform.parent = transform;
		riverObject.AddComponent<MeshRenderer>().material = riverMaterial;
		riverMeshFilter = riverObject.AddComponent<MeshFilter>();

		roadObject = new GameObject("Road");
		roadObject.transform.parent = transform;
		roadObject.AddComponent<MeshRenderer>().material = roadMaterial;
		roadMeshFilter = roadObject.AddComponent<MeshFilter>();

		random = new System.Random(seed);
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
		riverPoints.Clear();
		riverPoints.Add(startPosition);
		Vector3 distance = endPosition - startPosition;
		float riverAngle = Mathf.Atan2(distance.x, distance.z) * Mathf.Rad2Deg;
		Vector3 zeroDegEndPoint = Quaternion.Euler(0, -riverAngle, 0) * distance;
		float pointDistance = zeroDegEndPoint.z / (numberOfPoint + 1);
		Vector3 lastZeroDegPoint = Vector3.zero;
		for (int i0 = 0; i0 < numberOfPoint; ++i0)
		{
			float angle = (float)random.NextDouble() * angleRange - angleRange / 2;
			if (i0 != 0)
			{
				Vector3 dist = zeroDegEndPoint - lastZeroDegPoint;
				angle += Mathf.Atan2(dist.x, dist.z) * Mathf.Rad2Deg;
			}
			Vector3 pos = Quaternion.Euler(0, angle, 0) * Vector3.forward;
			pos *= pointDistance / pos.z;
			pos += lastZeroDegPoint;
			lastZeroDegPoint = pos;
			pos = Quaternion.Euler(0, riverAngle, 0) * pos;
			pos += startPosition;
			riverPoints.Add(pos);
		}
		riverPoints.Add(endPosition);

		riverPointsPlacer.PlaceObjects(prefab, riverPoints);

		riverMeshFilter.sharedMesh = MeshCreator.CreateLineMesh(riverPoints, riverWidth);
	}

	void GenerateRoad()
	{
		GenerateRoadAlongRiver();
		GenerateGridRoad();
	}

	void GenerateRoadAlongRiver()
	{
		roadPoints.Clear();
		Vector3 riverDistance = riverPoints[riverPoints.Count - 1] - riverPoints[0];
		float riverAngle = Mathf.Atan2(riverDistance.x, riverDistance.z) * Mathf.Rad2Deg;
		Vector3 basePoint = new Vector3(riverWidth / 2 + distanceFromRiverToRoad + roadWidth / 2, 0, 0);
		basePoint = Quaternion.Euler(0, riverAngle, 0) * basePoint;

		Quaternion reverse = Quaternion.Euler(0, -riverAngle, 0);
		roadPoints.Add(basePoint + riverPoints[0]);
		int index = 0;
		while (index < riverPoints.Count - 1)
		{
			int relativeIndex = 1;
			if (index != riverPoints.Count - 2)
			{
				Vector3 dist1 = riverPoints[index + 1] - riverPoints[index];
				dist1 = reverse * dist1;
				Vector3 dist2 = riverPoints[index + 2] - riverPoints[index];
				dist2 = reverse * dist2;
				float angle1 = Mathf.Atan2(dist1.x, dist1.z) * Mathf.Rad2Deg;
				float angle2 = Mathf.Atan2(dist2.x, dist2.z) * Mathf.Rad2Deg;
				relativeIndex = angle1 > angle2 ? 1 : 2;
			}
			roadPoints.Add(basePoint + riverPoints[index + relativeIndex]);
			index += relativeIndex;
		}

		roadPointsPlacer.PlaceObjects(prefab, roadPoints);

		roadMeshFilter.sharedMesh = MeshCreator.CreateLineMesh(roadPoints, roadWidth);
	}

	void GenerateGridRoad()
	{
		parallelRoads.Clear();
		Vector3 distance = roadPoints[roadPoints.Count - 1] - roadPoints[0];
		float roadAngle = Mathf.Atan2(distance.x, distance.z) * Mathf.Rad2Deg;
		Quaternion rotation = Quaternion.Euler(0, roadAngle, 0);
		float length = distance.magnitude;
		int numRoad = 0;
		float totalWidth = 0;
		var parallelRoadPoints = new List<Vector3>();
		float outermostX = 0;
		Quaternion reverse = Quaternion.Euler(0, -roadAngle, 0);
		for (int i0 = 0; i0 < roadPoints.Count; ++i0)
		{
			Vector3 point = reverse * roadPoints[i0];
			if (point.x > outermostX)
			{
				outermostX = point.x;
			}
		}
		Vector3 zeroDegRoadStartPoint = reverse * roadPoints[0];
		while (totalWidth + roadWidth + roadSpacing < length)
		{
			++numRoad;
			parallelRoadPoints.Add(new Vector3(outermostX - zeroDegRoadStartPoint.x + roadWidth, 0, totalWidth + roadWidth / 2));
			totalWidth += roadWidth + roadSpacing;
		}
		if (totalWidth + roadWidth < length)
		{
			++numRoad;
			parallelRoadPoints.Add(new Vector3(outermostX - zeroDegRoadStartPoint.x + roadWidth, 0, totalWidth + roadWidth / 2));
			totalWidth += roadWidth;
		}
		else
		{
			totalWidth -= roadSpacing;
		}

		float offsetZ = (length - totalWidth) / 2;
		for (int i0 = 0; i0 < parallelRoadPoints.Count; ++i0)
		{
			Vector3 point = parallelRoadPoints[i0];
			point.z += offsetZ;
			point = rotation * point;
			point += roadPoints[0];
			parallelRoadPoints[i0] = point;
		}

		parallelRoads.Add(parallelRoadPoints);
		Vector3 offset = rotation * Vector3.right * (roadSpacing + roadWidth);
		for (int i0 = 1; i0 < numberParallelRoad; ++i0)
		{
			var points = new List<Vector3>(parallelRoadPoints);
			for (int i1 = 0; i1 < points.Count; ++i1)
			{
				points[i1] += offset * i0;
			}
			parallelRoads.Add(points);
		}

		for (int i0 = 0; i0 < parallelRoads.Count; ++i0)
		{
			if (i0 < placers.Count)
			{
				placers[i0].PlaceObjects(prefab, parallelRoads[i0]);
			}
			else
			{
				var placer = new ObjectPlacer($"ParallelRoadPoints_{i0}");
				placer.PlaceObjects(prefab, parallelRoads[i0]); 
				placers.Add(placer);
			}
		}

		if (placers.Count > parallelRoads.Count)
		{
			for (int i0 = placers.Count - 1; i0 >= parallelRoads.Count; --i0)
			{
				placers[i0].Clear();
				placers.RemoveAt(i0);
			}
		}
	}

	[SerializeField]
	GameObject prefab = default;
	[SerializeField]
	Material riverMaterial = default;
	[SerializeField]
	Material roadMaterial = default;
	[SerializeField]
	int seed = 0;
	[SerializeField]
	int numberOfPoint = 10;
	[SerializeField]
	Vector3 startPosition = default;
	[SerializeField]
	Vector3 endPosition = default;
	[SerializeField]
	float angleRange = 60;
	[SerializeField]
	float riverWidth = 10;

	[SerializeField]
	float roadWidth = 4;
	[SerializeField]
	float distanceFromRiverToRoad = 2;
	[SerializeField]
	float roadSpacing = 8;
	[SerializeField]
	int numberParallelRoad = 3;

	System.Random random;

	GameObject riverObject;
	MeshFilter riverMeshFilter;
	List<Vector3> riverPoints = new List<Vector3>();

	GameObject roadObject;
	MeshFilter roadMeshFilter;
	List<Vector3> roadPoints = new List<Vector3>();

	List<List<Vector3>> parallelRoads = new List<List<Vector3>>();

	ObjectPlacer riverPointsPlacer;
	ObjectPlacer roadPointsPlacer;
	List<ObjectPlacer> placers = new List<ObjectPlacer>();
}
