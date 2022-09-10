using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class SnapToGrid
{

	public static Vector3 GetNearestGridPoint(Vector3 val, NaviSurface surface, out bool walkable)
		{
			Vector3 worldPosition = surface.transform.position;
			Vector3 surfaceGridSize = surface.surfaceGridSize;

			float nodeRadius = surface.nodeRadius;
			float nodeDiameter = nodeRadius * 2;

			int gridSizeX = (int)Math.Round(surfaceGridSize.x / nodeDiameter, MidpointRounding.AwayFromZero);
			int gridSizeY = (int)Math.Round(surfaceGridSize.y / nodeDiameter, MidpointRounding.AwayFromZero);

			Vector2 gridSize = new Vector2(gridSizeX, gridSizeY);
			Vector3 gridPosition = surface.transform.InverseTransformPoint(val);

			gridPosition.x = (int)Math.Round(gridPosition.x * gridSize.x, MidpointRounding.AwayFromZero) / gridSize.x;
			gridPosition.y = worldPosition.y;
			gridPosition.z = (int)Math.Round(gridPosition.z * gridSize.y, MidpointRounding.AwayFromZero) / gridSize.y;

			gridPosition.x = Mathf.Min(0.5f, Mathf.Max(-0.5f, gridPosition.x));
			val.z = Mathf.Min(0.5f, Mathf.Max(-0.5f, gridPosition.z));

			gridPosition = surface.transform.TransformPoint(gridPosition);
			walkable = surface.WorldToGridPos(gridPosition).walkable;

			Debug.Log(walkable + " - " + surface.WorldToGridPos(gridPosition).gridX + " - " + surface.WorldToGridPos(gridPosition).gridY);
			return gridPosition;
		}

	}
