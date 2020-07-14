using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class RoadParameter
	{
		public Vector2Int NumberOfChunk { get; set; }
		public float ChunkSize { get; set; }
		public float Width { get; set; }
		public float DistanceFromRiver { get; set; }
		public float Spacing { get; set; }
	}
}
