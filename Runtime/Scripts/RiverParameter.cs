﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class RiverParameter
	{
		public GameObject Prefab { get; set; }
		public Vector2 FieldSize { get; set; }
		public bool HeadwaterIsOutside { get; set; }
		public float Width { get; set; }
		public float AngleRange { get; set; }
		public float StepSize { get; set; }
		public float BranchingProbability { get; set; }
		public int MinNumStepToBranch { get; set; }
		public float BendabilityAttenuation { get; set; }
	}
}
