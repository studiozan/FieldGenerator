using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class Road
	{
		public void Generate(RoadParameter parameter, RiverPoint riverRoot, System.Random random)
		{
			this.parameter = parameter;
			this.random = random;

			points.Clear();

			GenerateDistrictRoad();
			GenerateRoadAlongRiver(riverRoot);
			GenerateGridRoad();
		}

		void GenerateDistrictRoad()
		{
			float chunkSize = parameter.ChunkSize;
			Vector2Int numberOfChunk = parameter.NumberOfChunk;

			for (int row = 0; row <= numberOfChunk.y; ++row)
			{
				for (int column = 0; column <= numberOfChunk.x; ++column)
				{
					float x = chunkSize * column;
					float z = chunkSize * row;
					var point = new FieldPoint
					{
						Position = new Vector3(x, 0, z),
						Type = PointType.kDistrictRoad,
					};
					points.Add(point);
				}
			}
		}

		void GenerateRoadAlongRiver(RiverPoint riverRoot)
		{
			for (int i0 = 0; i0 < riverRoot.NextPoints.Count; ++i0)
			{
				GenerateRoadAlongRiverRecursive(riverRoot, riverRoot.NextPoints[i0]);
			}
		}

		void GenerateRoadAlongRiverRecursive(RiverPoint currentPoint, RiverPoint nextPoint)
		{
			Vector3 dir = nextPoint.Position - currentPoint.Position;
			float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
			Quaternion rotation = Quaternion.Euler(0, angle, 0);
			float dist = currentPoint.Width / 2 + parameter.DistanceFromRiver + parameter.Width / 2;
			var leftBase = new Vector3(-dist, 0, 0);
			var rightBase = new Vector3(dist, 0, 0);
			Vector3 left = rotation * leftBase + currentPoint.Position;
			Vector3 right = rotation * rightBase + currentPoint.Position;
			var leftPoint = new FieldPoint
			{
				Position = left,
				Type = PointType.kRoadAlongRiver,
			};
			var rightPoint = new FieldPoint
			{
				Position = right,
				Type = PointType.kRoadAlongRiver,
			};
			points.Add(leftPoint);
			points.Add(rightPoint);

			if (nextPoint.NextPoints.Count > 0)
			{
				for (int i0 = 0; i0 < nextPoint.NextPoints.Count; ++i0)
				{
					GenerateRoadAlongRiverRecursive(nextPoint, nextPoint.NextPoints[i0]);
				}
			}
			else
			{
				var leftPoint2 = new FieldPoint
				{
					Position = rotation * leftBase + nextPoint.Position,
					Type = PointType.kRoadAlongRiver,
				};
				var rightPoint2 = new FieldPoint
				{
					Position = rotation * rightBase + nextPoint.Position,
					Type = PointType.kRoadAlongRiver, 
				};
				points.Add(leftPoint2);
				points.Add(rightPoint2);
			}
		}

		void GenerateGridRoad()
		{
			for (int row = 0; row < parameter.NumberOfChunk.y; ++row)
			{
				for (int column = 0; column < parameter.NumberOfChunk.x; ++column)
				{
					float chunkSize = parameter.ChunkSize;
					var chunkRect = new Rect(column * chunkSize, row * chunkSize, chunkSize, chunkSize);
					var center = new Vector3(chunkRect.x + chunkRect.width / 2, 0, chunkRect.y + chunkRect.height / 2);
					int tensPlace = random.Next(9);
					float angle = (float)(tensPlace * 10);
					Quaternion rotation = Quaternion.Euler(0, angle, 0);
					Vector3 p1, p2;
					if (tensPlace == 0)
					{
						p1 = new Vector3(center.x, 0, chunkRect.y);
						p2 = new Vector3(center.x, 0, chunkRect.y + chunkRect.height);
					}
					else
					{
						Vector3 normDir = rotation * Vector3.forward;
						float a = normDir.z / normDir.x;
						float b = center.z - a * center.x;
						float y1 = chunkRect.y;
						float x1 = (y1 - b) / a;
						if (x1 < chunkRect.x)
						{
							x1 = chunkRect.x;
							y1 = a * x1 + b;
						}
						p1 = new Vector3(x1, 0, y1);

						float y2 = chunkRect.y + chunkRect.height;
						float x2 = (y2 - b) / a;
						if (x2 > chunkRect.x + chunkRect.width)
						{
							x2 = chunkRect.x + chunkRect.width;
							y2 = a * x2 + b;
						}
						p2 = new Vector3(x2, 0, y2);
					}

					//------------------------------------------------------------------
					Vector3 dir = p2 - p1;
					float maxLength = (Quaternion.Euler(0, -angle, 0) * dir).z;
					float width = parameter.Width;
					float spacing = parameter.Spacing;
					float totalLength = 0;
					var basePoints = new List<FieldPoint>();
					while (totalLength + width + spacing < maxLength)
					{
						var point = new FieldPoint
						{
							Position = new Vector3(0, 0, totalLength + width / 2),
							Type = PointType.kGridRoad,
						};
						basePoints.Add(point);
						totalLength += width + spacing;
					}
					if (totalLength + width < maxLength)
					{
						var point = new FieldPoint
						{
							Position = new Vector3(0, 0, totalLength + width / 2),
							Type = PointType.kGridRoad,
						};
						basePoints.Add(point);
						totalLength += width;
					}
					else
					{
						totalLength -= spacing;
					}

					float offsetZ = (maxLength - totalLength) / 2;
					for (int i0 = 0; i0 < basePoints.Count; ++i0)
					{
						Vector3 pos = basePoints[i0].Position;
						pos.z += offsetZ;
						basePoints[i0].Position = rotation * pos + p1;
					}

					var point1 = new FieldPoint
					{
						Position = p1,
						Type = PointType.kIntersectionOfGridRoadAndDistrictRoad,
					};
					var point2 = new FieldPoint
					{
						Position = p2,
						Type = PointType.kIntersectionOfGridRoadAndDistrictRoad,
					};
					points.Add(point1);
					points.AddRange(basePoints);
					points.Add(point2);
					//------------------------------------------------------------------

					Quaternion leftRot = Quaternion.Euler(0, angle - 90, 0);
					Quaternion rightRot = Quaternion.Euler(0, angle + 90, 0);
					for (int i0 = 0; i0 < basePoints.Count; ++i0)
					{
						Vector3 basePoint = basePoints[i0].Position;
						var roadStep = new Vector3(0, 0, spacing + width);

						var leftPoints = new List<FieldPoint>();
						Vector3 leftPoint = leftRot * roadStep + basePoint;
						while (IsInsideRect(chunkRect, leftPoint) != false)
						{
							var lp = new FieldPoint
							{
								Position = leftPoint,
								Type = PointType.kGridRoad,
							};
							leftPoints.Add(lp);
							leftPoint = leftRot * roadStep + leftPoint;
						}
						Vector3 lastLeftPoint;
						if (leftPoints.Count > 0)
						{
							lastLeftPoint = leftPoints[leftPoints.Count - 1].Position;
						}
						else
						{
							lastLeftPoint = leftPoint;
						}
						float zl = lastLeftPoint.z - basePoint.z;
						if (Mathf.Approximately(zl, 0) == false)
						{
							float al = zl / (lastLeftPoint.x - basePoint.x);
							float bl = lastLeftPoint.z - al * lastLeftPoint.x;
							var leftIntersection = new Vector3();
							leftIntersection.x = chunkRect.x;
							leftIntersection.z = al * leftIntersection.x + bl;
							var upIntersection = new Vector3();
							upIntersection.z = chunkRect.y + chunkRect.height;
							upIntersection.x = (upIntersection.z - bl) / al;
							Vector3 intersectionL = leftIntersection.x >= upIntersection.x ? leftIntersection : upIntersection;
							var lp = new FieldPoint
							{
								Position = intersectionL,
								Type = PointType.kIntersectionOfGridRoadAndDistrictRoad,
							};
							leftPoints.Add(lp);
						}
						else
						{
							var lp = new FieldPoint
							{
								Position = new Vector3(0, 0, basePoint.z),
								Type = PointType.kIntersectionOfGridRoadAndDistrictRoad,
							};
							leftPoints.Add(lp);
						}
						points.AddRange(leftPoints);

						var rightPoints = new List<FieldPoint>();
						Vector3 rightPoint = rightRot * roadStep + basePoint;
						while (IsInsideRect(chunkRect, rightPoint) != false)
						{
							var rp = new FieldPoint
							{
								Position = rightPoint,
								Type = PointType.kGridRoad
							};
							rightPoints.Add(rp);
							rightPoint = rightRot * roadStep + rightPoint;
						}
						Vector3 lastRightPoint;
						if (rightPoints.Count > 0)
						{
							lastRightPoint = rightPoints[rightPoints.Count - 1].Position;
						}
						else
						{
							lastRightPoint = rightPoint;
						}
						float zr = lastRightPoint.z - basePoint.z;
						if (Mathf.Approximately(zr, 0) == false)
						{
							float ar = zr / (lastRightPoint.x - basePoint.x);
							float br = lastRightPoint.z - ar * lastRightPoint.x;
							var rightIntersection = new Vector3();
							rightIntersection.x = chunkRect.x + chunkRect.width;
							rightIntersection.z = ar * rightIntersection.x + br;
							var downIntersection = new Vector3();
							downIntersection.z = chunkRect.y;
							downIntersection.x = (downIntersection.z - br) / ar;
							Vector3 intersectionR = rightIntersection.x <= downIntersection.x ? rightIntersection : downIntersection;
							var rp = new FieldPoint
							{
								Position = intersectionR,
								Type = PointType.kIntersectionOfGridRoadAndDistrictRoad
							};
							rightPoints.Add(rp);
						}
						else
						{
							var rp = new FieldPoint
							{
								Position = new Vector3(chunkSize, 0, basePoint.z),
								Type = PointType.kIntersectionOfGridRoadAndDistrictRoad,
							};
							rightPoints.Add(rp);
						}
						points.AddRange(rightPoints);
					}
				}
			}
		}

		bool IsInsideRect(Rect rect, Vector3 pos)
		{
			return pos.x > rect.x && pos.x < rect.x + rect.width && pos.z > rect.y && pos.z < rect.y + rect.height;
		}

		public List<FieldPoint> Points
		{
			get => points;
		}

		System.Random random;

		List<FieldPoint> points = new List<FieldPoint>();
		RoadParameter parameter;
	}
}
