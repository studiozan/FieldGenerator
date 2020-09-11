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
		public float heightScale = 1;
		[SerializeField]
		public float horizontalScale = 1;
		[SerializeField]
		public float weight = default;
	}
}
