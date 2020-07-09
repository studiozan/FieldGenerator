/**
 * @file FieldConnectPoint.cs
 * @brief 接続関係を含めた座標と属性のリスト
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class FieldConnectPoint : FieldPoint
	{
		/**
		 * 初期化処理
		 */
		public void Initialize( Vector3 pos, PointType type)
		{
			Position = pos;
			Type = type;
			ConnectionList = new List<FieldConnectPoint>();
		}

		/**
		 * リストの設定
		 * 設定したいポイントがすでにリストにあるかどうか調べて、ない場合は追加する
		 * @param point		リストに追加したいポイント
		 */
		 public void SetConnection( FieldConnectPoint point)
		 {
			int i0;
			float tmp_f;
			Vector3 sub;
			bool flg;

			flg = true;
			for( i0 = 0; i0 < ConnectionList.Count; i0++)
			{
				sub = point.Position - ConnectionList[ i0].Position;
				tmp_f = sub.x * sub.x + sub.z * sub.z;
				if( tmp_f >= 0.1f)
				{
					continue;
				}
				flg = false;
				break;
			}
			if( flg != false)
			{
				ConnectionList.Add( point);
			}
		 }

		/**
		 * リストの切断
		 * @param index		切断したい要素のインデックス。-1の場合は全て切断する
		 */
		public void Disconnection( int index = -1)
		{
			if( index >= 0)
			{
				ConnectionList.RemoveAt( index);
			}
			else
			{
				ConnectionList.Clear();
			}
		}

		//! 繋がっているポイントのリスト
		public List<FieldConnectPoint> ConnectionList
		{
			get;
			private set;
		}

		//! リストのインデックス
		public int Index
		{
			get;
			set;
		}
	}
}
