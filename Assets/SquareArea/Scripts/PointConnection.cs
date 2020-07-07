using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SquareArea;

namespace FieldGenerator
{
	public class PointConnection : MonoBehaviour
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
			VecList = new List<Vector3>();
			RiverVecList = new List<Vector3>();
			ObjectList = new List<GameObject>();

			TestCalc();
		}

		void VecCreate()
		{
			int i0, i1, num;
			float tmp_f, pow = 0.2f, half = pow * 0.5f;
			Vector3 tmp_vec = Vector3.zero;
			num = 10;
			for( i0 = 0; i0 < num; i0++)
			{
				tmp_f = (float)RandomSystem.NextDouble() * pow - half;
				tmp_vec.z = Interval * i0 + Interval * tmp_f;
				for( i1 = 0; i1 < num; i1++)
				{
					tmp_f = (float)RandomSystem.NextDouble() * pow - half;
					tmp_vec.x = Interval * i1 + Interval * tmp_f;
					VecList.Add( tmp_vec);
				}
			}
			
			for( i0 = 0; i0 < 2; i0++)
			{
				tmp_f = (float)RandomSystem.NextDouble() * pow - half;
				tmp_vec.z = Interval * i0 + Interval * tmp_f + 110f;
				for( i1 = 0; i1 < num; i1++)
				{
					tmp_f = (float)RandomSystem.NextDouble() * pow - half;
					tmp_vec.x = Interval * i1 + Interval * tmp_f;
					VecList.Add( tmp_vec);
				}
			}

			tmp_vec.z = 100f;
			for( i0 = 0; i0 < num; i0++)
			{
				tmp_vec.x = Interval * i0 * 2f - Interval * 5.5f;
				RiverVecList.Add(tmp_vec);
			}
		}

		void TestCalc()
		{
			PointList.Clear();
			VecList.Clear();
			RiverVecList.Clear();

			VecCreate();
			SetPoint();
			int[] tbl;
			tbl = new int[ 1];
			tbl[ 0] = 0;
			/*! 川を繋げる */
			SetConnection( PointList, Interval * 2, tbl);
			tbl = new int[ 2];
			tbl[ 0] = 2;
			tbl[ 1] = 3;
			/*! 道路を繋げる */
			SetConnection( PointList, Interval, tbl);
			/*! 橋を作る */
			tbl = new int[ 1];
			tbl[ 0] = 3;
			SetConnection( PointList, Interval * 3, tbl, 1.1f, -1);
			ObjectCreate();

#if true
			int i0, i1;
			Point tmp_point;
			for( i0 = 0; i0 < PointList.Count; i0++)
			{
				tmp_point = PointList[ i0];
				Debug.Log( $"point[ {i0} ]: {tmp_point.Position}");
				for( i1 = 0; i1 < tmp_point.ConnectionList.Count; i1++)
				{
					Debug.Log($"[{i1}]: {tmp_point.ConnectionList[ i1]}");
				}
			}
#endif
		}

		void ObjectCreate()
		{
			int i0, i1, tmp_i;
			GameObject obj, obj2;
			Point tmp_point;
			List<Vector2> vec_list = new List<Vector2>();
			MeshCreate mesh_script;
			int[] tbl = new int[ 4];
			tbl[ 0] = 1;
			tbl[ 2] = 2;
			tbl[ 3] = 2;

			for( i0 = 0; i0 < ObjectList.Count; i0++)
			{
				Destroy( ObjectList[ i0].gameObject);
			}
			ObjectList.Clear();

			for( i0 = 0; i0 < PointList.Count; i0++)
			{
				tmp_point = PointList[ i0];
				obj = Instantiate( ObjectTable[ 0]) as GameObject;
				obj.transform.localPosition = tmp_point.Position;
				obj.transform.parent = this.gameObject.transform;
				obj.name = "obj" + i0;
				ObjectList.Add( obj);

				tmp_i = tbl[ tmp_point.Attribute];
				if( tmp_point.ConnectionList == null)
				{
					continue;
				}
				for( i1 = 0; i1 < tmp_point.ConnectionList.Count; i1++)
				{
					obj2 = Instantiate( ObjectTable[ tmp_i]) as GameObject;
					mesh_script = obj2.GetComponent<MeshCreate>();
					vec_list.Clear();
					vec_list.Add( new Vector2( tmp_point.Position.x, tmp_point.Position.z));
					vec_list.Add( new Vector2( tmp_point.ConnectionList[ i1].x, tmp_point.ConnectionList[ i1].z));
					mesh_script.RoadCreatePoly( vec_list, 2, 0f, 0f);
					obj2.transform.parent = obj.transform;
				}
			}
		}

		void SetPoint()
		{
			int i0, tmp_i;
			Point tmp_point;

			PointList.Clear();
			for( i0 = 0; i0 < VecList.Count; i0++)
			{
				tmp_point = new Point();
				if( i0 >= 90 && i0 < 110)
				{
					tmp_i = 3;
				}
				else
				{
					tmp_i = 2;
				}
				tmp_point.Initialize( VecList[ i0], tmp_i);
				PointList.Add( tmp_point);
			}
			for( i0 = 0; i0 < RiverVecList.Count; i0++)
			{
				tmp_point = new Point();
				tmp_point.Initialize( RiverVecList[ i0], 0);
				PointList.Add( tmp_point);
			}
		}

		/**
		 * ポイントのリストを元に接続の処理を行う
		 * @param point_list	ポイントクラスのリスト
		 * @param interval		繋がる座標感の幅
		 * @param use_tbl		接続を行う属性のテーブル
		 * @param random		接続する確率
		 * @param max_num		接続する最大数。-1の場合は判定しない
		 */
		public static void SetConnection( List<Point> point_list, float interval, int[] use_tbl,
			float random = 1f, int max_num = -1)
		{
			int i0, i1, i2, tmp_i, random_count;
			float itv, length, theta = 0.707f, tmp_f, rand;
			bool flg;
			Vector3 sub;
			Point tmp_point;
			List<Vector3> tmp_list;
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
				for( i1 = 0; i1 < use_tbl.Length; i1++)
				{
					if( tmp_point.Attribute == use_tbl[ i1])
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
				tmp_list = new List<Vector3>();
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
					for( i2 = 0; i2 < use_tbl.Length; i2++)
					{
						if( point_list[ i1].Attribute == use_tbl[ i2])
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
						rand = (float)_random.NextDouble();
						if( rand > random)
						{
							/*! ランダムに判定しない */
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
					tmp_list.Add( point_list[ tmp_i].Position);
				}
				tmp_point.SetConnection( tmp_list);
				random_count++;
			}
		}

		void Update()
		{
			if( Input.GetKeyDown( KeyCode.Space))
			{
				TestCalc();
			}
		}

		System.Random RandomSystem;
		List<Vector3> VecList;
		List<Vector3> RiverVecList;
		List<Point> PointList;
		float Interval = 10f;
		int Seed = -1;

		List<GameObject> ObjectList;

		[SerializeField]
		GameObject[] ObjectTable = default;
	}
}
