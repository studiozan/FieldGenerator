using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class Main : MonoBehaviour
	{
		void Awake()
		{
			generator = GetComponent<TownGenerator>();
			generator.Initialize();
		}

		void Start()
		{
			StartCoroutine(generator.GenerateTown());
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space) != false)
			{
				StartCoroutine(generator.GenerateTown());
			}
		}

		TownGenerator generator;
	}
}
