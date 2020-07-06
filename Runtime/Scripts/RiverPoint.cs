using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class RiverPoint
	{
		public Vector3 Point { get; set; }
		public float Width { get; set; }
		public List<RiverPoint> NextPoints { get; set; } = new List<RiverPoint>();
	}
}
