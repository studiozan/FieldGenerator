using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquareArea2
{
	public class SquareAreaGenerator2 : MonoBehaviour
	{
		/* 初期化 */
		public void initialize()
		{
			if( seed >= 0)
			{
				randomSystem = new System.Random( seed);
			}
			else
			{
				randomSystem = new System.Random();
			}

			int i0, i1, num;
			pointList = new List<Point>();
			massList = new List<Mass>();
			Vector3 tmp_vec = Vector3.zero;
			vecList = new List<Vector3>();

			/* 座標の生成 */
			num = 10;
			for( i0 = 0; i0 < num; i0++)
			{
				tmp_vec.x = interval * i0;
				for( i1 = 0; i1 < num; i1++)
				{
					tmp_vec.z = interval * i1;
					vecList.Add( tmp_vec);
				}
			}
			
			CreatePoint();
			Point tmp_point;
			for( i0 = 0; i0 < pointList.Count; i0++)
			{
				tmp_point = pointList[ i0];
				Debug.Log("point[" + i0 + "]:" + tmp_point.position);
				for( i1 = 0; i1< tmp_point.connectList.Count; i1++)
				{
					Debug.Log("[" + i1 + "]:" + tmp_point.connectList[ i1]);
				}
			}
		}

		void Start()
		{
			initialize();
		}

		void Update()
		{
			if( Input.GetKeyDown( KeyCode.Space))
			{
				initialize();
			}
		}

		/* ポイントの情報を設定する */
		void CreatePoint()
		{
			int i0, i1, i2;
			Vector3 sub;
			Vector3[] houkou = new Vector3[ 4];
			float tmp_f, tmp_f2, itv, rad = 0.7f;
			Point tmp_point;
			float[] min = new float[ 4];
			int[] no = new int[ 4];
			List<Vector3> tmp_list;

			houkou[ 0] = Vector3.forward;
			houkou[ 1] = Vector3.back;
			houkou[ 2] = Vector3.right;
			houkou[ 3] = Vector3.left;
			itv = interval * 1.5f;
			itv = itv * itv;

			/* 座標を設定 */
			for( i0 = 0; i0 < vecList.Count; i0++)
			{
				tmp_point = new Point();
				tmp_point.init( vecList[ i0], 0);
				min[ 0] = itv;		min[ 1] = itv;
				min[ 2] = itv;		min[ 3] = itv;
				no[ 0] = -1;	no[ 1] = -1;	no[ 2] = -1;	no[ 3] = -1;
				for( i1 = 0; i1 < vecList.Count; i1++)
				{
					if( i0 == i1)
					{
						continue;
					}
					sub = vecList[ i1] - tmp_point.position;
					tmp_f = sub.x * sub.x + sub.z * sub.z;
					sub = sub.normalized;
					if( tmp_f >= itv)
					{
						continue;
					}
					for( i2 = 0; i2 < houkou.Length; i2++)
					{
						tmp_f2 = sub.x * houkou[ i2].x + sub.z * houkou[ i2].z;
						if( tmp_f2 <= rad)
						{
							continue;
						}
						if( min[ i2] > tmp_f)
						{
							min[ i2] = tmp_f;
							no[ i2] = i1;
						}
					}
				}
				tmp_list = new List<Vector3>();
				for( i1 = 0; i1 < houkou.Length; i1++)
				{
					if( no[ i1] >= 0)
					{
						tmp_list.Add( vecList[ no[ i1]]);
					}
				}
				tmp_point.SetConnectList( tmp_list);
				pointList.Add( tmp_point);
			}
		}

		/* マスの繋がりを調べる */
		void CreateMassConnect()
		{
			int i0;
			float len = 10f;
			Vector3 tmp_vec = Vector3.zero;
			Mass tmp_mass;

			for( i0 = 0; i0 < vecList.Count; i0++)
			{
				tmp_mass = new Mass();
				tmp_vec.x = (i0 % 10) * len;
				tmp_vec.z = (i0 / 10) * len;
				tmp_mass.init( tmp_vec);
				massList.Add( tmp_mass);
			}
		}

		/* 同じ座標かどうか調べる */
		bool SameVec3( Vector3 vec1, Vector3 vec2)
		{
			bool ret = false;
			Vector3 tmp_vec;
			float tmp_f;

			tmp_vec = vec1 - vec2;
			tmp_f = tmp_vec.x * tmp_vec.x + tmp_vec.y * tmp_vec.y + tmp_vec.z * tmp_vec.z;
			if( tmp_f < 1f)
			{
				ret = true;
			}
			return ret;
		}

		System.Random randomSystem;		// ランダムシステム
		List<Vector3> vecList;
		List<Mass> massList;			// マスのリスト
		int[][] wallTbl;

		List<Point> pointList;			// 点のリスト
		
		int seed = -1;
		float interval = 10f;
	}

	/* 座標の繋がりや属性のクラス */
	public class Point
	{
		/* 初期化 */
		public void init( Vector3 vec, int ty)
		{
			position = vec;
			pointType = ty;
		}

		/* 繋がっている座標リストの設定 */
		public void SetConnectList( List<Vector3> list)
		{
			connectList = list;
		}

		public Vector3 position;			// 座標
		public List<Vector3> connectList;	// 繋がっている座標のリスト
		public int pointType;				// 属性
	}

	public class Mass
	{
		public void init( Vector3 vec)
		{
			position = vec;
			vecList = new List<Vector3>();
			vecListIdx = new List<int>();
			status = 0;
		}

		public Vector3 position;		// 座標
		public List<Vector3> vecList;	// この座標と繋がっているマスの座標のベクトル
		public List<int> vecListIdx;	// この座標と繋がっているマスのインデックス
		public int status;				// マスの状態
	}
}
