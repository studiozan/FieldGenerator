using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	[System.Serializable]
	public class ObjectPlacer
	{
		public void Initialize(int seed = 0)
		{
			random = new System.Random(seed);
		}

		public List<GameObject> PlaceObjects(PlacementParameter parameter, List<SurroundedArea> areas, Transform parent = null)
		{
			var objects = new List<GameObject>();

			var copyAreas = new List<SurroundedArea>(areas);
			int count = copyAreas.Count;
			int max = Mathf.RoundToInt((float)count * parameter.placementRate);
			var objectCountMap = new Dictionary<string, int>();
			for (int i0 = 0; i0 < max; ++i0)
			{
				int randomIndex = random.Next(count);
				List<Vector3> points = copyAreas[randomIndex].AreaPoints;
				Vector3 center = CalcCenter(points);
				GameObject prefab = DetectWeightedPrefab(parameter.weightedPrefabs);

				if (prefab == null)
				{
					Debug.LogError($"抽選されたプレハブがnullです。");
				}

				GameObject obj = Object.Instantiate(prefab);
				obj.transform.SetParent(parent);
				string prefabName = prefab.name;
				if (objectCountMap.ContainsKey(prefabName) == false)
				{
					objectCountMap.Add(prefabName, 0);
				}
				obj.name = $"{prefabName}{objectCountMap[prefabName]}";
				++objectCountMap[prefabName];
				obj.transform.position = center;
				objects.Add(obj);

				--count;
				copyAreas[randomIndex] = copyAreas[count];
				copyAreas.RemoveAt(count);
			}

			return objects;
		}

		Vector3 CalcCenter(List<Vector3> points)
		{
			var center = new Vector3();

			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				center += points[i0];
			}
			center /= points.Count;

			return center;
		}

		GameObject DetectWeightedPrefab(WeightedObject[] weightedPrefabs)
		{
			GameObject prefab = null;

			float totalWeight = 0;
			for (int i0 = 0; i0 < weightedPrefabs.Length; ++i0)
			{
				totalWeight += weightedPrefabs[i0].weight;
			}

			float border = totalWeight * (float)random.NextDouble();

			for (int i0 = 0; i0 < weightedPrefabs.Length; ++i0)
			{
				float weight = weightedPrefabs[i0].weight;
				if (Mathf.Approximately(weight, 0) == false)
				{
					if (border <= weight)
					{
						prefab = weightedPrefabs[i0].gameObject;
						break;
					}

					border -= weight;
				}
			}

			return prefab;
		}

		System.Random random;
	}
}
