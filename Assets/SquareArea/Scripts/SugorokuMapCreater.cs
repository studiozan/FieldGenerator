using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;
using SquareArea;

namespace SugorokuMap
{
	public class SugorokuMapCreater : MonoBehaviour
	{
		void Awake()
		{
			PointTypeList = new List<int>();
			SugorokuPointList = new List<FieldConnectPoint>();
			ObjectList = new List<GameObject>();

			RandomSystem = new System.Random();
			State = 0;
		}

		/**
		 * すごろくマップの生成
		 */
		public bool MapCreate()
		{
			List<FieldConnectPoint> tmp_list = new List<FieldConnectPoint>(), enable_list = new List<FieldConnectPoint>();
			FieldConnectPoint tmp_point;
			int idx, count = 0;

			/* リストを設定した後じゃないと処理しない */
			if( State != 1)
			{
				return false;
			}

			SugorokuPointList.Clear();
			tmp_point = new FieldConnectPoint();
			// スタート地点
			idx = 0;
			tmp_point.Initialize( PointList[ idx].Position, 0);
			tmp_point.Index = idx;
			PointTypeList[ idx] = 1;
			tmp_list.Add( tmp_point);
			enable_list.Add( tmp_point);

			while( enable_list.Count > 0)
			{
				PassCreate( PointList, tmp_list, enable_list, PointTypeList, 2);
				count++;
				if( count > 25)
				{
					enable_list.Clear();
				}
			}

			SugorokuPointList = tmp_list;

			int i0, i1;
			List<Vector2> vec_list = new List<Vector2>();
			FieldConnectPoint tmp_point2;
			GameObject obj, mass_obj;
			MeshCreate mesh_script;

			for( i0 = 0; i0 < ObjectList.Count; i0++)
			{
				Destroy( ObjectList[ i0].gameObject);
			}
			ObjectList.Clear();
			
			for( i0 = 0; i0 < tmp_list.Count; i0++)
			{
				tmp_point = tmp_list[ i0];
				mass_obj = Instantiate( ObjectTbl[ 0]) as GameObject;
				mass_obj.transform.localPosition = tmp_point.Position;
				mass_obj.transform.localScale = new Vector3( 7.5f, 1f, 7.5f);
				mass_obj.transform.parent = gameObject.transform;
				ObjectList.Add( mass_obj);
				for( i1 = 0; i1 < tmp_point.ConnectionList.Count; i1++)
				{
					tmp_point2 = tmp_point.ConnectionList[ i1];
					vec_list.Clear();
					vec_list.Add( new Vector2( tmp_point.Position.x, tmp_point.Position.z));
					vec_list.Add( new Vector2( tmp_point2.Position.x, tmp_point2.Position.z));
					obj = Instantiate( ObjectTbl[ 1]) as GameObject;
					obj.transform.parent = mass_obj.transform;
					ObjectList.Add( obj);
					mesh_script = obj.GetComponent<MeshCreate>();
					mesh_script.RoadCreatePoly( vec_list, 2, 0f, 0.1f);
				}
			}

			State = 2;

			return true;
		}

