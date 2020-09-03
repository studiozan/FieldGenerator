using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	[System.Serializable]
	public class WeightedRange
	{
		[SerializeField]
		public float min = default;
		[SerializeField]
		public float max = default;
		[SerializeField]
		public float weight = default;
	}
}
