using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  FieldGenerator
{
	public class CoroutineUtility
	{
		public static IEnumerator CoroutineCycle( IEnumerator routine)
		{
#if UNITY_EDITOR
			if( Application.isPlaying == false)
			{
				while( routine.MoveNext() != false){}
			}
#endif
			return routine;
		}
	}
}
