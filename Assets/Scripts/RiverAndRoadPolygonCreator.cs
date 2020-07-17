using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;

namespace PolygonGenerator
{
	public class RiverAndRoadPolygonCreator : MonoBehaviour
	{
		void Awake()
		{
			riverObj = Instantiate(riverPrefab);
			roadObj = Instantiate(roadPrefab);

			riverPolygonCreator.SetObject(riverObj);
			roadPolygonCreator.SetObject(roadObj);

			GetComponent<TownGenerator>().OnGenerate += CreateRiverAndRoadPolygon;
		}

		void CreateRiverAndRoadPolygon(TownGenerator generator)
		{
			StartCoroutine(riverPolygonCreator.CreatePolygon(generator.GetRiverConnectPointList(), generator.RiverWidth));
			StartCoroutine(roadPolygonCreator.CreatePolygon(generator.GetRoadConnectPointList(), generator.RoadWidth));
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
