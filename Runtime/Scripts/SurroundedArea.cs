using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class SurroundedArea
	{
		public Vector3 GetCenter()
		{
			Vector3 center = Vector3.zero;
			for (int i0 = 0; i0 < AreaPoints.Count; ++i0)
			{
				center += AreaPoints[i0];
			}
			center /= AreaPoints.Count;

			return center;
		}

		public List<Vector3> AreaPoints { get; set; }
	}
}
