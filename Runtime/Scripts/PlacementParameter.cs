using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	[System.Serializable]
	public class PlacementParameter
	{
		[SerializeField]
		public float placementRate = 1;
		[SerializeField]
		public WeightedObject[] weightedPrefabs = default;
	}
}
