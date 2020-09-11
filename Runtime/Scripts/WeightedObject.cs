using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	[System.Serializable]
	public class WeightedObject
	{
		[SerializeField]
		public GameObject gameObject = default;
		[SerializeField]
		public bool rotatable = true;
		[SerializeField]
		public float minHeightScale = 1;
		[SerializeField]
		public float maxHeightScale = 1;
		[SerializeField]
		public float minHorizontalScale = 1;
		[SerializeField]
		public float maxHorizontalScale = 1;
		[SerializeField]
		public float weight = default;
	}
}
