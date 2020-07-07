using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SquareArea;
using SquareArea2;

public class CheckScr : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
		areaScr.init();
		areaScr.Create();
	}

	// Update is called once per frame
	void Update()
	{
		if( Input.GetKeyDown( KeyCode.Space))
		{
			areaScr.Create();
		}
	}

	[SerializeField]
	SquareAreaGenerator areaScr = default;
}
