using UnityEngine;
using UnityEditor;

/// <summary>
/// Represents a preprocessed path for nav: a set of waypoints,
/// the turn boundaries (2D lines in XZ) for early turning,
/// and indices for finishing and slowing down;
/// </summary>
public class Navi_Path 
	{

		public readonly Vector3[] lookPoints;
		public readonly Line[] turnBoundaries;
		public readonly int finishLineIndex;
		public readonly int slowDownIndex;

		Vector3 size = Vector3.one;

		/// <summary>
		/// Builds a path from a set of waypoints, computing turn boundaries in XZ plane and
		/// the slow-down point near the end;
		/// </summary>
		/// <param name="_waypoints">World-space waypoints the path should follow;</param>
		/// <param name="_startPos">Starting world pos (used to build the first boundary);</param>
		/// <param name="_turnDst">
		/// Distance before each waypoint at which the turn range limit should be placed
		/// to encourage earlier turning (except on the final segment);
		/// </param>
		/// <param name="_stoppingDst">
		/// Distance from the final waypoint and the segement lenght the agent should startto slow down;
		/// </param>
		public Navi_Path(Vector3[] _waypoints, Vector3 _startPos, float _turnDst, float _stoppingDst) 
		{
			lookPoints = _waypoints;
			turnBoundaries = new Line[lookPoints.Length];
			finishLineIndex = turnBoundaries.Length - 1;

			Vector2 previousPoint = V3ToV2 (_startPos);

			for (int i = 0; i < lookPoints.Length; i++) 
				{
					Vector2 currentPoint = V3ToV2 (lookPoints [i]);
					Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
					Vector2 turnBoundaryPoint = (i == finishLineIndex)?currentPoint : currentPoint - dirToCurrentPoint * _turnDst;
					turnBoundaries [i] = new Line (turnBoundaryPoint, previousPoint - dirToCurrentPoint * _turnDst);
					previousPoint = turnBoundaryPoint;
				}

			float dstFromEndPoint = 0;
			for (int i = lookPoints.Length - 1; i > 0; i--) 
				{
					dstFromEndPoint += Vector3.Distance (lookPoints [i], lookPoints [i - 1]);
					if (dstFromEndPoint > _stoppingDst) 
						{
							slowDownIndex = i;
							break;
						}
				}
		}

		public void ClearPath(Vector3[] _waypoints, Vector3 _startPos) { }

		/// <summary>
		/// Projects a 3D point to 2D (XZ -> XY ): (x, z);
		/// </summary>
		Vector2 V3ToV2(Vector3 _v3) 
			{
				return new Vector2 (_v3.x, _v3.z);
			}


#if UNITY_EDITOR
		public void DrawWithGizmos() 
			{
				Gizmos.color = Color.green;
				foreach (Vector3 p in lookPoints) {
					Gizmos.DrawCube (p , size * (2) * 0.45f);

					}

				Gizmos.color = Color.green;
				for (int i = 0; i < lookPoints.Length; i++)
					{
						if(i + 1 >= lookPoints.Length)
						return;

						Handles.DrawBezier(lookPoints[i], lookPoints[i + 1], lookPoints[i], lookPoints[i + 1],Color.green ,null, 4);
					}

				Gizmos.color = Color.white;
				foreach (Line l in turnBoundaries) 
					{
						l.DrawWithGizmos (10);
					}

				SortedGizmos.BatchCommit();
			}
#endif

    }
