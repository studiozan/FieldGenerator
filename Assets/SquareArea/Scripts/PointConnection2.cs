using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SquareArea;
using FieldGenerator;
using SugorokuMap;

namespace FieldGenerator
{
	public class PointConnection2 : MonoBehaviour
	{
		// Start is called before the first frame update
		void Start()
		{
			if( Seed >= 0)
			{
				RandomSystem = new System.Random( Seed);
			}
			else
			{
				RandomSystem = new System.Random();
			}
			PointList = new List<Point>();
			ObjectList = new List<GameObject>();
			FieldPointList = new List<FieldPoint>();

			TestCalc2();
		}

		void TestCalc2()
		{
			/* 川で繋がっている部分のポリゴンを生成 */
			ObjectCreate( TownScript.GetRiverConnectPointList(), true);
			/* すごろく用に繋がっている部分のポリゴンを生成 */
			ObjectCreate( TownScript.GetSugorokuConnectPointList());

			SugorokuScript.SetPointList( GetSugorokuList());
		}

		public List<FieldConnectPoint> GetSugorokuList()
		{
			return TownScript.GetSugorokuConnectPointList();
		}

		void VecCreate()
		{
#if false
			int i0, i1, num, tmp_i;
			float tmp_f, pow = 0.2f, half = pow * 0.5f;
			Vector3 tmp_vec = Vector3.zero;
			FieldPoint tmp_point;
			num = 10;
			/* 下部分の道路 */
			for( i0 = 0; i0 < num; i0++)
			{
				tmp_f = (float)RandomSystem.NextDouble() * pow - half;
				tmp_vec.z = Interval * i0 + Interval * tmp_f;
				for( i1 = 0; i1 < num; i1++)
				{
					tmp_point = new FieldPoint();
					tmp_f = (float)RandomSystem.NextDouble() * pow - half;
					tmp_vec.x = Interval * i1 + Interval * tmp_f;
					tmp_point.Position = tmp_vec;
					if( i0 == num - 1)
					{
						/* 碁盤目と川沿い道路の交差点 */
						tmp_i = 4;
					}
					else
					{
						tmp_i = 2;
					}
					tmp_point.Type = (PointType)tmp_i;
					FieldPointList.Add( tmp_point);
				}
			}

			/* 下部分の川沿いの道路 */
			tmp_i = FieldPointList.Count - 1;
			tmp_vec = FieldPointList[ tmp_i].Position;
			for( i0 = 0; i0 < num + 4; i0++)
			{
				tmp_vec.x = Interval * i0 - Interval * 2.5f;
				tmp_point = new FieldPoint();
				tmp_point.Position = tmp_vec;
				tmp_point.Type = (PointType)1;
				FieldPointList.Add( tmp_point);
			}
			
			/* 上部分の道路 */
			for( i0 = 0; i0 < 2; i0++)
			{
				tmp_f = (float)RandomSystem.NextDouble() * pow - half;
				tmp_vec.z = Interval * i0 + Interval * tmp_f + 110f;
				for( i1 = 0; i1 < num; i1++)
				{
					tmp_point = new FieldPoint();
					tmp_f = (float)RandomSystem.NextDouble() * pow - half;
					tmp_vec.x = Interval * i1 + Interval * tmp_f;
					tmp_point.Position = tmp_vec;
					if( i0 == 0)
					{
						/* 碁盤目と川沿い道路の交差点 */
						tmp_i = 4;
					}
					else
					{
						tmp_i = 2;
					}
					tmp_point.Type = (PointType)tmp_i;
					FieldPointList.Add( tmp_point);
				}
			}

			/* 川 */
			tmp_vec.z = 100f;
			for( i0 = 0; i0 < num; i0++)
			{
				tmp_point = new FieldPoint();
				tmp_vec.x = Interval * i0 * 2f - Interval * 5.5f;
				tmp_point.Position = tmp_vec;
				tmp_point.Type = (PointType)0;
				FieldPointList.Add( tmp_point);
			}
#else
			FieldPointList.AddRange( TownScript.GetFieldPoints());
#endif
		}

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

		void SetPoint()
		{
			int i0;
			Point tmp_point;
			FieldPoint tmp_field;

			PointList.Clear();
			
			for( i0 = 0; i0 < FieldPointList.Count; i0++)
			{
				tmp_field = FieldPointList[ i0];
				tmp_point = new Point();
				tmp_point.Initialize( tmp_field.Position, (int)tmp_field.Type);
				PointList.Add( tmp_point);
			}
		}

