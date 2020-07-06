using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class RiverParameter
	{
		public Vector3 Start { get; set; }
		public Vector3 End { get; set; }
		public int NumberPointBetween { get; set; }
		public float Width { get; set; }
		public float AngleRange { get; set; }
	}
}
