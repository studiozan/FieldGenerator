using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class ObjectPlacer
	{
		public ObjectPlacer()
		{
			parentObj = new GameObject();
		}

		public ObjectPlacer(string name) : this()
		{
			SetName(name);
		}

		public void SetName(string name)
		{
			parentObj.name = name;
		}

		public void PlaceObjects(GameObject prefab, List<Vector3> points)
		{
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				if (i0 < objects.Count)
				{
					objects[i0].transform.position = points[i0];
				}
				else
				{
					GameObject obj = Object.Instantiate(prefab, points[i0], Quaternion.identity, parentObj.transform);
					obj.name = $"Point_{i0}";
					objects.Add(obj);
				}
			}

			if (objects.Count > points.Count)
			{
				for (int i0 = objects.Count - 1; i0 >= points.Count; --i0)
				{
					Object.Destroy(objects[i0]);
					objects.RemoveAt(i0);
				}
			}
		}

		public void Clear()
		{
			for (int i0 = 0; i0 < objects.Count; ++i0)
			{
				Object.Destroy(objects[i0]);
			}
			objects.Clear();
		}

		GameObject parentObj;
		List<GameObject> objects = new List<GameObject>();
	}
}