		/**
		 * 候補のリストから通路と部屋を作る処理
		 *
		 * @param list			マスとして使用可能なポイントのリスト
		 * @param save_list		すごろくマスとして新たに生成したポイントのリスト
		 * @param enable_list	伸ばす余地のあるすごろくマスのリスト
		 * @param use_type_list	マスの使用状況
		 * @param pass_num		通路の数。0の場合は通路作成後に部屋の生成を行う。
		 * @param use_enable_index	通路を伸ばすすごろくマスの指定。-1の場合はランダムなすごろくマスを使う。
		 */
		void PassCreate( List<FieldConnectPoint> list, List<FieldConnectPoint> save_list, List<FieldConnectPoint> enable_list, List<int> use_type_list,
			int pass_num = 1, int use_enable_index = -1)
		{
			int i0, idx, rand, count, tmp_i;
			FieldConnectPoint tmp_point, tmp_point2;
			List<int> tmp_list = new List<int>();
			bool flg = false;

			if( use_enable_index < 0)
			{
				rand = RandomSystem.Next( 0, enable_list.Count);
			}
			else
			{
				rand = use_enable_index;
			}
			idx = enable_list[ rand].Index;
			tmp_point = list[ idx];
			idx = rand;
			
			/*! 残りの接続数がいくつか調べる */
			count = tmp_point.ConnectionList.Count - enable_list[ idx].ConnectionList.Count;
			if( count > 0)
			{
				tmp_list.Clear();
				for( i0 = 0; i0 < tmp_point.ConnectionList.Count; i0++)
				{
					tmp_i = tmp_point.ConnectionList[ i0].Index;
					if( use_type_list[ tmp_i] == 0)
					{
						tmp_list.Add( i0);
					}
				}

				if( tmp_list.Count > 0)
				{
					/*! 通路として繋ぐ */
					rand = RandomSystem.Next( 0, tmp_list.Count);
					rand = tmp_list[ rand];
					tmp_point2 = new FieldConnectPoint();
					tmp_point2.Initialize( tmp_point.ConnectionList[ rand].Position, 0);
					tmp_point2.Index = tmp_point.ConnectionList[ rand].Index;
					use_type_list[ tmp_point2.Index] = 1;
					tmp_point2.SetConnection( enable_list[ idx]);
					enable_list[ idx].SetConnection( tmp_point2);
					enable_list.Add( tmp_point2);
					save_list.Add( tmp_point2);

					pass_num--;
					if( pass_num <= 0)
					{
						/*! 四角の部屋を作る */
						SquareRoomCreate( list, save_list, enable_list, use_type_list, save_list.Count - 1, 1);
					}
					else
					{
						/*! 通路を伸ばす */
						PassCreate( list, save_list, enable_list, use_type_list, pass_num, enable_list.Count - 1);
					}

					flg = true;
				}
			}
			
			if( flg == false)
			{
				enable_list.RemoveAt( idx);
			}
		}

