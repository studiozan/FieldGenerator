﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;
using SugorokuGenerator;
using PolygonGenerator;
using SquareArea;

public class SugorokuMapCheck : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
		initializedFlag = false;

		riverPolygonScript = new LinePolygonCreator();
		roadPolygonScript = new LinePolygonCreator();
		sugorokuScript = new SugorokuMapGenerator();
		mapGroundScript = new MapGroundPolygonCreator();
		objectList = new List<GameObject>();

		GameObject river, road;
		river = Instantiate( objectTable[ 4]);
		river.transform.parent = gameObject.transform;
		road = Instantiate( objectTable[ 5]);
		road.transform.parent = gameObject.transform;
		riverPolygonScript.SetObject( river);
		roadPolygonScript.SetObject( road);
		
		mapGroundScript.SetObject( objectTable[ 3]);
		
		fieldPointScript.Initialize( fieldPointParameter);
		sugorokuScript.Initialize();
	}

	// Update is called once per frame
	void Update()
	{
		if( initializedFlag == false)
		{
			Initialize();
			initializedFlag = true;
		}
	}

	void Initialize()
	{
		/*! マップの頂点の生成 */
		StartCoroutine( fieldPointScript.Generate());
		/*! 川のポリゴンの生成 */
		StartCoroutine( riverPolygonScript.CreatePolygon( fieldPointScript.GetRiverConnectPointList(), fieldPointScript.RiverWidth));
		/*! 道路のポリゴンの生成 */
		StartCoroutine( roadPolygonScript.CreatePolygon( fieldPointScript.GetRoadConnectPointList(), fieldPointScript.RoadWidth));
		//StartCoroutine( roadPolygonScript.CreatePolygon( townScript.GetSugorokuConnectPointList(), townScript.RoadWidth));
		/*! 地面のポリゴンの生成 */
		var minSize = new Vector3( 0f, 0f, 0f);
		var maxSize = new Vector3( 600f, 0f, 600f);
		StartCoroutine( mapGroundScript.GroundPolygonCreate( gameObject.transform, fieldPointScript.GetRoadConnectPointList(), minSize, maxSize));

		sugorokuScript.SetPointList(fieldPointScript.GetSugorokuConnectPointList());
		/*! すごろくマップの生成 */
		StartCoroutine( sugorokuScript.SugorokuMapCreate());

		ViewCreate();
	}

	/**
	 * 表示の処理
	 */
	void ViewCreate()
	{
		/*! デバッグ用の見た目オブジェクトの生成 */
		int i0, i1, tmp_i;
		List<Vector2> vec_list = new List<Vector2>();
		FieldConnectPoint tmp_point, tmp_point2;
		GameObject obj, mass_obj;
		MeshCreate mesh_script;
		List<FieldConnectPoint> tmp_list;
		List<int> tmp_data_list;

		for( i0 = 0; i0 < objectList.Count; i0++)
		{
			Destroy( objectList[ i0].gameObject);
		}
		objectList.Clear();

		tmp_list = sugorokuScript.GetPointList();
		tmp_data_list = sugorokuScript.GetSugorokuDataList();
		
		for( i0 = 0; i0 < tmp_list.Count; i0++)
		{
			tmp_point = tmp_list[ i0];
			if( tmp_data_list[ i0] == 0)
			{
				tmp_i = 0;
			}
			else
			{
				tmp_i = 2;
			}
			mass_obj = Instantiate( objectTable[ tmp_i]) as GameObject;
			mass_obj.transform.localPosition = new Vector3( tmp_point.Position.x, 10f, tmp_point.Position.z);
			mass_obj.transform.localScale = new Vector3( 7.5f, 1f, 7.5f);
			mass_obj.transform.parent = gameObject.transform;
			objectList.Add( mass_obj);
			for( i1 = 0; i1 < tmp_point.ConnectionList.Count; i1++)
			{
				tmp_point2 = tmp_point.ConnectionList[ i1];
				vec_list.Clear();
				vec_list.Add( new Vector2( tmp_point.Position.x, tmp_point.Position.z));
				vec_list.Add( new Vector2( tmp_point2.Position.x, tmp_point2.Position.z));
				obj = Instantiate( objectTable[ 1]) as GameObject;
				obj.transform.parent = mass_obj.transform;
				objectList.Add( obj);
				mesh_script = obj.GetComponent<MeshCreate>();
				mesh_script.RoadCreatePoly( vec_list, 2, 0f, 10f);
			}
		}
	}

	bool initializedFlag;
	List<GameObject> objectList;
	
	[SerializeField, HideInInspector]
	SugorokuMapGenerator sugorokuScript = default;
	[SerializeField, HideInInspector]
	LinePolygonCreator riverPolygonScript = default;
	[SerializeField, HideInInspector]
	LinePolygonCreator roadPolygonScript = default;
	[SerializeField, HideInInspector]
	MapGroundPolygonCreator mapGroundScript = default;

	[SerializeField, HideInInspector]
	FieldPointGenerator fieldPointScript = default;
	[SerializeField]
	FieldPointParameter fieldPointParameter = default;
	[SerializeField]
	GameObject[] objectTable = default;
}