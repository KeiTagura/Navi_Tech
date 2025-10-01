
using UnityEngine;
using System;


/// <summary>
/// Util for snapping arbitrary world positions to the nearest cell center on a given <see cref="NaviSurface"/> grid.
/// </summary>
public static class SnapToGrid
{
    /// <summary>
    /// Returns the nearest WorldSpace grid point on <paramref name="_surface"/> to <paramref name="_val"/>,
    /// and reports whether that snapped cell is walkable;
    /// </summary>
    /// <param name="_val">World-space position to snap;</param>
    /// <param name="_surface">Target nav surface whose grid definition is used;</param>
    /// <param name="_walkable">Outputs whether the snapped node is walkable;</param>
    /// <returns>WorldSpcae position of the nearest grid point;</returns>
    public static Vector3 GetNearestGridPoint(Vector3 _val, NaviSurface _surface, out bool _walkable)
		{
			Vector3 worldPosition = _surface.transform.position;
			Vector3 surfaceGridSize = _surface.surfaceGridSize;

			float nodeRadius = _surface.nodeRadius;
			float nodeDiameter = nodeRadius * 2;

			int gridSizeX = (int)Math.Round(surfaceGridSize.x / nodeDiameter, MidpointRounding.AwayFromZero);
			int gridSizeY = (int)Math.Round(surfaceGridSize.y / nodeDiameter, MidpointRounding.AwayFromZero);

			Vector2 gridSize = new Vector2(gridSizeX, gridSizeY);
			Vector3 gridPosition = _surface.transform.InverseTransformPoint(_val);

			gridPosition.x = (int)Math.Round(gridPosition.x * gridSize.x, MidpointRounding.AwayFromZero) / gridSize.x;
			gridPosition.y = worldPosition.y;
			gridPosition.z = (int)Math.Round(gridPosition.z * gridSize.y, MidpointRounding.AwayFromZero) / gridSize.y;

			gridPosition.x = Mathf.Min(0.5f, Mathf.Max(-0.5f, gridPosition.x));
			_val.z = Mathf.Min(0.5f, Mathf.Max(-0.5f, gridPosition.z));

			gridPosition = _surface.transform.TransformPoint(gridPosition);
			_walkable = _surface.WorldToGridPos(gridPosition).walkable;

			Debug.Log(_walkable + " - " + _surface.WorldToGridPos(gridPosition).gridX + " - " + _surface.WorldToGridPos(gridPosition).gridY);
			return gridPosition;
		}

	}
