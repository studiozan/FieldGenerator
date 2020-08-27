using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	[System.Serializable]
	public class FieldPointParameter
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
		//川の初期の幅の最小
		[SerializeField]
		public float minInitialRiverWidth = 10;
		//川の初期の幅の最大
		[SerializeField]
		public float maxInitialRiverWidth = 30;
		//川が曲がる角度の範囲
		[SerializeField]
		public float angleRange = 60;
		//川生成時の次の点までの距離
		[SerializeField]
		public float riverStepSize = 10;
		//川の分岐率の初期値の最小
		[SerializeField]
		public float minInitialBranchingProbability = 0;
		//川の分岐率の初期値の最大
		[SerializeField]
		public float maxInitialBranchingProbability = 1;
		//川が分岐しないステップ数の最小
		[SerializeField]
		public int minNumStepWithoutBranching = 10;
		//川が分岐しないステップ数の最大
		[SerializeField]
		public int maxNumStepWithoutBranching = 30;
		//川の曲がりやすさの減衰量
		[SerializeField]
		public float bendabilityAttenuation = 0.01f;
		//道路の幅の候補
		[SerializeField]
		public WeightedValue[] roadWidthCandidates = { new WeightedValue { value = 4, weight = 1 } };
		//川から川沿いの道路までの距離
		[SerializeField]
		public float distanceFromRiver = 2;
		//碁盤目状道路の間隔
		[SerializeField]
		public float roadSpacing = 20;
		//道路に囲まれたエリアを内側に寄せる量
		[SerializeField]
		public float amountInwardMovement = 6;
		//すごろく用の接続座標を間引く範囲の倍率
		[SerializeField]
		public float sugorokuMergeMulti = 1.75f;
		//すごろく用の接続座標を外周から指定された距離分を間引く時の値
		[SerializeField]
		public float sugorokuOffset = 1000f;
	}
}
