using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class RiverAndRoadPolygonCreator : MonoBehaviour
	{
		void Awake()
		{
			riverObj = Instantiate(riverPrefab);
			roadObj = Instantiate(roadPrefab);

			riverPolygonCreator.SetObject(riverObj);
			roadPolygonCreator.SetObject(roadObj);

			TownGenerator generator = GetComponent<TownGenerator>();
			generator.OnGenerate += () =>
			{
				riverPolygonCreator.CreatePolygon(generator.GetRiverConnectPointList(), generator.RiverWidth);
				roadPolygonCreator.CreatePolygon(generator.GetRoadConnectPointList(), generator.RoadWidth);
			};
		}



		[SerializeField]
		GameObject riverPrefab = default;
		[SerializeField]
		GameObject roadPrefab = default;

		GameObject riverObj;
		GameObject roadObj;

		LinePolygonCreator riverPolygonCreator = new LinePolygonCreator();
		LinePolygonCreator roadPolygonCreator = new LinePolygonCreator();
	}
}
