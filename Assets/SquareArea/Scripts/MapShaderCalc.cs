using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;
using PolygonGenerator;
using SquareArea;

public class MapShaderCalc : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
		ObjectList = new List<GameObject>();
		MapGroundScript = new MapGroundPolygonCreator();
		MapGroundScript.SetObject( ObjectTable[ 4]);

		initializedFlag = false;
	}

	// Update is called once per frame
	void Update()
	{
		if( initializedFlag == false)
		{
			Create();
			initializedFlag = true;
		}
	}

	void Create()
	{
		//TownScript.GenerateTown();
		
		/* 川で繋がっている部分のポリゴンを生成 */
		//ObjectCreate( TownScript.GetRiverConnectPointList(), true);
		/* すごろく用に繋がっている部分のポリゴンを生成 */
		//ObjectCreate( TownScript.GetSugorokuConnectPointList());

		List<FieldConnectPoint> point_list;
		point_list = TownScript.GetSugorokuConnectPointList();
		//point_list = TownScript.GetRoadConnectPointList();
		MapGroundScript.PolygonCreate( point_list);
#if false
		List<Vector3> vec_list = new List<Vector3>();
		List<Vector2> uv_list = new List<Vector2>();
		List<Vector2> tmp_list;

		vec_list.Add( new Vector3(-100,0,100));		// 0
		vec_list.Add( new Vector3(0,0,100));		// 1
		vec_list.Add( new Vector3(0,0,0));			// 2
		vec_list.Add( new Vector3(-100,0,0));		// 3

		BuildingParameter buil_param = new BuildingParameter( vec_list);
		List<BuildingParameter> buil_list = new List<BuildingParameter>();
		//buil_param.SetBuildingType(BuildingParameter.BuildingType.kBuildingB, 2);
		buil_param.SetBuildingHeight( 100f);
		tmp_list = buil_param.GetRoofTopUV();
//		tmp_list = buil_param.GetSideUV();
		uv_list.Add( tmp_list[ 0]);
		uv_list.Add( tmp_list[ 1]);
		uv_list.Add( tmp_list[ 2]);
		
		uv_list.Add( tmp_list[ 2]);
		uv_list.Add( tmp_list[ 3]);
		uv_list.Add( tmp_list[ 0]);

		buil_list.Add( buil_param);
		
		vec_list = new List<Vector3>();
		vec_list.Add( new Vector3(-100,0,300));
		vec_list.Add( new Vector3(0,0,300));
		vec_list.Add( new Vector3(0,0,200));
		vec_list.Add( new Vector3(-100,0,200));
		buil_param = new BuildingParameter( vec_list);
		buil_param.SetBuildingHeight( 50f);
		buil_param.SetBuildingType(BuildingParameter.BuildingType.kBuildingC, 3);
		buil_list.Add( buil_param);
		//test_mesh.PolygonCreate( vec_list, uv_list);
		test_mesh.BuildingPolygonCreate( buil_list);
#endif
	}
	
	/* 川と道路を線状ポリゴンで結ぶ */
	void ObjectCreate( List<FieldConnectPoint> list, bool clear = false)
	{
		int i0, i1, tmp_i;
		GameObject obj, obj2;
		FieldConnectPoint tmp_point;
		List<Vector2> vec_list = new List<Vector2>();
		MeshCreate mesh_script;
		int[] tbl = new int[ 7];
		tbl[ 0] = 1;	// 川
		tbl[ 1] = 2;	// 川沿いの道路
		tbl[ 2] = 2;	// 碁盤目
		tbl[ 3] = 2;	// 区画の境目の道路
		tbl[ 4] = 2;	// 碁盤目と川沿い道路の交差点
		tbl[ 5] = 2;	// 碁盤目と区域の境目の交差点
		tbl[ 6] = 2;	// 川沿い道路と区域の境目の交差点

		if( clear != false)
		{
			for( i0 = 0; i0 < ObjectList.Count; i0++)
			{
				Destroy( ObjectList[ i0].gameObject);
			}
			ObjectList.Clear();
		}

		for( i0 = 0; i0 < list.Count; i0++)
		{
			tmp_point = list[ i0];
			obj = Instantiate( ObjectTable[ 0]) as GameObject;
			obj.transform.localPosition = tmp_point.Position;
			obj.transform.parent = gameObject.transform;
			obj.name = "obj" + i0;
			ObjectList.Add( obj);

			//if( !(tmp_point.Attribute == 2 || tmp_point.Attribute == 4))
			{
				obj.GetComponent<MeshRenderer>().enabled = false;
			}

			tmp_i = tbl[ (int)tmp_point.Type];
			for( i1 = 0; i1 < tmp_point.ConnectionList.Count; i1++)
			{
				obj2 = Instantiate( ObjectTable[ tmp_i]) as GameObject;
				mesh_script = obj2.GetComponent<MeshCreate>();
				vec_list.Clear();
				vec_list.Add( new Vector2( tmp_point.Position.x, tmp_point.Position.z));
				vec_list.Add( new Vector2( tmp_point.ConnectionList[ i1].Position.x, tmp_point.ConnectionList[ i1].Position.z));
				mesh_script.RoadCreatePoly( vec_list, 2, 0f, 0f);
				obj2.transform.parent = obj.transform;
			}
		}
	}

	bool initializedFlag;

	List<GameObject> ObjectList;
	MapGroundPolygonCreator MapGroundScript;

	[SerializeField]
	GameObject[] ObjectTable = default;

	[SerializeField]
	TownGenerator TownScript = default;
#if false
	[SerializeField]
	MeshCreator test_mesh = default;
#endif
}
