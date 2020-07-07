using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquareArea
{
	public class SquareAreaGenerator : MonoBehaviour
	{
		/* 初期化 */
		public void init()
		{
			if( seed >= 0)
			{
				randomSystem = new System.Random( seed);
			}
			else
			{
				randomSystem = new System.Random();
			}
		}

		public void Create()
		{
			int tmp_i, rand;
			Clear();
			massCnt = 0;
			PassCreate();

			while( massCnt < MassNum)
			{
				tmp_i = roomList.Count - 1;
				rand = randomSystem.Next( 0, RoomMinNum.Length);
				ExtendRoom2( roomList[ tmp_i], RoomMinNum[ rand]);
				if( massCnt < MassNum)
				{
					PassCreate();
				}
				Debug.Log("room:" + roomList.Count + " pass:" + passList.Count);
			}
			//ExtendRoomToPass( roomList[ 0]);

			ViewObj();
#if false
			int i0, i1;
			for( i0 = 0; i0 < passList.Count; i0++)
			{
				for( i1 = 0; i1 < passList[ i0].massList.Count; i1++)
				{
					Debug.Log("pass[" + i0 + "] mass[" + i1 + "]:" + passList[ i0].massList[ i1].position);
				}
			}
			for( i0 = 0; i0 < roomList.Count; i0++)
			{
				for( i1 = 0; i1 < roomList[ i0].connectPassMass.Count; i1++)
				{
					Debug.Log("room[" + i0 + "] connect[" + i1 + "] mass:" + roomList[ i0].connectPassMass[ i1].position + " pass:" + roomList[i0].connectPassList[ i1].massList[ 0].position);
				}
			}
#endif
		}

		void Clear()
		{
			posList.Clear();
			massList.Clear();
			passList.Clear();
			roomList.Clear();
		}

		/* 通路生成 */
		void PassCreate()
		{
			Room tmp_room;
			Mass tmp_mass;
			Pass tmp_pass;
			List<Mass> tmp_list = new List<Mass>();
			int tmp_i;

			if( posList.Count == 0)
			{
				/* スタート座標を設定 */
				posList.Add( StartPos);
				tmp_mass = new Mass();
				tmp_mass.init( StartPos, false);
				massList.Add( tmp_mass);
				tmp_list.Add( tmp_mass);
				tmp_pass = new Pass();
				tmp_pass.init( tmp_list);
				passList.Add( tmp_pass);

				ExtendPassToRoom( tmp_mass);
				massCnt++;
			}
			else
			{
				/* 部屋の外周から通路を伸ばす */
				tmp_i = roomList.Count - 1;
				tmp_room = roomList[ tmp_i];
				Debug.Log("room[" + tmp_i + "]");
				tmp_mass = ExtendRoomToPass( tmp_room);
				if( tmp_mass != null)
				{
					Debug.Log("PassToRoom");
					ExtendPassToRoom( tmp_mass);
				}
			}
		}

		/* 部屋を拡張する処理 */
		void ExtendRoom2( Room room, int size)
		{
			int i0, i1, x, y, num, tmp_i, tmp_i2;
			float half = massLength * 0.5f, tmp_f;
			Vector3 tmp_vec, min, max, init_vec, sub_vec;
			bool flg = false;
			List<int> i_list = new List<int>(), tmp_list = new List<int>();
			x = 1;
			y = 1;
			num = 1;

			init_vec = new Vector3( room.massList[ 0].position.x, room.massList[ 0].position.y, room.massList[ 0].position.z);
//			min = new Vector3( init_vec.x - half, init_vec.y, init_vec.z - half);
//			max = new Vector3( init_vec.x + half, init_vec.y, init_vec.z + half);
			min = new Vector3( init_vec.x, init_vec.y, init_vec.z);
			max = new Vector3( init_vec.x, init_vec.y, init_vec.z);

			/* 伸ばせる方向の確認 */
			CheckExtend( i_list, room, min, max);

			/* 縦の伸ばせる方向 */
			tmp_list.Clear();
			for( i0 = 0; i0 < i_list.Count; i0++)
			{
				if( i_list[ i0] < 2)
				{
					tmp_list.Add( i0);
				}
			}
			if( tmp_list.Count > 0)
			{
				tmp_i = randomSystem.Next( 0, tmp_list.Count);
				tmp_i = tmp_list[ tmp_i];
				CheckExtendRoom2( i_list[ tmp_i], ref min, ref max, x, y, room, ref num);
				y++;
			}
			else
			{
				CheckExtend( i_list, room, min, max);
			}
			/* 横の伸ばせる方向 */
			tmp_list.Clear();
			for( i0 = 0; i0 < i_list.Count; i0++)
			{
				if( i_list[ i0] >= 2)
				{
					tmp_list.Add( i0);
				}
			}
			if( tmp_list.Count > 0)
			{
				tmp_i = randomSystem.Next( 0, tmp_list.Count);
				tmp_i = tmp_list[ tmp_i];
				CheckExtendRoom2( i_list[ tmp_i], ref min, ref max, x, y, room, ref num);
				x++;
			}
			else
			{
				CheckExtend( i_list, room, min, max);
			}

			while( num < size)
			{
				flg = true;
				tmp_i = randomSystem.Next( 0, i_list.Count);
				tmp_i2 = num;
				if( i_list[ tmp_i] < 2)
				{
					tmp_i2 += x;
				}
				else
				{
					tmp_i2 += y;
				}
				if( tmp_i2 > size)
				{
					flg = false;
					Debug.Log("over");
				}
				if( flg != false)
				{
					flg = CheckExtendRoom2( i_list[ tmp_i], ref min, ref max, x, y, room, ref num);
				}
				if( flg == false)
				{
					i_list.RemoveAt( tmp_i);
					if( i_list.Count == 0)
					{
						break;
					}
				}
				else
				{
					if( i_list[ tmp_i] < 2)
					{
						y++;
					}
					else
					{
						x++;
					}
				}
			}

			min.x -= half;
			min.z -= half;
			max.x += half;
			max.z += half;
			Mass tmp_mass;
			flg = false;
			for( i0 = 0; i0 < y; i0++)
			{
				tmp_vec = new Vector3( min.x + half, min.y, min.z + half + massLength * i0);
				for( i1 = 0; i1 < x; i1++)
				{
					tmp_vec.x = min.x + half + massLength * i1;
					/* 通路と繋がっている部屋はすでに作られているので、同じ場所にマスを作らないように調べている */
					if( flg == false)
					{
						sub_vec = tmp_vec - init_vec;
						tmp_f = sub_vec.x * sub_vec.x + sub_vec.z * sub_vec.z;
						if( tmp_f < 1f)
						{
							flg = true;
							continue;
						}
					}
					tmp_mass = new Mass();
					tmp_mass.init( tmp_vec);
					room.massList.Add( tmp_mass);
				}
			}

			RoomConnectCalc( room, massLength * 1.5f);

			room.posRect[ 0] = min;
			room.posRect[ 1] = max;

			Debug.Log( "room:" + roomList.Count + " x:" + x + " y:" + y + " num:" + num);

			massCnt += num;
		}

		/* 伸ばせる方向を調べる */
		void CheckExtend( List<int> list, Room room, Vector3 min, Vector3 max)
		{
			int i0;
			Vector3 tmp_vec;
			
			list.Clear();
			for( i0 = 0; i0 < 4; i0++)
			{
				switch( i0)
				{
				case 0:
					tmp_vec = new Vector3( max.x, max.y, max.z);
					tmp_vec.z += massLength;
					break;
				case 1:
					tmp_vec = new Vector3( min.x, min.y, min.z);
					tmp_vec.z -= massLength;
					break;
				case 2:
					tmp_vec = new Vector3( max.x, max.y, max.z);
					tmp_vec.x += massLength;
					break;
				case 3:
					tmp_vec = new Vector3( min.x, min.y, min.z);
					tmp_vec.x -= massLength;
					break;
				default:
					tmp_vec = Vector3.zero;
					break;
				}
				if( room.posRect[ 0].x <= tmp_vec.x && room.posRect[ 1].x >= tmp_vec.x &&
					room.posRect[ 0].z <= tmp_vec.z && room.posRect[ 1].z >= tmp_vec.z)
				{
					list.Add( i0);
				}
			}
		}
		
		/* 部屋の領域だけ増やす処理 */
		bool CheckExtendRoom2( int type, ref Vector3 min, ref Vector3 max, int x, int y, Room room, ref int num)
		{
			bool flg = false;
			int i0, i1, x2, y2;
			float tmp_f;

			switch( type)
			{
				case 0:
					tmp_f = max.z + massLength;
					if( room.posRect[ 1].z >= tmp_f)
					{
						max.z = tmp_f;
						flg = true;
					}
					break;
				case 1:
					tmp_f = min.z - massLength;
					if( room.posRect[ 0].z <= tmp_f)
					{
						min.z = tmp_f;
						flg = true;
					}
					break;
				case 2:
					tmp_f = max.x + massLength;
					if( room.posRect[ 1].x >= tmp_f)
					{
						max.x = tmp_f;
						flg = true;
					}
					break;
				case 3:
					tmp_f = min.x - massLength;
					if( room.posRect[ 0].x <= tmp_f)
					{
						min.x = tmp_f;
						flg = true;
					}
					break;
				default:
					break;
			}

			if( flg == false)
			{
				return false;
			}

			if( type < 2)
			{
				y += 1;
				x2 = 0;
				y2 = y - 1;
			}
			else
			{
				x += 1;
				y2 = 0;
				x2 = x - 1;
			}

			for( i0 = x2; i0 < x; i0++)
			{
				for( i1 = y2; i1 < y; i1++)
				{
					num++;
				}
			}

			return true;
		}

		/* 通路から部屋を作る方向を決める */
		void ExtendPassToRoom( Mass mass)
		{
			Room tmp_room = new Room();
			Vector3 tmp_vec, tmp_vec2, sub_vec;
			List<Vector3> vec_list = new List<Vector3>();
			float tmp_f = massLength * 5.25f;
			float min_size = (massLength * 2) * (massLength * 2);
			float tmp_f2, tmp_min;
			List<int> rand_list = new List<int>();
			int no, tmp_i, i0;

			Debug.Log("mass:" + mass.position);
			CheckAllRangeExtendArea( vec_list, mass.position, tmp_f);

			no = -1;
			tmp_f2 = 0f;
			for( i0 = 0; i0 < vec_list.Count; i0+=2)
			{
				tmp_f = GetArea( vec_list[ i0], vec_list[ i0 + 1], massLength * 2f);
				/* 一番面積の大きい方を部屋にする */
				if( tmp_f2 < tmp_f)
				{
					tmp_f2 = tmp_f;
					no = i0 / 2;
				}
				if( tmp_f > min_size)
				{
					tmp_i = i0 / 2;
					rand_list.Add( tmp_i);
				}
			}

#if true
			/* ゴールに近い方を部屋にする */
			no = -1;
			tmp_min = float.MaxValue;
			for( i0 = 0; i0 < rand_list.Count; i0++)
			{
				tmp_i = rand_list[ i0] * 2;
				tmp_vec = (vec_list[ tmp_i] + vec_list[ tmp_i + 1]) * 0.5f;
				sub_vec = GoalPos - tmp_vec;
				tmp_f = sub_vec.x * sub_vec.x + sub_vec.z * sub_vec.z;
				if( tmp_min > tmp_f)
				{
					tmp_min = tmp_f;
					no = tmp_i / 2;
				}
			}
#endif
			/* 部屋を作るだけのマスが残っていないので、部屋を作らない */
			if( massCnt + 4 > MassNum)
			{
				return;
			}

			if( no >= 0)
			{
				tmp_i = no * 2;
				tmp_room.init( vec_list[ tmp_i], vec_list[ tmp_i + 1] );
				Mass tmp_mass = new Mass();
				tmp_vec2 = new Vector3( mass.position.x, mass.position.y, mass.position.z);
				switch( no)
				{
				case 0:
					tmp_vec2.z += massLength;
					break;
				case 1:
					tmp_vec2.z -= massLength;
					break;
				case 2:
					tmp_vec2.x += massLength;
					break;
				case 3:
					tmp_vec2.x -= massLength;
					break;
				default:
					break;
				}
				tmp_mass.init( tmp_vec2);
				tmp_room.massList.Add( tmp_mass);
				tmp_room.connectPassMass.Add( tmp_mass);
				massList.Add( tmp_mass);

				tmp_i = passList.Count - 1;
				tmp_room.connectPassList.Add( passList[ tmp_i]);
				roomList.Add( tmp_room);
				ConnectNearMass( passList[ tmp_i].massList, tmp_mass);
			}
		}

		/* 上下左右に伸ばせるエリアを調べる */
		void CheckAllRangeExtendArea( List<Vector3> list, Vector3 init_pos, float half_len)
		{
			int i0;
			Vector3 tmp_vec, tmp_vec2;

			/* 上 */
			tmp_vec = new Vector3( init_pos.x - half_len, 0f, init_pos.z);
			list.Add( tmp_vec);
			tmp_vec = new Vector3( init_pos.x + half_len, 0f, init_pos.z + half_len * 2);
			list.Add( tmp_vec);
			/* 下 */
			tmp_vec = new Vector3( init_pos.x - half_len, 0f, init_pos.z - half_len * 2);
			list.Add( tmp_vec);
			tmp_vec = new Vector3( init_pos.x + half_len, 0f, init_pos.z);
			list.Add( tmp_vec);
			/* 右 */
			tmp_vec = new Vector3( init_pos.x, 0f, init_pos.z - half_len);
			list.Add( tmp_vec);
			tmp_vec = new Vector3( init_pos.x + half_len * 2, 0f, init_pos.z + half_len);
			list.Add( tmp_vec);
			/* 左 */
			tmp_vec = new Vector3( init_pos.x - half_len * 2, 0f, init_pos.z - half_len);
			list.Add( tmp_vec);
			tmp_vec = new Vector3( init_pos.x, 0f, init_pos.z + half_len);
			list.Add( tmp_vec);

			for( i0 = 0; i0 < list.Count; i0++)
			{
				/* マップ境界の判定と補正 */
				list[ i0] = CheckArea( list[ i0]);
			}
#if true
			for( i0 = 0; i0 < 4; i0++)
			{
				/* 他の通路がないかどうか調べる */
				tmp_vec = list[ i0 * 2];
				tmp_vec2 = list[ i0 * 2 + 1];
				CheckPass( ref tmp_vec, ref tmp_vec2, i0 );
				list[ i0 * 2] = tmp_vec;
				list[ i0 * 2 + 1] = tmp_vec2;
			}
#endif
		}

		/* 外周に伸ばせる通路をランダムに選ぶ */
		Mass ExtendRoomToPass( Room room)
		{
			int i0, i1, i2, idx, tmp_i;
			Mass tmp_mass, ret = null;
			List<List<Mass>> mass_list = new List<List<Mass>>();
			List<Mass> tmp_list, tmp_list2 = new List<Mass>();
			Vector3 min, max, tmp_vec, sub_vec, extend_vec = Vector3.zero;
			float tmp_f, tmp_min;
			bool flg;
			List<int> i_list = new List<int>();
			List<Vector3> tmp_vec_list = new List<Vector3>();

			for( i0 = 0; i0 < 4; i0++)
			{
				switch( i0)
				{
				case 0:
					/* 上 */
					max = new Vector3( room.posRect[ 1].x, room.posRect[ 1].y, room.posRect[ 1].z);
					min = new Vector3( room.posRect[ 0].x, room.posRect[ 0].y, max.z - massLength);
					break;
				case 1:
					/* 下 */
					min = new Vector3( room.posRect[ 0].x, room.posRect[ 0].y, room.posRect[ 0].z);
					max = new Vector3( room.posRect[ 1].x, room.posRect[ 1].y, min.z + massLength);
					break;
				case 2:
					/* 右 */
					max = new Vector3( room.posRect[ 1].x, room.posRect[ 1].y, room.posRect[ 1].z);
					min = new Vector3( max.x - massLength, room.posRect[ 0].y, room.posRect[ 0].z);
					break;
				case 3:
					/* 左 */
					min = new Vector3( room.posRect[ 0].x, room.posRect[ 0].y, room.posRect[ 0].z);
					max = new Vector3( min.x + massLength, room.posRect[ 1].y, room.posRect[ 1].z);
					break;
				default:
					min = Vector3.zero;
					max = Vector3.zero;
					break;
				}
				tmp_list = new List<Mass>();
				for( i1 = 0; i1 < room.massList.Count; i1++)
				{
					tmp_mass = room.massList[ i1];
					flg = false;
					if( min.x <= tmp_mass.position.x && max.x >= tmp_mass.position.x &&
						min.z <= tmp_mass.position.z && max.z >= tmp_mass.position.z)
					{
						/* すでに他の通路と繋がっているマスは選択されないようにする */
						for( i2 = 0; i2 < room.connectPassMass.Count; i2++)
						{
							if( tmp_mass.CheckSameMass( room.connectPassMass[ i2]) != false)
							{
								flg = true;
								break;
							}
						}
						if( flg == false)
						{
							tmp_list.Add( tmp_mass);
						}
					}
				}
				mass_list.Add( tmp_list);
			}

			tmp_min = float.MaxValue;
			idx = -1;
			/* 伸ばす方向のリスト */
			for( i0 = 0; i0 < mass_list.Count; i0++)
			{
				tmp_list = mass_list[ i0];
				if( tmp_list.Count == 0)
				{
					continue;
				}
				tmp_vec = Vector3.zero;
				for( i1 = 0; i1 < tmp_list.Count; i1++)
				{
					tmp_vec += tmp_list[ i1].position;
				}
				tmp_vec = tmp_vec / (float)tmp_list.Count;
				CheckAllRangeExtendArea( tmp_vec_list, tmp_vec, massLength * 2);
				tmp_f = GetArea( tmp_vec_list[ i0 * 2], tmp_vec_list[ i0 * 2 + 1], massLength * 2);
				if( tmp_f <= 0)
				{
					continue;
				}
				sub_vec = GoalPos - tmp_vec;
				tmp_f = sub_vec.x * sub_vec.x + sub_vec.z * sub_vec.z;
				if( tmp_min > tmp_f)
				{
					tmp_min = tmp_f;
					idx = i0;
				}
			}

			Pass tmp_pass = new Pass();
			if( idx >= 0)
			{
				tmp_list = mass_list[ idx];
				tmp_i = randomSystem.Next( 0, tmp_list.Count);
				tmp_vec = new Vector3( tmp_list[ tmp_i].position.x, tmp_list[ tmp_i].position.y, tmp_list[ tmp_i].position.z);
				switch( idx)
				{
				case 0:
					/* 上 */
					extend_vec.z = massLength;
					break;
				case 1:
					/* 下 */
					extend_vec.z = -massLength;
					break;
				case 2:
					/* 右 */
					extend_vec.x = massLength;
					break;
				case 3:
					/* 左 */
					extend_vec.x = -massLength;
					break;
				default:
					break;
				}
				tmp_vec += extend_vec;
				tmp_mass = new Mass();
				tmp_mass.init( tmp_vec, false);
				tmp_list2.Add( tmp_mass);
				tmp_list[ tmp_i].connectList.Add( tmp_mass);
				tmp_mass.connectList.Add( tmp_list[ tmp_i]);

				tmp_vec += extend_vec;
				tmp_mass = new Mass();
				tmp_mass.init( tmp_vec, false);
				tmp_list2.Add( tmp_mass);
				tmp_mass.connectList.Add( tmp_list2[ 0]);
				tmp_list2[ 0].connectList.Add( tmp_mass);
				tmp_pass.init( tmp_list2);
				room.connectPassMass.Add( tmp_list[ tmp_i]);
				room.connectPassList.Add( tmp_pass);
				massList.AddRange( tmp_list2);
				passList.Add(tmp_pass);
				ret = tmp_mass;
				massCnt += 2;
			}

			return ret;
		}

		/* 部屋の中の接続処理 */
		void RoomConnectCalc( Room room, float itv, float check_rad = 45f)
		{
			int i0, i1, i2, tmp_i;
			int[] near_i_tbl = new int[ 4];
			float[] near_f_tbl = new float[ 4];
			float itv2 = itv * itv, tmp_f, rad;
			Mass tmp_mass, tmp_mass2;
			Vector3 sub_vec;

			for( i0 = 0; i0 < room.massList.Count; i0++)
			{
				for( i2 = 0; i2 < 4; i2++)
				{
					near_i_tbl[ i2] = -1;
					near_f_tbl[ i2] = itv2;
				}
				tmp_mass = room.massList[ i0];
				for( i1 = 0; i1 < room.massList.Count; i1++)
				{
					if( i0 == i1)
					{
						continue;
					}
					tmp_mass2 = room.massList[ i1];
					sub_vec = tmp_mass.position - tmp_mass2.position;
					rad = Mathf.Atan2( sub_vec.x, sub_vec.z) * Mathf.Rad2Deg;
					tmp_f = sub_vec.x * sub_vec.x + sub_vec.z * sub_vec.z;
					tmp_i = -1;
					if( rad >= -check_rad && rad <= check_rad)
					{
						tmp_i = 0;
					}
					else if( rad >= 90f - check_rad && rad <= 90f + check_rad)
					{
						tmp_i = 1;
					}
					else if( rad >= 180f - check_rad || rad <= -180f + check_rad)
					{
						tmp_i = 2;
					}
					else if( rad >= -90f - check_rad && rad <= -90f + check_rad)
					{
						tmp_i = 3;
					}
					if( tmp_i >= 0)
					{
						if( tmp_f < near_f_tbl[ tmp_i])
						{
							near_f_tbl[ tmp_i] = tmp_f;
							near_i_tbl[ tmp_i] = i1;
						}
					}
				}
				for( i1 = 0; i1 < 4; i1++)
				{
					tmp_i = near_i_tbl[ i1];
					if( tmp_i >= 0)
					{
						tmp_mass.connectList.Add( room.massList[ tmp_i]);
					}
				}
			}
		}

		/* エリア範囲内か調べて、範囲外なら補正を行う */
		Vector3 CheckArea( Vector3 vec)
		{
			Vector3 tmp_vec = new Vector3( vec.x, vec.y, vec.z);
			bool flg = false;

			if( MapSize.x > vec.x)
			{
				vec.x = MapSize.x;
				flg = true;
			}
			else if( MapSize.x + MapSize.width < vec.x)
			{
				vec.x = MapSize.x + MapSize.width;
				flg = true;
			}
			if( MapSize.y > vec.z)
			{
				vec.z = MapSize.y;
				flg = true;
			}
			else if( MapSize.y + MapSize.height < vec.z)
			{
				vec.z = MapSize.y + MapSize.height;
				flg = true;
			}

			if( flg != false)
			{
				Debug.Log("old:" + tmp_vec + " new:" + vec);
			}

			return vec;
		}

		/* 通路と被っているかどうか調べる */
		void CheckPass( ref Vector3 min, ref Vector3 max, int houkou)
		{
			bool ret;
			int i0, i1;
			float tmp_f, tmp_f2;
			Vector3 tmp_vec;

			for( i0 = 0; i0 < passList.Count; i0++)
			{
				for( i1 = 0; i1 < passList[ i0].massList.Count; i1++)
				{
					ret = false;
					tmp_vec = passList[ i0].massList[ i1].position;
					if( tmp_vec.x > min.x && tmp_vec.x < max.x &&
						tmp_vec.z > min.z && tmp_vec.z < max.z)
					{
						ret = true;
					}
					if( ret != false)
					{
						Debug.Log("pass[" + i0 + "][" + i1 + "]:" + tmp_vec + " min:" + min + " max:" + max + " houkou:" + houkou);
						switch( houkou)
						{
						case 0:
							max.z = tmp_vec.z;
							tmp_f = tmp_vec.x - min.x;
							tmp_f2 = tmp_vec.x - max.x;
							if( tmp_f < 0)
							{
								tmp_f = -tmp_f;
							}
							if( tmp_f2 < 0)
							{
								tmp_f2 = -tmp_f2;
							}
							if( tmp_f < tmp_f2)
							{
								min.x = tmp_vec.x;
							}
							else
							{
								max.x = tmp_vec.x;
							}
							break;
						case 1:
							min.z = tmp_vec.z;
							tmp_f = tmp_vec.x - min.x;
							tmp_f2 = tmp_vec.x - max.x;
							if( tmp_f < 0)
							{
								tmp_f = -tmp_f;
							}
							if( tmp_f2 < 0)
							{
								tmp_f2 = -tmp_f2;
							}
							if( tmp_f < tmp_f2)
							{
								min.x = tmp_vec.x;
							}
							else
							{
								max.x = tmp_vec.x;
							}
							break;
						case 2:
							max.x = tmp_vec.x;
							tmp_f = tmp_vec.z - min.z;
							tmp_f2 = tmp_vec.z - max.z;
							if( tmp_f < 0)
							{
								tmp_f = -tmp_f;
							}
							if( tmp_f2 < 0)
							{
								tmp_f2 = -tmp_f2;
							}
							if( tmp_f < tmp_f2)
							{
								min.z = tmp_vec.z;
							}
							else
							{
								max.z = tmp_vec.z;
							}
							break;
						case 3:
							min.x = tmp_vec.x;
							tmp_f = tmp_vec.z - min.z;
							tmp_f2 = tmp_vec.z - max.z;
							if( tmp_f < 0)
							{
								tmp_f = -tmp_f;
							}
							if( tmp_f2 < 0)
							{
								tmp_f2 = -tmp_f2;
							}
							if( tmp_f < tmp_f2)
							{
								min.z = tmp_vec.z;
							}
							else
							{
								max.z = tmp_vec.z;
							}
							break;
							default:
							break;
						}
						Debug.Log("pass:" + tmp_vec + " min:" + min + " max:" + max);
					}
				}
			}
		}

		/* 矩形の面積を求める */
		float GetArea( Vector3 min, Vector3 max, float min_size)
		{
			float ret = 0f, x, y;

			x = max.x - min.x;
			if( x < 0)
			{
				x = -x;
			}
			/* 辺の長さが最低サイズ以下なら0とする */
			if( x < min_size)
			{
				x = 0;
			}
			y = max.z - min.z;
			if( y < 0)
			{
				y = -y;
			}
			/* 辺の長さが最低サイズ以下なら0とする */
			if( y < min_size)
			{
				y = 0;
			}

			ret = x * y;

			return ret;
		}

		/* 一番近いマスに繋げる */
		void ConnectNearMass( List<Mass> list, Mass mass)
		{
			int i0, no;
			float tmp_f, min;
			Vector3 tmp_vec;

			no = -1;
			min = float.MaxValue;
			for( i0 = 0; i0 < list.Count; i0++)
			{
				tmp_vec = mass.position - list[ i0].position;
				tmp_f = tmp_vec.x * tmp_vec.x + tmp_vec.z * tmp_vec.z;
				if( min > tmp_f)
				{
					no = i0;
					min = tmp_f;
				}
			}

			if( no >= 0)
			{
				mass.connectList.Add( list[ no]);
				list[ no].connectList.Add( mass);
			}
		}

		/* とりあえずマスのある場所にオブジェクトを作る */
		void ViewObj()
		{
			GameObject obj, obj0 = null;
			MeshCreate mesh_scr;
			Pass tmp_pass;
			Room tmp_room;
			Mass tmp_mass;
			int i0, i1, i2;
			List<Vector2> tmp_list = new List<Vector2>();

			for( i0 = _obj_list.Count - 1; i0 >= 0; i0--)
			{
				Destroy( _obj_list[ i0]);
			}
			_obj_list.Clear();

			for( i0 = 0; i0 < passList.Count; i0++)
			{
				tmp_pass = passList[ i0];
				for( i1 = 0; i1 < tmp_pass.massList.Count; i1++ )
				{
					tmp_mass = tmp_pass.massList[ i1];
					obj = Instantiate( prefabTbl[ 0]) as GameObject;
					obj.name = "pass_" + i0 + "_mass_" + i1;
					obj.transform.localPosition = tmp_mass.position;
					_obj_list.Add( obj);
					obj.transform.parent = this.transform;
					tmp_mass.massObject = obj;
					if( i1 == 0)
					{
						obj0 = obj;
					}
					else if( obj0 != null)
					{
						obj.transform.parent = obj0.transform;
					}
					for( i2 = 0; i2 < tmp_mass.connectList.Count; i2++)
					{
						tmp_list.Clear();
						tmp_list.Add( new Vector2( tmp_mass.position.x, tmp_mass.position.z));
						tmp_list.Add( new Vector2( tmp_mass.connectList[ i2].position.x, tmp_mass.connectList[ i2].position.z));
						obj = Instantiate( prefabTbl[ 1]) as GameObject;
						mesh_scr = obj.GetComponent<MeshCreate>();
						mesh_scr.RoadCreatePoly( tmp_list, 2, 0f, 0f);
						_obj_list.Add( obj);
						obj.transform.parent = tmp_mass.massObject.transform;
					}
				}
			}
			
			for( i0 = 0; i0 < roomList.Count; i0++)
			{
				tmp_room = roomList[ i0];
				for( i1 = 0; i1 < tmp_room.massList.Count; i1++ )
				{
					tmp_mass = tmp_room.massList[ i1];
					obj = Instantiate( prefabTbl[ 0]) as GameObject;
					obj.name = "room_" + i0 + "_mass_" + i1;
					obj.transform.localPosition = tmp_mass.position;
					_obj_list.Add( obj);
					obj.transform.parent = this.transform;
					tmp_mass.massObject = obj;
					if( i1 == 0)
					{
						obj0 = obj;
					}
					else if( obj0 != null)
					{
						obj.transform.parent = obj0.transform;
					}
					for( i2 = 0; i2 < tmp_mass.connectList.Count; i2++)
					{
						tmp_list.Clear();
						tmp_list.Add( new Vector2( tmp_mass.position.x, tmp_mass.position.z));
						tmp_list.Add( new Vector2( tmp_mass.connectList[ i2].position.x, tmp_mass.connectList[ i2].position.z));
						obj = Instantiate( prefabTbl[ 2]) as GameObject;
						mesh_scr = obj.GetComponent<MeshCreate>();
						mesh_scr.RoadCreatePoly( tmp_list, 2, 0f, 0f);
						_obj_list.Add( obj);
						obj.transform.parent = tmp_mass.massObject.transform;
					}
				}
			}

			Debug.Log("mass cnt:" + massCnt);
		}

		System.Random randomSystem;		// ランダムシステム
		List<Vector3> posList = new List<Vector3>();	// 座標リスト
		List<Mass> massList = new List<Mass>();			// マスのリスト
		List<Pass> passList = new List<Pass>();			// パスのリスト
		List<Room> roomList = new List<Room>();			// 部屋のリスト
		int massCnt;		// マスの数
		float massLength = 10f;		// マス同士の間隔

		List<GameObject> _obj_list = new List<GameObject>();

		[SerializeField]
		Rect MapSize = new Rect( -100, -50, 200, 100);	// マップサイズ
		[SerializeField]
		Vector3 StartPos = new Vector3( -90, 0, 0);		// スタート位置
		[SerializeField]
		Vector3 GoalPos = new Vector3( 90, 0, 0);		// ゴール位置
		[SerializeField]
		int MassNum = 24;		// マスの数
		[SerializeField]
		int[] RoomMinNum = { 4, 6, 8, 9};		// 部屋のサイズパターン

		[SerializeField]
		GameObject[] prefabTbl = default;

		int seed = -1;
	}

	/* マス */
	public class Mass
	{
		/* 初期化 */
		public void init( Vector3 pos, bool room = true)
		{
			position = pos;
			connectList = new List<Mass>();
			isRoom = room;
		}

		/* 繋がりの設定 */
		public void SetConnect( Mass mass)
		{
			int i0;
			Vector3 tmp_vec = Vector3.zero;
			Mass tmp_mass;
			bool flg = true;

			/* 同じマスが無いかどうか調べる */
			for( i0 = 0; i0 < connectList.Count; i0++)
			{
				tmp_mass = connectList[ i0];
				if( CheckSameMass( tmp_mass) != false)
				{
					flg = false;
					break;
				}
			}

			/* 同じマスが無かったので追加する */
			if( flg != false)
			{
				connectList.Add( mass);
			}
		}

		/* 同じマスかどうか調べる */
		public bool CheckSameMass( Mass mass)
		{
			bool ret = false;
			Vector3 tmp_vec;
			float tmp_f;

			tmp_vec = mass.position - this.position;
			tmp_f = tmp_vec.x * tmp_vec.x + tmp_vec.z * tmp_vec.z;
			if( tmp_f < 1f)
			{
				ret = true;
			}

			return ret;
		}

		public Vector3 position;		// 座標
		public List<Mass> connectList;	// 別のマスとの繋がり
		public bool isRoom;				// 部屋かどうか

		public GameObject massObject;	// オブジェクト
	}

	/* 通路 */
	public class Pass
	{
		public void init( List<Mass> mass_list)
		{
			posList = new List<Vector3>();
			massList = new List<Mass>();
			connectRoomMass = new List<Mass>();
			connectRoomList = new List<Room>();

			massList.AddRange( mass_list);
		}

		public List<Vector3> posList;		// 通路の座標リスト
		public List<Mass> massList;			// 通路のマスのリスト
		public List<Mass> connectRoomMass;	// 部屋と繋がっているマスのリスト
		public List<Room> connectRoomList;	// 繋がっている部屋のリスト
	}

	/* 部屋 */
	public class Room
	{
		public void init( Vector3 min_vec, Vector3 max_vec)
		{
			posRect = new Vector3[ 2];
			massList = new List<Mass>();
			connectPassMass = new List<Mass>();
			connectPassList = new List<Pass>();

			posRect[ 0] = new Vector3( min_vec.x, min_vec.y, min_vec.z);
			posRect[ 1] = new Vector3( max_vec.x, max_vec.y, max_vec.z);
		}

		public Vector3[] posRect = new Vector3[ 2];			// 部屋のRect座標
		public List<Mass> massList = new List<Mass>();		// 部屋のマスのリスト
		public List<Mass> connectPassMass = new List<Mass>();	// 通路と繋がっているマスのリスト
		public List<Pass> connectPassList = new List<Pass>();	// 繋がっている通路のリスト
	}
}	// namespace SquareArea