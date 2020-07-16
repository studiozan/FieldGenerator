using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class PointConnection
	{
		/**
		 * 初期化処理
		 */
		public void Initialize( int seed = -1)
		{
			if( seed >= 0)
			{
				randomSystem = new System.Random( seed);
			}
			else
			{
				randomSystem = new System.Random();
			}
			riverConnectPointList = new List<FieldConnectPoint>();
			roadConnectPointList = new List<FieldConnectPoint>();
			sugorokuConnectPointList = new List<FieldConnectPoint>();
		}

		/**
		 * フィールド座標の接続処理をする
		 * 川と全ての道路とすごろく用道路のリストを作成する。作成するリストは今後増減する可能性がある。
		 * @param field_list	接続を行うフィールドポイントのリスト
		 * @param interval		接続を行う座標の間隔
		 */
		public void FieldConnectCreate( List<FieldPoint> field_list, float interval, float river_interval = 10f)
		{
			/*! フィールドの座標情報のリストを設定 */
			fieldPointList = field_list;

			SetFieldPoint();
			List<int> list = new List<int>();

			/*! 川を繋げる */
			list.Add( (int)PointType.kRiver);
			SetConnection( riverConnectPointList, river_interval, list, true);
			/*! 道路を全て繋げる */
			list.Clear();
			list.Add((int)PointType.kRoadAlongRiver);
			list.Add((int)PointType.kGridRoad);
			list.Add((int)PointType.kDistrictRoad);
			list.Add((int)PointType.kIntersectionOfGridRoadAndRoadAlongRiver);
			list.Add((int)PointType.kIntersectionOfGridRoadAndDistrictRoad);
			list.Add((int)PointType.kIntersectionOfRoadAlongRiverAndDistrictRoad);
			SetConnection( roadConnectPointList, interval, list);
			/*! すごろくで使う道路を繋げる */
			list.Clear();
			list.Add((int)PointType.kGridRoad);
			list.Add((int)PointType.kIntersectionOfGridRoadAndRoadAlongRiver);
			list.Add((int)PointType.kIntersectionOfGridRoadAndDistrictRoad);
			SetConnection( sugorokuConnectPointList, interval, list);
		}

		/**
		 * フィールドの座標リストを接続クラスに変えてリストにする
		 */
		void SetFieldPoint()
		{
			int i0, i1;
			FieldPoint tmp_field;
			FieldConnectPoint tmp_point;
			List<FieldConnectPoint> tmp_list;
			bool flg;

			riverConnectPointList.Clear();
			roadConnectPointList.Clear();
			sugorokuConnectPointList.Clear();
			
			for( i0 = 0; i0 < fieldPointList.Count; i0++)
			{
				tmp_field = fieldPointList[ i0];
				for( i1 = 0; i1 < 3; i1++)
				{
					flg = false;
					switch( i1)
					{
					case 0:
						tmp_list = riverConnectPointList;
						if( tmp_field.Type == PointType.kRiver)
						{
							flg = true;
						}
						break;
					case 1:
						tmp_list = roadConnectPointList;
						if( tmp_field.Type == PointType.kRoadAlongRiver ||
							tmp_field.Type == PointType.kGridRoad ||
							tmp_field.Type == PointType.kDistrictRoad ||
							tmp_field.Type == PointType.kIntersectionOfGridRoadAndRoadAlongRiver ||
							tmp_field.Type == PointType.kIntersectionOfGridRoadAndDistrictRoad ||
							tmp_field.Type == PointType.kIntersectionOfRoadAlongRiverAndDistrictRoad)
						{
							flg = true;
						}
						break;
					case 2:
						tmp_list = sugorokuConnectPointList;
						if( tmp_field.Type == PointType.kGridRoad ||
							tmp_field.Type == PointType.kIntersectionOfGridRoadAndRoadAlongRiver ||
							tmp_field.Type == PointType.kIntersectionOfGridRoadAndDistrictRoad)
						{
							flg = true;
						}
						break;
					default:
						tmp_list = riverConnectPointList;
						flg = false;
						break;
					}
					if( flg != false)
					{
						tmp_point = new FieldConnectPoint();
						tmp_point.Initialize( tmp_field.Position, tmp_field.Type);
						tmp_list.Add( tmp_point);
					}
				}
			}
		}

		/**
		 * ポイントのリストを元に接続の処理を行う
		 * @param point_list	ポイントクラスのリスト
		 * @param interval		繋がる座標感の幅
		 * @param specific_list	接続を行う属性のテーブル
		 * @param in_order		順番通りに繋げるフラグ。川のように座標が順番に生成されるような時にtrueにする
		 * @param random		接続する確率
		 * @param max_num		接続する最大数。-1の場合は判定しない
		 */
		public void SetConnection( List<FieldConnectPoint> point_list, float interval, List<int> specific_list, bool in_order = false,
			float random = 1f, int max_num = -1)
		{
			int i0, i1, i2, tmp_i, random_count, count;
			float itv, length, theta = 0.707f, tmp_f, rand;
			bool flg, order_flg;
			Vector3 sub;
			FieldConnectPoint tmp_point;
			Vector3[] direction = new Vector3[ 4];
			float[] min = new float[ 4];
			int[] no = new int[ 4];
			itv = interval * 1.5f;
			itv = itv * itv;
			random_count = 0;

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
					if( (int)tmp_point.Type == specific_list[ i1])
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
				rand = (float)randomSystem.NextDouble();
				if( rand > random)
				{
					/*! ランダムに判定しない */
					continue;
				}
				// TODO: 4方向だけに繋げるための初期化だが、すでに接続してる物がある場合を考慮していなかったので、繋げているものがある場合はその値で初期化するように変更したい
				min[ 0] = itv;	min[ 1] = itv;	min[ 2] = itv;	min[ 3] = itv;
				no[ 0] = -1;	no[ 1] = -1;	no[ 2] = -1;	no[ 3] = -1;
				order_flg = false;
				count = tmp_point.ConnectionList.Count;
				for( i1 = 0; i1 < point_list.Count; i1++)
				{
					if( in_order != false && count > 0)
					{
						/*! 順番通りの場合は次の座標と判断する */
						if( order_flg == false)
						{
							if( i0 < i1)
							{
								break;
							}
							i1 = i0 + 1;
							if( i1 >= point_list.Count)
							{
								break;
							}
							order_flg = true;
						}
					}
					if( i0 == i1)
					{
						/*! 同じものは判定しない */
						continue;
					}
					flg = false;
					for( i2 = 0; i2 < specific_list.Count; i2++)
					{
						if( (int)point_list[ i1].Type == specific_list[ i2])
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
					flg = false;
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
						flg = true;
					}
					if( flg != false)
					{
						count++;
					}
					if( in_order != false)
					{
						if( order_flg != false)
						{
							i1 = point_list.Count;
						}
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
		 * 川のリストを渡す
		*/
		public List<FieldConnectPoint> GetRiverConnectPointList()
		{
			return riverConnectPointList;
		}

		/**
		 * 道路のリストを渡す
		*/
		public List<FieldConnectPoint> GetRoadConnectPointList()
		{
			return roadConnectPointList;
		}

		/**
		 * すごろくのリストを渡す
		*/
		public List<FieldConnectPoint> GetSugorokuConnectPointList()
		{
			return sugorokuConnectPointList;
		}

		System.Random randomSystem;
		List<FieldPoint> fieldPointList;
		/*! 川の接続リスト */
		List<FieldConnectPoint> riverConnectPointList;
		/*! 全ての道路の接続リスト */
		List<FieldConnectPoint> roadConnectPointList;
		/*! すごろくで使用する接続リスト */
		List<FieldConnectPoint> sugorokuConnectPointList;
	}
}