		/**
		 * 四角の部屋を作る
		 *
		 * @param list			マスとして使用可能なポイントのリスト
		 * @param save_list		すごろくマスとして新たに生成したポイントのリスト
		 * @param enable_list	伸ばす余地のあるすごろくマスのリスト
		 * @param use_type_list	マスの使用状況
		 * @param save_idx		部屋を生成する起点となるすごろくマスのインデックス
		 * @param room_extend_num	部屋を拡張する回数
		 */
		void SquareRoomCreate( List<FieldConnectPoint> list, List<FieldConnectPoint> save_list, List<FieldConnectPoint> enable_list, List<int> use_type_list, int save_idx,
			int room_extend_num = 0)
		{
			FieldConnectPoint center_point, tmp_point, tmp_point2;
			List<FieldConnectPoint> room_mass_list = new List<FieldConnectPoint>();
			List<int> tmp_list = new List<int>(), tmp_list2 = new List<int>();
			int i0, i1, i2, i3, tmp_i, no;
			bool flg;

			int[] list_idx_tbl = new int[ 3];
			FieldConnectPoint[] field_tbl = new FieldConnectPoint[ 3];

			center_point = save_list[ save_idx];
			tmp_point = list[ center_point.Index];
			room_mass_list.Add( center_point);

			for( i0 = 0; i0 < tmp_point.ConnectionList.Count; i0++)
			{
				tmp_i = tmp_point.ConnectionList[ i0].Index;
				if( use_type_list[ tmp_i] == 0)
				{
					tmp_list.Add( tmp_i);
				}
			}

			if( tmp_list.Count <= 1)
			{
				return;
			}

			flg = false;
			/*! 2方向に伸ばして、伸ばした2方向で共通の点がある場合は伸ばす */
			for( i0 = 0; i0 < tmp_list.Count; i0++)
			{
				tmp_point = list[ tmp_list[ i0]];
				tmp_list2.Clear();
				/* 伸ばせる方向のインデックスを調べる */
				for( i1 = 0; i1 < tmp_point.ConnectionList.Count; i1++)
				{
					tmp_i = tmp_point.ConnectionList[ i1].Index;
					if( use_type_list[ tmp_i] == 0)
					{
						tmp_list2.Add( tmp_i);
					}
				}
				for( i1 = 0; i1 < tmp_list.Count; i1++)
				{
					if( i0 == i1)
					{
						continue;
					}
					tmp_point2 = list[ tmp_list[ i1]];
					for( i2 = 0; i2 < tmp_list2.Count; i2++)
					{
						for( i3 = 0; i3 < tmp_point2.ConnectionList.Count; i3++)
						{
							/* 2方向の伸ばした場所が共通のマスと繋がる場所がある場合（四角の4マスが繋がるような部屋を作る） */
							if( tmp_list2[ i2] == tmp_point2.ConnectionList[ i3].Index)
							{
								list_idx_tbl[ 0] = tmp_list[ i0];
								list_idx_tbl[ 1] = tmp_list[ i1];
								list_idx_tbl[ 2] = tmp_list2[ i2];
								flg = true;
								i0 = tmp_list.Count;
								i1 = tmp_point.ConnectionList.Count;
								i2 = tmp_list2.Count;
								i3 = tmp_point2.ConnectionList.Count;
							}
						}
					}
				}
			}

			if( flg != false)
			{
				/*! 新しいマス3つを生成する */
				for( i0 = 0; i0 < 3; i0++)
				{
					field_tbl[ i0] = new FieldConnectPoint();
					no = list_idx_tbl[ i0];
					field_tbl[ i0].Initialize( list[ no].Position, 0);
					field_tbl[ i0].Index = list[ no].Index;
					save_list.Add( field_tbl[ i0]);
					enable_list.Add( field_tbl[ i0]);
					use_type_list[ field_tbl[ i0].Index] = 1;
					room_mass_list.Add( field_tbl[ i0]);
				}
				/*! 新しく生成したマスを相互に接続する */
				field_tbl[ 0].SetConnection( center_point);
				center_point.SetConnection( field_tbl[ 0]);
				field_tbl[ 1].SetConnection( center_point);
				center_point.SetConnection( field_tbl[ 1]);
				field_tbl[ 0].SetConnection( field_tbl[ 2]);
				field_tbl[ 2].SetConnection( field_tbl[ 0]);
				field_tbl[ 1].SetConnection( field_tbl[ 2]);
				field_tbl[ 2].SetConnection( field_tbl[ 1]);
			}

			/* 部屋の拡張 */
			while( room_extend_num > 0)
			{
				ExtendRoom( list, save_list, enable_list, room_mass_list, use_type_list);
				room_extend_num--;
			}
		}

