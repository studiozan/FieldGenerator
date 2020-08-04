﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class RiverPoint
	{
		public Vector3 Position { get; set; }
		public float Width { get; set; }
		public RiverPoint PrevPoint { get; set; }
		public List<RiverPoint> NextPoints { get; set; } = new List<RiverPoint>();
	}
}
