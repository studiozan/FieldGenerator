using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	[System.Serializable]
	public class RiverPoint
	{
		public Vector3 Point
		{
			get => point;
		}

		public RiverPoint[] ForkedRiverPoints
		{
			get => forkedRiverPoints;
		}

		[SerializeField]
		Vector3 point = default;
		[SerializeField]
		RiverPoint[] forkedRiverPoints = default;
	}
}