		/**
		 * ポイントのリストを元に接続の処理を行う
		 * @param point_list	ポイントクラスのリスト
		 * @param interval		繋がる座標感の幅
		 * @param specific_list	接続を行う属性のテーブル
		 * @param random		接続する確率
		 * @param max_num		接続する最大数。-1の場合は判定しない
		 */
		public static void SetConnection( List<Point> point_list, float interval, List<int> specific_list,
			float random = 1f, int max_num = -1)
		{
			int i0, i1, i2, tmp_i, random_count;
			float itv, length, theta = 0.707f, tmp_f, rand;
			bool flg;
			Vector3 sub;
			Point tmp_point;
			Vector3[] direction = new Vector3[ 4];
			float[] min = new float[ 4];
			int[] no = new int[ 4];
			itv = interval * 1.5f;
			itv = itv * itv;
			random_count = 0;
			System.Random _random = new System.Random();

			direction[ 0] = Vector3.forward;
			direction[ 1] = Vector3.back;
			direction[ 2] = Vector3.right;
			direction[ 3] = Vector3.left;

			/* ランダムを使う場合は双方向で繋がるようにしないとダメかもしれない */

			for( i0 = 0; i0 < point_list.Count; i0++)
			{
				tmp_point = point_list[ i0];
				flg = false;
				for( i1 = 0; i1 < specific_list.Count; i1++)
				{
					if( tmp_point.Attribute == specific_list[ i1])
					{
						flg = true;
						break;
					}
				}
				if( flg == false)
				{
					/*! 使用する属性じゃないので判定しない */
					continue;
				}
				if( max_num >= 0 && max_num <= random_count)
				{
					/*! ランダムで作る最大数に達しているので処理を終わる */
					return;
				}
				rand = (float)_random.NextDouble();
				if( rand > random)
				{
					/*! ランダムに判定しない */
					continue;
				}
				min[ 0] = itv;	min[ 1] = itv;	min[ 2] = itv;	min[ 3] = itv;
				no[ 0] = -1;	no[ 1] = -1;	no[ 2] = -1;	no[ 3] = -1;
				for( i1 = 0; i1 < point_list.Count; i1++)
				{
					if( i0 == i1)
					{
						/*! 同じものは判定しない */
						continue;
					}
					flg = false;
					for( i2 = 0; i2 < specific_list.Count; i2++)
					{
						if( point_list[ i1].Attribute == specific_list[ i2])
						{
							flg = true;
							break;
						}
					}
					if( flg == false)
					{
						/*! 使用する属性じゃないので判定しない */
						continue;
					}
					sub = point_list[ i1].Position - tmp_point.Position;
					length = sub.x * sub.x + sub.z * sub.z;
					if( length > itv)
					{
						/*! 距離が離れているものは判定しない */
						continue;
					}
					sub = sub.normalized;
					for( i2 = 0; i2 < direction.Length; i2++)
					{
						tmp_f = sub.x * direction[ i2].x + sub.z * direction[ i2].z;
						if( tmp_f <= theta)
						{
							/*! 角度の条件を満たしていない */
							continue;
						}
						if( min[ i2] <= length)
						{
							/*! すでに設定しているものより遠い */
							continue;
						}
						min[ i2] = length;
						no[ i2] = i1;
					}
				}
				for( i2 = 0; i2 < direction.Length; i2++)
				{
					if( no[ i2] < 0)
					{
						/*! この方向に繋げる座標が無かった */
						continue;
					}
					tmp_i = no[ i2];
					tmp_point.SetConnection( point_list[ tmp_i]);
					point_list[ tmp_i].SetConnection( tmp_point);
				}
				random_count++;
			}
		}

		/**
		 * ポイントのリストを渡す
		*/
		public List<Point> GetPointList()
		{
			return PointList;
		}

		/**
		 * ポイントのリストから特定の属性だけ渡す
		 */
		 public List<Point> GetSpecificPointList( List<int> specific_list)
		 {
			 int i0, i1;
			 bool flg;
			 List<Point> ret_list = new List<Point>();
			 Point tmp_point;

			 for( i0 = 0; i0 < PointList.Count; i0++)
			 {
				flg = false;
				tmp_point = PointList[ i0];
				for( i1 = 0; i1 < specific_list.Count; i1++)
				{
					if( tmp_point.Attribute == specific_list[ i1])
					{
						flg = true;
						break;
					}
				}
				if( flg != false)
				{
					ret_list.Add( tmp_point);
				}
			 }

			 return ret_list;
		 }

		void Update()
		{
			if( Input.GetKeyDown( KeyCode.Z))
			{
				TestCalc2();
			}
		}

		System.Random RandomSystem;
		List<Point> PointList;
		List<FieldPoint> FieldPointList;
		int Seed = -1;

		List<GameObject> ObjectList;

		[SerializeField]
		GameObject[] ObjectTable = default;

		[SerializeField]
		SugorokuMapCreater SugorokuScript = default;

		[SerializeField]
		TownGenerator TownScript = default;
	}
}
