using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;
using SquareArea;

namespace MapPolygon
{
	public class MapGroundPolygonCreate : MonoBehaviour
	{
		// Start is called before the first frame update
		void Start()
		{
			PolygonVectorList = new List<Vector3>();
			PointStateList = new List<int>();
		}

		// Update is called once per frame
		void Update()
		{
			if( RoadPointList == null)
			{
				GetData();
				PolygonVectorCreate();
			}
		}

		void GetData()
		{
			RoadPointList = TownScript.GetRoadConnectPointList();
			RoadPointList = TownScript.GetSugorokuConnectPointList();
			PointStateList.Clear();

			if( RoadPointList == null)
			{
				return;
			}

			int i0;
			for( i0 = 0; i0 < RoadPointList.Count; i0++)
			{
				RoadPointList[ i0].Index = i0;
				PointStateList.Add( 0);
			}
		}

		/**
		 * ポリゴン生成用の頂点情報を生成
		 */
		void PolygonVectorCreate()
		{
			int i0, i1, i2, i3, count;
			List<int> tmp_list = new List<int>();
			Vector3 tmp_vec;
			Vector3[] vec_tbl = new Vector3[ 3];
			GameObject obj;
			MeshCreate mesh_script;
			FieldConnectPoint tmp_point, tmp_point2;
			List<Vector2> uv_list = new List<Vector2>();
			Vector2 tmp_uv = Vector2.zero;

			for( i0 = 0; i0 < RoadPointList.Count; i0++)
			//for( i0 = 0; i0 < 5; i0++)
			{
				tmp_point = RoadPointList[ i0];
				count = tmp_point.ConnectionList.Count;
				Debug.Log($"count:{count}");
				if( count <= 1)
				{
					continue;
				}
				for( i1 = 0; i1 < count; i1++)
				{
					for( i2 = i1 + 1; i2 < count; i2++)
					{
						if( i1 == i2)
						{
							continue;
						}
						/* 基準点と繋がっている2点間とのポリゴンを生成する */
						vec_tbl[ 0] = new Vector3( tmp_point.Position.x, tmp_point.Position.y, tmp_point.Position.z);
						tmp_point2 = tmp_point.ConnectionList[ i1];
						vec_tbl[ 1] = new Vector3( tmp_point2.Position.x, tmp_point2.Position.y, tmp_point2.Position.z);
						tmp_point2 = tmp_point.ConnectionList[ i2];
						vec_tbl[ 2] = new Vector3( tmp_point2.Position.x, tmp_point2.Position.y, tmp_point2.Position.z);
						tmp_vec = Cross( vec_tbl[ 0], vec_tbl[ 1], vec_tbl[ 2]);
						if( tmp_vec.y < 0)
						{
							tmp_vec = vec_tbl[ 1];
							vec_tbl[ 1] = vec_tbl[ 2];
							vec_tbl[ 2] = tmp_vec;
						}
						for( i3 = 0; i3 < vec_tbl.Length; i3++)
						{
							vec_tbl[ i3].y = -0.2f;
							PolygonVectorList.Add( vec_tbl[ i3]);
							tmp_uv.x = vec_tbl[ i3].x * 0.01f;
							tmp_uv.y = vec_tbl[ i3].z * 0.01f;
							Debug.Log($"uv x:{tmp_uv.x} y:{tmp_uv.y} pos:{vec_tbl[ i3]}");
							uv_list.Add(tmp_uv);
						}
					}
				}
			}

			for( i0 = 0; i0 < uv_list.Count; i0++)
			{
				tmp_uv = uv_list[ i0];
			}

			obj = Instantiate( ObjectTable[ 0]) as GameObject;
			mesh_script = obj.GetComponent<MeshCreate>();
			mesh_script.VectorPolygonCreate( PolygonVectorList, uv_list);
		}

		/**
		 * 外積を求める
		 *
		 * ポリゴンの表面判定に使う
		 */
		Vector3 Cross( Vector3 pos1, Vector3 pos2, Vector3 pos3)
		{
			Vector3 tmp_vec, tmp_vec2, ret = new Vector3(0,0,0);

			tmp_vec = pos2 - pos1;
			tmp_vec2 = pos3 - pos1;

			ret.x = tmp_vec.y * tmp_vec2.z - tmp_vec.z * tmp_vec2.y;
			ret.y = tmp_vec.z * tmp_vec2.x - tmp_vec.x * tmp_vec2.z;
			ret.z = tmp_vec.x * tmp_vec2.y - tmp_vec.y * tmp_vec2.x;

			return ret;
		}

		List<Vector3> PolygonVectorList;			/* ポリゴン生成に渡す頂点リスト */
		List<FieldConnectPoint> RoadPointList;		/* 道路の繋がりポイントのリスト */
		List<int> PointStateList;					/* 道路の繋がりポイントのステータスリスト */

		[SerializeField]
		GameObject[] ObjectTable = default;			/* オブジェクトテーブル */

		[SerializeField]
		TownGenerator TownScript = default;
	}
}
