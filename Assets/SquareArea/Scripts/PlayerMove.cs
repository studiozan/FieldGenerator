using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;

public class PlayerMove : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
	}

	void GetList()
	{
		PointList = PointScript.GetSugorokuList();
	}

	// Update is called once per frame
	void Update()
	{
		if( NowPoint == null)
		{
			List<FieldConnectPoint> tmp_list = SugorokuScript.GetPointList();
			if( tmp_list != null)
			{
				NowPoint = tmp_list[ 0];
				gameObject.transform.localPosition = NowPoint.Position;
			}
		}
		if( Input.GetKeyDown(KeyCode.Z))
		{
			List<FieldConnectPoint> tmp_list = SugorokuScript.GetPointList();
			NowPoint = tmp_list[ 0];
			gameObject.transform.localPosition = NowPoint.Position;
		}

		int dir = -1;
		if( Input.GetKeyDown(KeyCode.UpArrow))
		{
			dir = 0;
		}
		if( Input.GetKeyDown(KeyCode.DownArrow))
		{
			dir = 1;
		}
		if( Input.GetKeyDown(KeyCode.RightArrow))
		{
			dir = 2;
		}
		if( Input.GetKeyDown(KeyCode.LeftArrow))
		{
			dir = 3;
		}

		if( dir >= 0)
		{
			/*! 移動入力があったので、移動を行う */
			Move( dir);
		}
	}

	/*
	 * 移動処理
	 */
	void Move( int direction)
	{
		Vector3 dir, sub, tmp_vec = Vector3.zero;
		float tmp_f, min, tmp_f2;
		FieldConnectPoint tmp_point;
		int i0;
		bool flg;

		tmp_point = NowPoint;

		switch( direction)
		{
		case 0:
			dir = Vector3.forward;
			break;
		case 1:
			dir = Vector3.back;
			break;
		case 2:
			dir = Vector3.right;
			break;
		case 3:
			dir = Vector3.left;
			break;
		default:
			dir = Vector3.forward;
			break;
		}

		min = 0.65f;
		flg = false;
		for( i0 = 0; i0 < tmp_point.ConnectionList.Count; i0++)
		{
			sub = tmp_point.ConnectionList[ i0].Position - tmp_point.Position;
			tmp_f = sub.x * dir.x + sub.z * dir.z;
			//tmp_f2 = Mathf.Abs( sub.x) + Mathf.Abs( sub.z);
			tmp_f2 = sub.magnitude;
			if( tmp_f2 > 0f)
			{
				tmp_f = tmp_f / tmp_f2;
			}
			//Debug.Log($"[{i0}] pos:{tmp_point.ConnectionList[ i0].Position} tmp_pos:{tmp_point.Position} sub:{sub} tmp_f:{tmp_f} min:{min}");
			if( tmp_f > min)
			{
				min = tmp_f;
				tmp_vec = tmp_point.ConnectionList[ i0].Position;
				NowPoint = tmp_point.ConnectionList[ i0];
				flg = true;
			}
		}

		if( flg != false)
		{
			transform.localPosition = NowPoint.Position;
			//Debug.Log($"index:{NowPoint.Index}");
		}
	}

	List<FieldConnectPoint> PointList;
	FieldConnectPoint NowPoint;

	[SerializeField]
	PointConnection2 PointScript = default;

	[SerializeField]
	SugorokuMap.SugorokuMapCreater SugorokuScript = default;
}
