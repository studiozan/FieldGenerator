using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class ObjectPlacer
	{
		public ObjectPlacer()
		{
			gameObject = new GameObject();
		}

		public ObjectPlacer(string name) : this()
		{
			SetName(name);
		}

		public ObjectPlacer(string name, Transform parent) : this(name)
		{
			SetParent(parent);
		}

		public void SetName(string name)
		{
			gameObject.name = name;
		}

		public void SetParent(Transform parent)
		{
			gameObject.transform.SetParent(parent);
		}

		public void PlaceObjects<T>(GameObject prefab, List<T> points) where T : FieldPoint
		{
			for (int i0 = 0; i0 < points.Count; ++i0)
			{
				if (i0 < objects.Count)
				{
					objects[i0].transform.position = points[i0].Position;
				}
				else
				{
					GameObject obj = Object.Instantiate(prefab, points[i0].Position, Quaternion.identity, gameObject.transform);
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

		GameObject gameObject;
		List<GameObject> objects = new List<GameObject>();
	}
}
