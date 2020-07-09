using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public enum PointType
	{
		//川
		kRiver,
		//川沿いの道路
		kRoadAlongRiver,
		//碁盤目状道路
		kGridRoad,
		//地区の境の道路
		kDistrictRoad,
		//碁盤目状道路と川沿いの道路の交点
		kIntersectionOfGridRoadAndRoadAlongRiver,
		//碁盤目状道路と地区の境の道路の交点
		kIntersectionOfGridRoadAndDistrictRoad,
		//川沿いの道路と地区の境の道路の交点
		kIntersectionOfRoadAlongRiverAndDistrictRoad,
	}
}
