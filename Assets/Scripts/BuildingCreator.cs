using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldGenerator;

namespace PolygonGenerator
{
	public class BuildingCreator : MonoBehaviour
	{
		void Awake()
		{
			meshCreator = GetComponent<MeshCreator>();
			townGenerator.OnGenerate += () => CreateBuildingMesh(townGenerator);
		}

		void CreateBuildingMesh(TownGenerator generator)
		{
			List<SurroundedArea> areas = generator.SurroundedAreas;
			var parameters = new List<BuildingParameter>();
			for (int i0 = 0; i0 < areas.Count; ++i0)
			{
				List<Vector3> points = areas[i0].AreaPoints;
				if (points.Count == 3)
				{
					points.Add(points[2]);
				}

				var param = new BuildingParameter(points);
				param.SetBuildingType(BuildingParameter.BuildingType.kBuildingA, 0);
				param.SetBuildingHeight(30);
				parameters.Add(param);
			}

			meshCreator.BuildingPolygonCreate(parameters);
		}

		[SerializeField]
		TownGenerator townGenerator = default;

		MeshCreator meshCreator;
	}
}
