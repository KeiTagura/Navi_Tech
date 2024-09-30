using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Calc a 2D line from one point and a perpendicular ref point;
/// </summary>
public struct Line 
	{
		const float verticalLineGradient = 1e5f;
		float gradient;
		float y_intercept;
		float gradientPerpendicular;
		bool approachSide;
		Vector2 pointOnLine_1;
		Vector2 pointOnLine_2;

		/// <summary>
		/// Init a <see cref="Line"/> that passes through <paramref name="_pointOnLine"/>,
		/// with orientation defined so that <paramref name="_pointPerpendicularToLine"/> lies on a
		/// line perpendicular to this line;
		/// </summary>
		/// <param name="_pointOnLine">The point that lies on the line;</param>
		/// <param name="_pointPerpendicularToLine">The point used to set the line’s normal direction (perpendicular ref);</param>
		public Line(Vector2 _pointOnLine, Vector2 _pointPerpendicularToLine) 
			{
				float dx = _pointOnLine.x - _pointPerpendicularToLine.x;
				float dy = _pointOnLine.y - _pointPerpendicularToLine.y;

				if (dx == 0) 
					{
						gradientPerpendicular = verticalLineGradient;
					} 
				else 
					{
						gradientPerpendicular = dy / dx;
					}

				if (gradientPerpendicular == 0) 
					{
						gradient = verticalLineGradient;
					} 
				else 
					{
						gradient = -1 / gradientPerpendicular;
					}

				y_intercept = _pointOnLine.y - gradient * _pointOnLine.x;
				pointOnLine_1 = _pointOnLine;
				pointOnLine_2 = _pointOnLine + new Vector2 (1, gradient);

				approachSide = false;
				approachSide = GetSide (_pointPerpendicularToLine);
			}

		/// <summary>
		/// Returns if a given point lies on a specific side of the line
		/// defined by <see cref="pointOnLine_1"/> → <see cref="pointOnLine_2"/>;
		/// </summary>
		/// <param name="_line">The point to test;</param>
		/// <returns>
		/// <c>true</c> if <paramref name="_line"/> lies on the chosen side of the directed line; otherwise, <c>false</c>;
		/// </returns>
		bool GetSide(Vector2 _line) 
			{
				return (_line.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) > (_line.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);
			}

		/// <summary>
		/// Checks if the point <paramref name="_line"/> has crossed the line relative to the
		/// original side that was made;
		/// </summary>
		/// <param name="_line">The point to test;</param>
		/// <returns>
		/// <c>true</c> if <paramref name="_line"/> is now on the opposite side of the line compared to the original approach side; otherwise, <c>false</c>;
		/// </returns>
		public bool HasCrossedLine(Vector2 _line) 
			{
				return GetSide (_line) != approachSide;
			}

		/// <summary>
		/// Checks the shortest distance from point <paramref name="_line"/> to this line;
		/// </summary>
		/// <param name="_line">The point to measure the distance from;</param>
		/// <returns>The perpendicular distance from <paramref name="_line"/> to the line;</returns>
		public float DistanceFromPoint(Vector2 _line) 
			{
				float yInterceptPerpendicular = _line.y - gradientPerpendicular * _line.x;
				float intersectX = (yInterceptPerpendicular - y_intercept) / (gradient - gradientPerpendicular);
				float intersectY = gradient * intersectX + y_intercept;
				return Vector2.Distance (_line, new Vector2 (intersectX, intersectY));
				}
		/// <summary>
		/// Draws a segment of the line;
		/// The 2D (x,y) is mapped to (x,z) plane;
		/// </summary>
		/// <param name="length">The length of the gizmo segment to draw.</param>
		public void DrawWithGizmos(float length) 
			{
				Vector3 lineDir = new Vector3 (1, 0, gradient).normalized;
				Vector3 lineCentre = new Vector3 (pointOnLine_1.x, 0, pointOnLine_1.y) + Vector3.up;
				Gizmos.DrawLine (lineCentre - lineDir * length / 2f, lineCentre + lineDir * length / 2f);
			}

	}