		/**
		 * 部屋を拡張する
		 *
		 * @param list			マスとして使用可能なポイントのリスト
		 * @param save_list		すごろくマスとして新たに生成したポイントのリスト
		 * @param enable_list	伸ばす余地のあるすごろくマスのリスト
		 * @param room_list		同じ部屋のすごろくマスのリスト
		 * @param use_type_list	マスの使用状況
		 */
		 void ExtendRoom( List<FieldConnectPoint> list, List<FieldConnectPoint> save_list, List<FieldConnectPoint> enable_list,
		 		List<FieldConnectPoint> room_list, List<int> use_type_list)
		 {
			List<int> idx_list = new List<int>();		// 拡張候補のマスのインデックス
			List<int> room_idx_list = new List<int>();	// 拡張候補のマスがどの部屋からなのかのインデックス
			List<int> extend_idx_list = new List<int>();	// さらに拡張する際の候補となるidx_listのインデックス
			FieldConnectPoint tmp_point, tmp_point2, tmp_point3;
			int i0, i1, idx, tmp_idx, rand;

			for( i0 = 0; i0 < room_list.Count; i0++)
			{
				idx = room_list[ i0].Index;
				tmp_point = list[ idx];
				for( i1 = 0; i1 < tmp_point.ConnectionList.Count; i1++)
				{
					tmp_idx = tmp_point.ConnectionList[ i1].Index;
					if( use_type_list[ tmp_idx] == 0)
					{
						room_idx_list.Add( i0);
						idx_list.Add( tmp_idx);
					}
				}
			}

			/* 拡張できるマスが無いので終了 */
			if( room_idx_list.Count <= 0)
			{
				return;
			}

			/* すごろくマスの生成 */
			rand = RandomSystem.Next( 0, room_idx_list.Count);
			idx = idx_list[ rand];
			tmp_point2 = room_list[ room_idx_list[ rand]];
			tmp_point = new FieldConnectPoint();
			tmp_point.Initialize( list[ idx].Position, 0);
			tmp_point.Index = list[ idx].Index;
			tmp_point.SetConnection( tmp_point2);
			tmp_point2.SetConnection( tmp_point);
			enable_list.Add( tmp_point);
			room_list.Add( tmp_point);
			save_list.Add( tmp_point);
			use_type_list[ tmp_point.Index] = 1;

			/* 伸ばしたマスからさらに伸ばした時に、部屋と繋がるマスがあるかどうか調べる */
			for( i0 = 0; i0 < list[ idx].ConnectionList.Count; i0++)
			{
				tmp_idx = list[ idx].ConnectionList[ i0].Index;
				for( i1 = 0; i1 < idx_list.Count; i1++)
				{
					if( idx_list[ i1] == tmp_idx)
					{
						extend_idx_list.Add( i1);
					}
				}
			}

			/* さらに拡張できるマスが無いので終了 */
			if( extend_idx_list.Count <= 0)
			{
				return;
			}
			rand = RandomSystem.Next( 0, extend_idx_list.Count);
			tmp_idx = extend_idx_list[ rand];
			idx = idx_list[ tmp_idx];
			tmp_point3 = room_list[ room_idx_list[ tmp_idx]];
			tmp_point2 = new FieldConnectPoint();
			tmp_point2.Initialize( list[ idx].Position, 0);
			tmp_point2.Index = list[ idx].Index;
			tmp_point3.SetConnection( tmp_point2);
			tmp_point2.SetConnection( tmp_point3);
			tmp_point.SetConnection( tmp_point2);
			tmp_point2.SetConnection( tmp_point);
			enable_list.Add( tmp_point2);
			room_list.Add( tmp_point2);
			save_list.Add( tmp_point2);
			use_type_list[ tmp_point2.Index] = 1;
		 }

		/**
		 * すごろく用のリストを返す
		 * リスト生成状態じゃない場合はnullを返す
		 */
		public List<FieldConnectPoint> GetPointList()
		{
			if( State != 2)
			{
				return null;
			}

			return SugorokuPointList;
		}

		public void SetPointList( List<FieldConnectPoint> list)
		{
			int i0;
			PointList = list;
			PointTypeList.Clear();
			for( i0 = 0; i0 < PointList.Count; i0++)
			{
				PointList[ i0].Index = i0;
				PointTypeList.Add( 0);
			}
			State = 1;
			
			MapCreate();
		}

		List<FieldConnectPoint> PointList;				/*! 道路の繋がりポイントリスト */
		List<FieldConnectPoint> SugorokuPointList;		/*! すごろく用ポイントリスト */
		List<int> PointTypeList;			/*! ポイントの使用状況リスト */
		List<GameObject> ObjectList;		/*! 表示に使っているオブジェクトのリスト */

		System.Random RandomSystem;

		int State;
		
		[SerializeField]
		GameObject[] ObjectTbl = default;
	}
}
