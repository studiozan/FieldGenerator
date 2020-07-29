using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class RiverParameter
	{
		public float ChunkSize { get; set; }
		public Vector2Int NumberOfChunk { get; set; }
		public bool HeadwaterIsOutside { get; set; }
		public float MinInitialWidth { get; set; }
		public float MaxInitialWidth { get; set; }
		public float AngleRange { get; set; }
		public float StepSize { get; set; }
		public float MinInitialBranchingProbability { get; set; }
		public float MaxInitialBranchingProbability { get; set; }
		public int MinNumStepWithoutBranching { get; set; }
		public int MaxNumStepWithoutBranching { get; set; }
		public float BendabilityAttenuation { get; set; }
	}
}
