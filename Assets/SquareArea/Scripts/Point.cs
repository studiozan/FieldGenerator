/**
 * @file Point.cs
 * @brief 座標の属性と自分と繋がっている座標リストをまとめたクラス
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class Point
	{
		/**
		 * 初期化処理
		 */
		public void Initialize(Vector3 pos, int attr)
		{
			Position = pos;
			Attribute = attr;
			ConnectionPointList = new List<Point>();
		}

		/**
		 * リストの設定
		 * 設定したいポイントがすでにリストにあるかどうか調べて、ない場合は追加する
		 * @param point		リストに追加したいポイント
		 */
		public void SetConnection( Point point)
		{
			int i0;
			float tmp_f;
			Vector3 sub;
			bool flg;

			flg = true;
			for( i0 = 0; i0 < ConnectionPointList.Count; i0++)
			{
				sub = point.Position - ConnectionPointList[ i0].Position;
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
				ConnectionPointList.Add( point);
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
				ConnectionPointList.RemoveAt( index);
			}
			else
			{
				ConnectionPointList.Clear();
			}
		}

		//! 座標
		public Vector3 Position
		{
			get;
			private set;
		}

		//! 座標の属性
		public int Attribute
		{
			get;
			private set;
		}

		//! 繋がっているポイントのリスト
		public List<Point> ConnectionPointList
		{
			get;
			private set;
		}

		public int Index
		{
			get;
			set;
		}
	}
}
