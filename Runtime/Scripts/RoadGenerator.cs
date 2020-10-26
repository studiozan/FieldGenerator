using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldGenerator
{
	public class RoadGenerator
	{
		public void Generate(RoadParameter parameter, RiverGenerator river, System.Random random)
		{
			lastInterruptionTime = System.DateTime.Now;
			this.parameter = parameter;
			this.random = random;
			this.river = river;

			thinningDistance = (parameter.Spacing + parameter.Width) * 0.5f;

			points.Clear();
			pointMap.Clear();

			GenerateDistrictRoad();
			GenerateRoadAlongRiver(river.RootPoint);
			GenerateGridRoad();
		}

		void GenerateDistrictRoad()
		{
			districtRoadPoints.Clear();
			float chunkSize = parameter.ChunkSize;
			Vector2Int numberOfChunk = parameter.NumberOfChunk;

			for (int row = 0; row <= numberOfChunk.y; ++row)
			{
				for (int column = 0; column <= numberOfChunk.x; ++column)
				{
					float x = chunkSize * column;
					float z = chunkSize * row;
					var pos = new Vector3(x, 0, z);
					if (river.Covers(pos) == false)
					{
						var point = new FieldPoint
						{
							Position = pos,
							Type = PointType.kDistrictRoad,
						};
						districtRoadPoints.Add(point);
						AddToPointMap(point);
					}
				}
			}

			points.AddRange(districtRoadPoints);
		}

		void GenerateRoadAlongRiver(RiverPoint riverRoot)
		{
			roadAlongRiverPoints.Clear();
			leftRoadPoints.Clear();
			rightRoadPoints.Clear();
			for (int i0 = 0; i0 < riverRoot.NextPoints.Count; ++i0)
			{
				GenerateRoadAlongRiverRecursive(riverRoot, null, riverRoot.NextPoints[i0], false, false);
			}

			roadAlongRiverPoints.AddRange(leftRoadPoints);
			roadAlongRiverPoints.AddRange(rightRoadPoints);

			points.AddRange(roadAlongRiverPoints);
		}

		void GenerateRoadAlongRiverRecursive(RiverPoint currentPoint, RiverPoint prevPoint, RiverPoint nextPoint, bool addedLeftPoint, bool addedRightPoint)
		{
			Vector3 pos = currentPoint.Position;
			Vector3 nextPos = nextPoint.Position;

			Vector3 dir = nextPos - pos;
			Vector3 prevDir = prevPoint != null ? pos - prevPoint.Position : dir;

			float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
			Quaternion rotation = Quaternion.Euler(0, angle, 0);

			float dist = currentPoint.Width * 0.5f + parameter.DistanceFromRiver + parameter.Width * 0.5f;
			var leftBase = new Vector3(-dist, 0, 0);
			var rightBase = new Vector3(dist, 0, 0);

			float cross = Vector3.Cross(prevDir, dir).y;

			bool addedL = false;
			bool addedR = false;

			if (addedLeftPoint == false || cross > 0)
			{
				Vector3 left = rotation * leftBase + pos;
				if (river.Covers(left) == false)
				{
					var leftPoint = new FieldPoint
					{
						Position = left,
						Type = PointType.kRoadAlongRiver,
					};
					leftRoadPoints.Add(leftPoint);
					AddToPointMap(leftPoint);
					addedL = true;
				}
			}

			if (addedRightPoint == false || cross < 0)
			{
				Vector3 right = rotation * rightBase + pos;
				if (river.Covers(right) == false)
				{
					var rightPoint = new FieldPoint
					{
						Position = right,
						Type = PointType.kRoadAlongRiver,
					};
					rightRoadPoints.Add(rightPoint);
					AddToPointMap(rightPoint);
					addedR = true;
				}
			}

			if (nextPoint.NextPoints.Count > 0)
			{
				for (int i0 = 0; i0 < nextPoint.NextPoints.Count; ++i0)
				{
					GenerateRoadAlongRiverRecursive(nextPoint, currentPoint, nextPoint.NextPoints[i0], addedL, addedR);
				}
			}
			else
			{
				Vector3 nextLeft = rotation * leftBase + nextPos;
				if (river.Covers(nextLeft) == false)
				{
					var leftPoint2 = new FieldPoint
					{
						Position = nextLeft,
						Type = PointType.kRoadAlongRiver,
					};
					leftRoadPoints.Add(leftPoint2);
					AddToPointMap(leftPoint2);
				}

				Vector3 nextRight = rotation * rightBase + nextPos;
				if (river.Covers(nextRight) == false)
				{
					var rightPoint2 = new FieldPoint
					{
						Position = nextRight,
						Type = PointType.kRoadAlongRiver,
					};
					rightRoadPoints.Add(rightPoint2);
					AddToPointMap(rightPoint2);
				}
			}
		}

		void GenerateGridRoad()
		{
			gridRoadPoints.Clear();
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
					Vector3 pos1, pos2;
					if (tensPlace == 0)
					{
						pos1 = new Vector3(center.x, 0, chunkRect.y);
						pos2 = new Vector3(center.x, 0, chunkRect.y + chunkRect.height);
					}
					else
					{
						Vector3 normDir = rotation * Vector3.forward;

						Vector3 start1;
						Vector3 end1;
						Vector3 start2;
						Vector3 end2;

						if (tensPlace <= 4)
						{
							start1 = new Vector3(chunkRect.xMin, 0, chunkRect.yMin);
							end1 = new Vector3(chunkRect.xMax, 0, chunkRect.yMin);

							start2 = new Vector3(chunkRect.xMin, 0, chunkRect.yMax);
							end2 = new Vector3(chunkRect.xMax, 0, chunkRect.yMax);
						}
						else
						{
							start1 = new Vector3(chunkRect.xMin, 0, chunkRect.yMin);
							end1 = new Vector3(chunkRect.xMin, 0, chunkRect.yMax);

							start2 = new Vector3(chunkRect.xMax, 0, chunkRect.yMin);
							end2 = new Vector3(chunkRect.xMax, 0, chunkRect.yMax);
						}

						TryGetIntersection(center, center + normDir, start1, end1, out pos1);
						TryGetIntersection(center, center + normDir, start2, end2, out pos2);
					}

					if (river.Covers(pos1) == false)
					{
						var point1 = new FieldPoint
						{
							Position = pos1,
							Type = PointType.kIntersectionOfGridRoadAndDistrictRoad,
						};
						if (AddIfAway(point1, thinningDistance) != false)
						{
							gridRoadPoints.Add(point1);
						}
					}
					if (river.Covers(pos2) == false)
					{
						var point2 = new FieldPoint
						{
							Position = pos2,
							Type = PointType.kIntersectionOfGridRoadAndDistrictRoad,
						};
						if (AddIfAway(point2, thinningDistance) != false)
						{
							gridRoadPoints.Add(point2);
						}
					}

					//base---------------------------------------------------------------
					Vector3 dir = pos2 - pos1;
					float maxLength = (Quaternion.Euler(0, -angle, 0) * dir).z;
					float width = parameter.Width;
					float spacing = parameter.Spacing;
					float totalLength = 0;
					var basePoints = new List<Vector3>();
					while (totalLength + width + spacing < maxLength)
					{
						var pos = new Vector3(0, 0, totalLength + width / 2);
						basePoints.Add(pos);
						totalLength += width + spacing;
					}
					if (totalLength + width < maxLength)
					{
						var pos = new Vector3(0, 0, totalLength + width / 2);
						basePoints.Add(pos);
						totalLength += width;
					}
					else
					{
						totalLength -= spacing;
					}

					float offsetZ = (maxLength - totalLength) / 2;
					for (int i0 = 0; i0 < basePoints.Count; ++i0)
					{
						Vector3 pos = basePoints[i0];
						pos.z += offsetZ;
						pos = rotation * pos + pos1;
						basePoints[i0] = pos;
						if (river.Covers(pos) == false)
						{
							var point = new FieldPoint
							{
								Position = pos,
								Type = PointType.kGridRoad,
							};
							if (AddIfAway(point, thinningDistance) != false)
							{
								gridRoadPoints.Add(point);
							}
						}
					}
					//-------------------------------------------------------------------

					Quaternion leftRot = Quaternion.Euler(0, angle - 90, 0);
					Quaternion rightRot = Quaternion.Euler(0, angle + 90, 0);
					for (int i0 = 0; i0 < basePoints.Count; ++i0)
					{
						Vector3 basePoint = basePoints[i0];
						var roadStep = new Vector3(0, 0, spacing + width);

						//left------------------------
						var leftPoints = new List<FieldPoint>();
						Vector3 leftPos = leftRot * roadStep + basePoint;
						while (IsInsideRect(chunkRect, leftPos) != false)
						{
							if (river.Covers(leftPos) == false)
							{
								var leftPoint = new FieldPoint
								{
									Position = leftPos,
									Type = PointType.kGridRoad,
								};
								if (AddIfAway(leftPoint, thinningDistance) != false)
								{
									leftPoints.Add(leftPoint);
								}
							}
							leftPos = leftRot * roadStep + leftPos;
						}
						Vector3 lastLeftPos;
						if (leftPoints.Count > 0)
						{
							lastLeftPos = leftPoints[leftPoints.Count - 1].Position;
						}
						else
						{
							lastLeftPos = leftPos;
						}
						float zl = lastLeftPos.z - basePoint.z;
						if (Mathf.Approximately(zl, 0) == false)
						{
							float al = zl / (lastLeftPos.x - basePoint.x);
							float bl = lastLeftPos.z - al * lastLeftPos.x;
							var leftIntersection = new Vector3();
							leftIntersection.x = chunkRect.x;
							leftIntersection.z = al * leftIntersection.x + bl;
							var upIntersection = new Vector3();
							upIntersection.z = chunkRect.y + chunkRect.height;
							upIntersection.x = (upIntersection.z - bl) / al;
							Vector3 intersectionL = leftIntersection.x >= upIntersection.x ? leftIntersection : upIntersection;
							if (river.Covers(intersectionL) == false)
							{
								var leftPoint = new FieldPoint
								{
									Position = intersectionL,
									Type = PointType.kIntersectionOfGridRoadAndDistrictRoad,
								};
								if (AddIfAway(leftPoint, thinningDistance) != false)
								{
									leftPoints.Add(leftPoint);
								}
							}
						}
						else
						{
							var pos = new Vector3(0, 0, basePoint.z);
							if (river.Covers(pos) == false)
							{
								var leftPoint = new FieldPoint
								{
									Position = pos,
									Type = PointType.kIntersectionOfGridRoadAndDistrictRoad,
								};
								if (AddIfAway(leftPoint, thinningDistance) != false)
								{
									leftPoints.Add(leftPoint);
								}
							}
						}
						gridRoadPoints.AddRange(leftPoints);
						//----------------------------

						//right-----------------------
						var rightPoints = new List<FieldPoint>();
						Vector3 rightPos = rightRot * roadStep + basePoint;
						while (IsInsideRect(chunkRect, rightPos) != false)
						{
							if (river.Covers(rightPos) == false)
							{
								var rightPoint = new FieldPoint
								{
									Position = rightPos,
									Type = PointType.kGridRoad
								};
								if (AddIfAway(rightPoint, thinningDistance) != false)
								{
									rightPoints.Add(rightPoint);
								}
							}
							rightPos = rightRot * roadStep + rightPos;
						}
						Vector3 lastRightPos;
						if (rightPoints.Count > 0)
						{
							lastRightPos = rightPoints[rightPoints.Count - 1].Position;
						}
						else
						{
							lastRightPos = rightPos;
						}
						float zr = lastRightPos.z - basePoint.z;
						if (Mathf.Approximately(zr, 0) == false)
						{
							float ar = zr / (lastRightPos.x - basePoint.x);
							float br = lastRightPos.z - ar * lastRightPos.x;
							var rightIntersection = new Vector3();
							rightIntersection.x = chunkRect.x + chunkRect.width;
							rightIntersection.z = ar * rightIntersection.x + br;
							var downIntersection = new Vector3();
							downIntersection.z = chunkRect.y;
							downIntersection.x = (downIntersection.z - br) / ar;
							Vector3 intersectionR = rightIntersection.x <= downIntersection.x ? rightIntersection : downIntersection;
							if (river.Covers(intersectionR) == false)
							{
								var rightPoint = new FieldPoint
								{
									Position = intersectionR,
									Type = PointType.kIntersectionOfGridRoadAndDistrictRoad
								};
								if (AddIfAway(rightPoint, thinningDistance) != false)
								{
									rightPoints.Add(rightPoint);
								}
							}
						}
						else
						{
							var pos = new Vector3(chunkSize, 0, basePoint.z);
							if (river.Covers(pos) == false)
							{
								var rightPoint = new FieldPoint
								{
									Position = pos,
									Type = PointType.kIntersectionOfGridRoadAndDistrictRoad,
								};
								if (AddIfAway(rightPoint, thinningDistance) != false)
								{
									rightPoints.Add(rightPoint);
								}
							}
						}
						gridRoadPoints.AddRange(rightPoints);
						//----------------------------
					}
				}
			}
		}

		bool TryGetIntersection(Vector3 startPos1, Vector3 endPos1, Vector3 startPos2, Vector3 endPos2, out Vector3 intersection)
		{
			bool isIntersecting = false;
			intersection = Vector3.zero;

			Vector3 dir1 = endPos1 - startPos1;
			Vector3 dir2 = startPos2 - startPos1;
			Vector3 dir3 = startPos1 - endPos2;
			Vector3 dir4 = endPos2 - startPos2;

			float area1 = Vector3.Cross(dir1, dir2).y * 0.5f;
			float area2 = Vector3.Cross(dir1, dir3).y * 0.5f;
			float area = area1 + area2;

			if (Mathf.Approximately(area, 0) == false)
			{
				isIntersecting = true;
				intersection = startPos2 + dir4 * area1 / area;
			}

			return isIntersecting;
		}

		bool IsInsideRect(Rect rect, Vector3 pos)
		{
			return pos.x > rect.x && pos.x < rect.x + rect.width && pos.z > rect.y && pos.z < rect.y + rect.height;
		}

		bool AddIfAway(FieldPoint point, float distance)
		{
			bool canAdd = true;

			float chunkSize = parameter.ChunkSize;
			Vector3 pos = point.Position;

			int minX = Mathf.Max(Mathf.FloorToInt((pos.x - distance) / chunkSize), 0);
			int maxX = Mathf.Min(Mathf.FloorToInt((pos.x + distance) / chunkSize), parameter.NumberOfChunk.x);
			int minY = Mathf.Max(Mathf.FloorToInt((pos.z - distance) / chunkSize), 0);
			int maxY = Mathf.Min(Mathf.FloorToInt((pos.z + distance) / chunkSize), parameter.NumberOfChunk.y);

			for (int y = minY; y <= maxY; ++y)
			{
				for (int x = minX; x <= maxX; ++x)
				{
					var chunk = new Vector2Int(x, y);
					if (pointMap.TryGetValue(chunk, out List<FieldPoint> pointsInChunk) != false)
					{
						for (int i0 = 0; i0 < pointsInChunk.Count; ++i0)
						{
							Vector3 dist = pointsInChunk[i0].Position - pos;
							if (dist.sqrMagnitude < distance * distance)
							{
								canAdd = false;
								break;
							}
						}

						if (canAdd == false)
						{
							break;
						}
					}
				}

				if (canAdd == false)
				{
					break;
				}
			}

			if (canAdd != false)
			{
				points.Add(point);
				AddToPointMap(point);
			}

			return canAdd;
		}

		void AddToPointMap(FieldPoint point)
		{
			Vector2Int chunk = GetChunk(point.Position);
			List<FieldPoint> pointsInChunk;
			if (pointMap.TryGetValue(chunk, out pointsInChunk) == false)
			{
				pointsInChunk = new List<FieldPoint>();
				pointMap.Add(chunk, pointsInChunk);
			}
			pointsInChunk.Add(point);
		}

		Vector2Int GetChunk(Vector3 position)
		{
			var chunk = new Vector2Int();

			float chunkSize = parameter.ChunkSize;
			chunk.x = Mathf.FloorToInt(position.x / chunkSize);
			chunk.y = Mathf.FloorToInt(position.z / chunkSize);

			return chunk;
		}

		public List<FieldPoint> Points
		{
			get => points;
		}

		public List<FieldPoint> DistrictRoadPoints
		{
			get => districtRoadPoints;
		}

		public List<FieldPoint> RoadAlongRiverPoints
		{
			get => roadAlongRiverPoints;
		}

		public List<FieldPoint> GridRoadPoints
		{
			get => gridRoadPoints;
		}

		System.Random random;

		System.DateTime lastInterruptionTime;

		List<FieldPoint> points = new List<FieldPoint>();
		Dictionary<Vector2Int, List<FieldPoint>> pointMap = new Dictionary<Vector2Int, List<FieldPoint>>();
		RoadParameter parameter;
		RiverGenerator river;

		List<FieldPoint> districtRoadPoints = new List<FieldPoint>();
		List<FieldPoint> roadAlongRiverPoints = new List<FieldPoint>();
		List<FieldPoint> gridRoadPoints = new List<FieldPoint>();

		List<FieldPoint> leftRoadPoints = new List<FieldPoint>();
		List<FieldPoint> rightRoadPoints = new List<FieldPoint>();

		float thinningDistance;
	}
}
