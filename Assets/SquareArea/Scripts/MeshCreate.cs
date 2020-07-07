using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquareArea
{
	[RequireComponent (typeof(MeshRenderer))]
	[RequireComponent (typeof(MeshFilter))]
	public class MeshCreate : MonoBehaviour
	{
#if false		
		/* ポリゴンの生成 */
		public void CreatePoly(MapGenerator.QuadPos pos, float hei)
		{
			float hei_size = 100f, tmp_f, uv_1, uv_2, uv_3, uv_4, uv_x, uv_y;
			int i0, i1, cnt, tmp_i, type;
			var mesh = new Mesh();
			var vert = new List<Vector3>();
			var tri = new List<int>();
			var uvs = new List<Vector2>();
			var colors = new List<Color32>();
			type = Random.Range( 0, 3);

			hei = pos.objectHeight;
			type = pos.textureType;
			/* 階層を計算 */
			cnt = (int)(hei / hei_size) + 1;
			tmp_f = hei;
			/* 屋上部分 */
			for( i1 = 0; i1 < 4; i1++)
			{
				vert.Add( new Vector3( pos.posTable[ i1].x, tmp_f, pos.posTable[ i1].y));
			}
			/* 側面部分 */
			for( i0 = 0; i0 < cnt; i0++)
			{
				/*
				for( i2 = 0; i2 < 2; i2++)
				{
					tmp_f = hei - hei_size * (i0 + i2);
					if( tmp_f < 0f)
					{
						tmp_f = 0f;
					}
					for( i1 = 0; i1 < 4; i1++)
					{
						vert.Add( new Vector3( pos.posTable[ i1].x, tmp_f, pos.posTable[ i1].y));
					}
				}
				*/
				for( i1 = 0; i1 < 4; i1++)
				{
					tmp_i = (i1 + 1)%4;
					tmp_f = hei - hei_size * i0;
					vert.Add( new Vector3( pos.posTable[ i1].x, tmp_f, pos.posTable[ i1].y));
					vert.Add( new Vector3( pos.posTable[ tmp_i].x, tmp_f, pos.posTable[ tmp_i].y));
					tmp_f = hei - hei_size * (i0 + 1);
					if( tmp_f < 0f)
					{
						tmp_f = 0f;
					}
					vert.Add( new Vector3( pos.posTable[ tmp_i].x, tmp_f, pos.posTable[ tmp_i].y));
					vert.Add( new Vector3( pos.posTable[ i1].x, tmp_f, pos.posTable[ i1].y));
				}
			}
			/* 頂点の設定 */
			mesh.SetVertices( vert);
		
			tri.Add( 0);	tri.Add( 1);	tri.Add( 2);
			tri.Add( 0);	tri.Add( 2);	tri.Add( 3);
			for( i0 = 0; i0 < cnt; i0++)
			{
				tmp_i = 16 * i0;
				tri.Add( 5 + tmp_i);	tri.Add( 4 + tmp_i);	tri.Add( 6 + tmp_i);
				tri.Add( 4 + tmp_i);	tri.Add( 7 + tmp_i);	tri.Add( 6 + tmp_i);
			
				tri.Add( 5 + 4 + tmp_i);	tri.Add( 4 + 4 + tmp_i);	tri.Add( 6 + 4 + tmp_i);
				tri.Add( 4 + 4 + tmp_i);	tri.Add( 7 + 4 + tmp_i);	tri.Add( 6 + 4 + tmp_i);

				tri.Add( 5 + 8 + tmp_i);	tri.Add( 4 + 8 + tmp_i);	tri.Add( 6 + 8 + tmp_i);
				tri.Add( 4 + 8 + tmp_i);	tri.Add( 7 + 8 + tmp_i);	tri.Add( 6 + 8 + tmp_i);

				tri.Add( 5 + 12 + tmp_i);	tri.Add( 4 + 12 + tmp_i);	tri.Add( 6 + 12 + tmp_i);
				tri.Add( 4 + 12 + tmp_i);	tri.Add( 7 + 12 + tmp_i);	tri.Add( 6 + 12 + tmp_i);

			}
			/* 頂点配列の設定 */
			mesh.SetTriangles( tri, 0);

			/* UVの設定 */
			/* 屋上 */
			tmp_i = Random.Range( 0, 4);
			switch( tmp_i)
			{
			case 0:
				uv_x = 0f;		uv_y = 0f;
				break;
			case 1:
				uv_x = 0.125f;	uv_y = 0f;
				break;
			case 2:
				uv_x = 0f;		uv_y = 0.125f;
				break;
			case 3:
				uv_x = 0.125f;	uv_y = 0.125f;
				break;
			default:
				uv_x = 0f;		uv_y = 0f;
				break;
			}
			switch( type)
			{
			case 0:
				uv_1 = 0f + uv_x;		uv_2 = 0.125f + uv_x;
				uv_3 = 0f + uv_y;		uv_4 = 0.125f + uv_y;
				break;
			case 1:
				uv_1 = 0.75f + uv_x;	uv_2 = 0.875f + uv_x;
				uv_3 = 0f + uv_y;		uv_4 = 0.125f + uv_y;
				break;
			case 2:
				uv_1 = 0.5f + uv_x;		uv_2 = 0.625f + uv_x;
				uv_3 = 0f + uv_y;		uv_4 = 0.125f + uv_y;
				break;
			default:
				uv_1 = 0f;		uv_2 = 0.125f;
				uv_3 = 0f;		uv_4 = 0.125f;
				break;
			}
			uvs.Add( new Vector2( uv_1, uv_4));
			uvs.Add( new Vector2( uv_2, uv_4));
			uvs.Add( new Vector2( uv_2, uv_3));
			uvs.Add( new Vector2( uv_1, uv_3));
			/* 側面 */
			switch( type)
			{
			case 0:
				uv_1 = 0.25f;	uv_2 = 0.5f;
				break;
			case 1:
				uv_1 = 0.5f;	uv_2 = 0.75f;
				break;
			case 2:
				uv_1 = 0.75f;	uv_2 = 1f;
				break;
			default:
				uv_1 = 0.25f;	uv_2 = 0.5f;
				break;
			}
			for( i0 = 0; i0 < cnt; i0++)
			{
				for( i1 = 0; i1 < 4; i1++)
				{
	#if false
					if( i1 != 2)
					{
						tmp_i = Random.Range( 0, 2);
						switch( tmp_i)
						{
						case 0:
							tmp_f = 0.5f;
							break;
						case 1:
							tmp_f = 0.25f;
							break;
						case 2:
							tmp_f = 0f;
							break;
						default:
							tmp_f = 0f;
							break;
						}
					}
					else
					{
						tmp_f = 0f;
					}
	#else
					tmp_f = 0f;
	#endif
					uvs.Add( new Vector2( tmp_f, uv_2));
					uvs.Add( new Vector2( 1f, uv_2));
					uvs.Add( new Vector2( 1f, uv_1));
					uvs.Add( new Vector2( tmp_f, uv_1));
				}
			}
			mesh.SetUVs( 0, uvs);
	#if false		
			/* 頂点カラー */
			colors.Add( new Color32( 255, 0, 0, 128));
			colors.Add( new Color32( 255, 0, 0, 128));
			colors.Add( new Color32( 255, 0, 0, 128));
			colors.Add( new Color32( 255, 0, 0, 128));
			for( i0 = 0; i0 < cnt; i0++)
			{
				for( i1 = 0; i1 < 4; i1++)
				{
					colors.Add( new Color32( 0, 0, 0, 255));
					colors.Add( new Color32( 0, 0, 0, 255));
					colors.Add( new Color32( 0, 0, 0, 255));
					colors.Add( new Color32( 0, 0, 0, 255));
				}
			}
			mesh.SetColors( colors);
	#endif
			mesh.RecalculateNormals();
			var filter = GetComponent<MeshFilter>();
			filter.sharedMesh = mesh;
		}
#endif		

		/* 道路ポリゴンの生成 */
		public float RoadCreatePoly( List<Vector2> pos_list, float width, float plus_width = 0f, float pos_y = 1f)
		{
			int i0, i1, tmp_i;
			Vector2 tmp_vec, tmp_vec2, nor;
			Vector3 tmp_vec3;
			var mesh = new Mesh();
			var vert = new List<Vector3>();
			var tri = new List<int>();
			var uvs = new List<Vector2>();

			/* 頂点座標 */
			for( i0 = 0; i0 < pos_list.Count - 1; i0++)
			{
				tmp_vec2 = pos_list[ i0 + 1] - pos_list[ i0];
				nor = tmp_vec2.normalized;
				tmp_vec2.x = -nor.y;
				tmp_vec2.y = nor.x;
				nor = tmp_vec2 * width * 0.5f;
				for( i1 = 0; i1 < 2; i1++)
				{
					tmp_vec = pos_list[ i0 + i1] + nor;
					tmp_vec3 = new Vector3( tmp_vec.x, pos_y, tmp_vec.y);
					vert.Add( tmp_vec3);
					tmp_vec = pos_list[ i0 + i1] - nor;
					tmp_vec3 = new Vector3( tmp_vec.x, pos_y, tmp_vec.y);
					vert.Add( tmp_vec3);
					if( i1 == 0)
					{
						width += plus_width;
					}
					nor = tmp_vec2 * width * 0.5f;
				}
			}
			mesh.SetVertices( vert);

			/* 頂点情報 */
			for( i0 = 0; i0 < pos_list.Count - 1; i0++)
			{
				tmp_i = 4 * i0;
				if( i0 != 0)
				{
					/* つなぎ目部分 */
					tri.Add( 2 + tmp_i - 4);	tri.Add( 0 + tmp_i);	tri.Add( 1 + tmp_i);
					tri.Add( 2 + tmp_i - 4);	tri.Add( 1 + tmp_i);	tri.Add( 3 + tmp_i - 4);
				}
				tri.Add( 0 + tmp_i);	tri.Add( 2 + tmp_i);	tri.Add( 3 + tmp_i);
				tri.Add( 0 + tmp_i);	tri.Add( 3 + tmp_i);	tri.Add( 1 + tmp_i);
			}
			mesh.SetTriangles( tri, 0);

			/* UV */
			for( i0 = 0; i0 < pos_list.Count - 1; i0++)
			{
				for( i1 = 0; i1 < 2; i1++)
				{
					uvs.Add( new Vector2( 0f + (float)i1, 0f));
					uvs.Add( new Vector2( 0f + (float)i1, 1f));
				}
			}
			mesh.SetUVs( 0, uvs);

			mesh.RecalculateNormals();
			var filter = GetComponent<MeshFilter>();
			filter.sharedMesh = mesh;

			return width;
		}
	}
}