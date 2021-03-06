﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	[System.Serializable]
	public class WeightedValue
	{
		[SerializeField]
		public float value = default;
		[SerializeField]
		public float weight = default;
	}
}
