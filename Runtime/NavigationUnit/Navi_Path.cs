using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Navi_Path {

	public readonly Vector3[] lookPoints;
	public readonly Line[] turnBoundaries;
	public readonly int finishLineIndex;
	public readonly int slowDownIndex;

	public Navi_Path(Vector3[] waypoints, Vector3 startPos, float turnDst, float stoppingDst) {
		lookPoints = waypoints;
		turnBoundaries = new Line[lookPoints.Length];
		finishLineIndex = turnBoundaries.Length - 1;

		Vector2 previousPoint = V3ToV2 (startPos);
		for (int i = 0; i < lookPoints.Length; i++) {
			Vector2 currentPoint = V3ToV2 (lookPoints [i]);
			Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
			Vector2 turnBoundaryPoint = (i == finishLineIndex)?currentPoint : currentPoint - dirToCurrentPoint * turnDst;
			turnBoundaries [i] = new Line (turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);
			previousPoint = turnBoundaryPoint;
		}

		float dstFromEndPoint = 0;
		for (int i = lookPoints.Length - 1; i > 0; i--) {
			dstFromEndPoint += Vector3.Distance (lookPoints [i], lookPoints [i - 1]);
			if (dstFromEndPoint > stoppingDst) {
				slowDownIndex = i;
				break;
			}
		}
	}

	public void ClearPath(Vector3[] waypoints, Vector3 startPos)
        {

        }

	Vector2 V3ToV2(Vector3 v3) {
		return new Vector2 (v3.x, v3.z);
	}

	Vector3 size =  Vector3.one; 

#if UNITY_EDITOR
	public void DrawWithGizmos() {
		Gizmos.color = Color.green;
		foreach (Vector3 p in lookPoints) {
			Gizmos.DrawCube (p , size * (2) * 0.45f);
			//SortedGizmos.DrawCube(p + Vector3.up, size);

			}

		Gizmos.color = Color.green;
		for (int i = 0; i < lookPoints.Length; i++)
            {
				if(i + 1 >= lookPoints.Length)
				return;

				//Gizmos.DrawLine(lookPoints[i], lookPoints[i+1]); ;
			Handles.DrawBezier(lookPoints[i], lookPoints[i + 1], lookPoints[i], lookPoints[i + 1],Color.green ,null, 4);
			}

		Gizmos.color = Color.white;
		foreach (Line l in turnBoundaries) {
			l.DrawWithGizmos (10);
		}

		SortedGizmos.BatchCommit();
        }
#endif

    }
