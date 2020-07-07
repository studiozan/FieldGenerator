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
			ConnectionList = new List<Vector3>();
		}

		/**
		 * リストの設定
		 */
		public void SetConnection( List<Vector3> list)
		{
			int i0, i1, num;
			float tmp_f;
			bool flg;
			Vector3 sub;

			num = ConnectionList.Count;
			for( i0 = 0; i0 < list.Count; i0++)
			{
				flg = true;
				for( i1 = 0; i1 < num; i1++)
				{
					sub = list[ i0] - ConnectionList[ i1];
					tmp_f = sub.x * sub.x + sub.z * sub.z;
					if( tmp_f < 0.1f)
					{
						flg = false;
						break;
					}
				}
				if( flg != false)
				{
					ConnectionList.Add( list[ i0]);
				}
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

		//! 繋がっている座標のリスト
		public List<Vector3> ConnectionList
		{
			get;
			private set;
		}
	}
}
