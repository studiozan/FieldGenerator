using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	[System.Serializable]
	public class TownParameter
	{
		//シード値
		[SerializeField]
		public int seed = 0;
		//チャンクサイズ
		[SerializeField]
		public float chunkSize = 100;
		//チャンク数
		[SerializeField]
		public Vector2Int numberOfChunk = new Vector2Int(10, 10);
		//川の源流が外側にあるか
		[SerializeField]
		public bool headwaterIsOutside = true;
		//川の幅
		[SerializeField]
		public float riverWidth = 10;
		//川が曲がる角度の範囲
		[SerializeField]
		public float angleRange = 60;
		//川が分岐する確率
		[SerializeField]
		public float branchingProbability = 1.0f;
		//川が分岐するための最低ステップ数
		[SerializeField]
		public int minNumStepToBranch = 10;
		//川の曲がりやすさの減衰量
		[SerializeField]
		public float bendabilityAttenuation = 0.01f;
		//道路の幅
		[SerializeField]
		public float roadWidth = 4;
		//川から川沿いの道路までの距離
		[SerializeField]
		public float distanceFromRiver = 2;
		//碁盤目状道路の間隔
		[SerializeField]
		public float roadSpacing = 20;
	}
}
