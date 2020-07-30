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
		 * @param fieldList		接続を行うフィールドポイントのリスト
		 * @param interval		接続を行う道路の座標の間隔
		 * @param riverInterval	接続を行う川の座標の間隔
		 * @param sugorokuMergeMulti	すごろく用の接続座標を間引く範囲の倍率
		 * @param sugorokuOfset	すごろく用の接続座標を外周から指定された距離分を間引く時の値
		 */
		public void FieldConnectCreate( List<FieldPoint> fieldList, float interval, float riverInterval = 10f, float sugorokuMergeMulti = 1.75f,
			float sugorokuOfset = 250f)
		{
			/* フィールドの座標情報のリストを設定 */
			fieldPointList = fieldList;

			SetFieldPoint();

			/* 川を繋げる */
			SetConnection( riverConnectPointList, riverInterval, true);
			/* 道路を全て繋げる */
			SetConnection( roadConnectPointList, interval);
			/* すごろくで使う道路を繋げる */
			float power = sugorokuMergeMulti;
			if( power < 1f)
			{
				power = 1f;
			}
			SetConnection( sugorokuConnectPointList, interval * ( power * 2f - 1f), false, interval * power, sugorokuOfset);
		}

		/**
		 * フィールドの座標リストを接続クラスに変えてリストにする
		 */
		void SetFieldPoint()
		{
			int i0, i1;
			FieldConnectPoint createPoint;
			List<FieldConnectPoint> addList;
			bool flg;

			riverConnectPointList.Clear();
			roadConnectPointList.Clear();
			sugorokuConnectPointList.Clear();
			
			for( i0 = 0; i0 < fieldPointList.Count; ++i0)
			{
				for( i1 = 0; i1 < 3; ++i1)
				{
					flg = false;
					switch( i1)
					{
					case 0:
						addList = riverConnectPointList;
						if( fieldPointList[ i0].Type == PointType.kRiver)
						{
							flg = true;
						}
						break;
					case 1:
						addList = roadConnectPointList;
						if( fieldPointList[ i0].Type == PointType.kRoadAlongRiver ||
							fieldPointList[ i0].Type == PointType.kGridRoad ||
							fieldPointList[ i0].Type == PointType.kDistrictRoad ||
							fieldPointList[ i0].Type == PointType.kIntersectionOfGridRoadAndRoadAlongRiver ||
							fieldPointList[ i0].Type == PointType.kIntersectionOfGridRoadAndDistrictRoad ||
							fieldPointList[ i0].Type == PointType.kIntersectionOfRoadAlongRiverAndDistrictRoad)
						{
							flg = true;
						}
						break;
					case 2:
						addList = sugorokuConnectPointList;
						if( fieldPointList[ i0].Type == PointType.kGridRoad ||
							fieldPointList[ i0].Type == PointType.kIntersectionOfGridRoadAndRoadAlongRiver ||
							fieldPointList[ i0].Type == PointType.kIntersectionOfGridRoadAndDistrictRoad)
						{
							flg = true;
						}
						break;
					default:
						addList = riverConnectPointList;
						flg = false;
						break;
					}
					if( flg != false)
					{
						createPoint = new FieldConnectPoint();
						createPoint.Initialize( fieldPointList[ i0].Position, fieldPointList[ i0].Type);
						addList.Add( createPoint);
					}
				}
			}
		}

		/**
		 * ポイントのリストを元に接続の処理を行う
		 * @param pointList	ポイントクラスのリスト
		 * @param interval		繋がる座標感の幅
		 * @param inOrder		順番通りに繋げるフラグ。川のように座標が順番に生成されるような時にtrueにする
		 * @param mergeSize		頂点を融合させる時の判定サイズ
		 * @param ofsetSize		外周からのオフセットサイズ、外周からこのサイズ分内側でのみ接続を行う
		 * @param random		接続する確率
		 * @param maxNum		接続する最大数。-1の場合は判定しない
		 */
		public void SetConnection( List<FieldConnectPoint> pointList, float interval, bool inOrder = false, float mergeSize = 0f,
			float ofsetSize = 0f, float random = 1f, int maxNum = -1)
		{
			int i0, i1, i2, index, randomCount, count;
			float itv, length, checkTheta = 0.707f, theta, rand;
			bool flg, orderFlag;
			Vector3 sub;
			FieldConnectPoint currentPoint;
			var direction = new Vector3[ 4];
			var min = new float[ 4];
			var no = new int[ 4];
			itv = interval * 1.5f;
			itv = itv * itv;
			randomCount = 0;

			direction[ 0] = Vector3.forward;
			direction[ 1] = Vector3.back;
			direction[ 2] = Vector3.right;
			direction[ 3] = Vector3.left;

			if( mergeSize > 0f)
			{
				/* 頂点を間引く処理 */
				FieldPointMerge( pointList, mergeSize);
			}
			/* 外周から一定範囲の点を間引く処理 */
			if( ofsetSize > 0f)
			{
				OuterPointThinning( pointList, ofsetSize);
			}
			for( i0 = 0; i0 < pointList.Count; ++i0)
			{
				currentPoint = pointList[ i0];
				if( maxNum >= 0 && maxNum <= randomCount)
				{
					/* ランダムで作る最大数に達しているので処理を終わる */
					return;
				}
				rand = (float)randomSystem.NextDouble();
				if( rand > random)
				{
					/* ランダムに判定しない */
					continue;
				}
				// TODO: 4方向だけに繋げるための初期化だが、すでに接続してる物がある場合を考慮していなかったので、繋げているものがある場合はその値で初期化するように変更したい
				//			試してみた結果、繋がりそうで繋がっていない場所が多数発生したので、別の方法を考える。
				min[ 0] = itv;	min[ 1] = itv;	min[ 2] = itv;	min[ 3] = itv;
				no[ 0] = -1;	no[ 1] = -1;	no[ 2] = -1;	no[ 3] = -1;
				orderFlag = false;
				count = currentPoint.ConnectionList.Count;
				for( i1 = 0; i1 < pointList.Count; ++i1)
				{
					if( inOrder != false && count > 0)
					{
						/* 順番通りの場合は次の座標と判断する */
						if( orderFlag == false)
						{
							if( i0 < i1)
							{
								break;
							}
							i1 = i0 + 1;
							if( i1 >= pointList.Count)
							{
								break;
							}
							orderFlag = true;
						}
					}
					if( i0 == i1)
					{
						/* 同じものは判定しない */
						continue;
					}
					if( currentPoint.Type == PointType.kRoadAlongRiver)
					{
						if( pointList[ i1].Type == PointType.kRoadAlongRiver)
						{
							/* 川沿いの道路は川と同じように前後の座標と結ぶようにする */
							if( i1 != i0 + 1)
							{
								continue;
							}
						}
						else
						{
							/* 川沿いから繋ぐ場合は、川沿いの道路とだけ接続する */
							continue;
						}
					}
					sub = pointList[ i1].Position - currentPoint.Position;
					length = sub.x * sub.x + sub.z * sub.z;
					if( length > itv)
					{
						/* 距離が離れているものは判定しない */
						continue;
					}
					sub = sub.normalized;
					flg = false;
					for( i2 = 0; i2 < direction.Length; ++i2)
					{
						theta = sub.x * direction[ i2].x + sub.z * direction[ i2].z;
						if( theta <= checkTheta)
						{
							/* 角度の条件を満たしていない */
							continue;
						}
						if( min[ i2] <= length)
						{
							/* すでに設定しているものより遠い */
							continue;
						}
						min[ i2] = length;
						no[ i2] = i1;
						flg = true;
					}
					if( flg != false)
					{
						++count;
					}
					if( inOrder != false)
					{
						if( orderFlag != false)
						{
							i1 = pointList.Count;
						}
					}
				}
				for( i2 = 0; i2 < direction.Length; ++i2)
				{
					if( no[ i2] < 0)
					{
						/* この方向に繋げる座標が無かった */
						continue;
					}
					index = no[ i2];
					currentPoint.SetConnection( pointList[ index]);
					pointList[ index].SetConnection( currentPoint);
				}
				++randomCount;
			}
		}

		/**
		 * 接続ポイントを間引く処理
		 */
		void FieldPointMerge( List<FieldConnectPoint> pointList, float mergeSize)
		{
			int i0, i1;
			float powerSize = mergeSize * mergeSize, length;
			Vector3 sub;
			var indexList = new List<int>();

			for( i0 = 0; i0 < pointList.Count - 1; ++i0)
			{
				indexList.Clear();
				for( i1 = i0 + 1; i1 < pointList.Count; ++i1)
				{
					if( i0 == i1)
					{
						continue;
					}
					sub = pointList[ i0].Position - pointList[ i1].Position;
					length = sub.x * sub.x + sub.z * sub.z;
					if( powerSize > length)
					{
						indexList.Add(i1);
					}
				}
				for( i1 = indexList.Count - 1; i1 >= 0; --i1)
				{
					pointList.RemoveAt( indexList[ i1]);
				}
			}
		}

		/**
		 * 外周から指定された範囲内のポイントを間引く処理
		 */
		void OuterPointThinning( List<FieldConnectPoint> pointList, float ofset)
		{
			int i0;
			Vector3 min, max;
			var indexList = new List<int>();
			float sub;
			float maxSize = 100000f;

			min = new Vector3( float.MaxValue, 0f, float.MaxValue);
			max = new Vector3( float.MinValue, 0f, float.MinValue);
			for( i0 = 0; i0 < pointList.Count; ++i0)
			{
				if( Mathf.Abs( pointList[ i0].Position.x) >= maxSize || Mathf.Abs( pointList[ i0].Position.z) >= maxSize)
				{
					continue;
				}
				if( min.x > pointList[ i0].Position.x)
				{
					min.x = pointList[ i0].Position.x;
				}
				else if( max.x < pointList[ i0].Position.x)
				{
					max.x = pointList[ i0].Position.x;
				}
				if( min.z > pointList[ i0].Position.z)
				{
					min.z = pointList[ i0].Position.z;
				}
				else if( max.z < pointList[ i0].Position.z)
				{
					max.z = pointList[ i0].Position.z;
				}
			}
			sub = max.x - min.x;
			if( sub > ofset * 2f)
			{
				min.x += ofset;
				max.x -= ofset;
			}
			sub = max.z - min.z;
			if( sub > ofset * 2f)
			{
				min.z += ofset;
				max.z -= ofset;
			}
			for( i0 = 0; i0 < pointList.Count; ++i0)
			{
				if( min.x > pointList[ i0].Position.x)
				{
					indexList.Add( i0);
					continue;
				}
				if( min.z > pointList[ i0].Position.z)
				{
					indexList.Add( i0);
					continue;
				}
				if( max.x < pointList[ i0].Position.x)
				{
					indexList.Add( i0);
					continue;
				}
				if( max.z < pointList[ i0].Position.z)
				{
					indexList.Add( i0);
					continue;
				}
			}
			for( i0 = indexList.Count - 1; i0 >= 0; --i0)
			{
				pointList.RemoveAt( indexList[ i0]);
			}
			Debug.Log($"point:{pointList.Count} out:{indexList.Count} min:{min} max:{max}");
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
		/* 川の接続リスト */
		List<FieldConnectPoint> riverConnectPointList;
		/* 全ての道路の接続リスト */
		List<FieldConnectPoint> roadConnectPointList;
		/* すごろくで使用する接続リスト */
		List<FieldConnectPoint> sugorokuConnectPointList;
	}
}
