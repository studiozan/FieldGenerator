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
		public float weight = default;
	}
}
