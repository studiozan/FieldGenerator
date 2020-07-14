using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;
using SugorokuGenerator;
using SquareArea;

public class SugorokuMapCheck : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
		InitializedFlag = false;
	}

	// Update is called once per frame
	void Update()
	{
		if( InitializedFlag == false)
		{
			Initialize();
			InitializedFlag = true;
		}
	}

	void Initialize()
	{
		ObjectList = new List<GameObject>();
		SugorokuScript = new SugorokuMapGenerator();
		SugorokuScript.Initialize();
		SugorokuScript.SetPointList(TownScript.GetSugorokuConnectPointList());
		SugorokuScript.SugorokuMapCreate();

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

		for( i0 = 0; i0 < ObjectList.Count; i0++)
		{
			Destroy( ObjectList[ i0].gameObject);
		}
		ObjectList.Clear();

		tmp_list = SugorokuScript.GetPointList();
		tmp_data_list = SugorokuScript.GetSugorokuDataList();
		
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
			mass_obj = Instantiate( ObjectTable[ tmp_i]) as GameObject;
			mass_obj.transform.localPosition = new Vector3( tmp_point.Position.x, 10f, tmp_point.Position.z);
			mass_obj.transform.localScale = new Vector3( 7.5f, 1f, 7.5f);
			mass_obj.transform.parent = gameObject.transform;
			ObjectList.Add( mass_obj);
			for( i1 = 0; i1 < tmp_point.ConnectionList.Count; i1++)
			{
				tmp_point2 = tmp_point.ConnectionList[ i1];
				vec_list.Clear();
				vec_list.Add( new Vector2( tmp_point.Position.x, tmp_point.Position.z));
				vec_list.Add( new Vector2( tmp_point2.Position.x, tmp_point2.Position.z));
				obj = Instantiate( ObjectTable[ 1]) as GameObject;
				obj.transform.parent = mass_obj.transform;
				ObjectList.Add( obj);
				mesh_script = obj.GetComponent<MeshCreate>();
				mesh_script.RoadCreatePoly( vec_list, 2, 0f, 10f);
			}
		}
	}

	SugorokuMapGenerator SugorokuScript;
	bool InitializedFlag;
	List<GameObject> ObjectList;

	[SerializeField]
	TownGenerator TownScript = default;

	[SerializeField]
	GameObject[] ObjectTable = default;
}
