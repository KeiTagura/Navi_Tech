using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
public class Pathfinder 
{

	public void StartFindPath(RequestData requestData, Vector3 startPos, Vector3 targetPos)
		{
			if(requestData.surface == null)
			return;

		requestData.surface.StartCoroutine(FindPath(requestData, startPos, targetPos));
		}

	public IEnumerator FindPath(RequestData requestData, Vector3 startPos, Vector3 targetPos)
		{

		Stopwatch sw = new Stopwatch();
		sw.Start();

		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;

		//Node startNode = grid.NodeFromWorldPoint(startPos);
		//Node targetNode = grid.NodeFromWorldPoint(targetPos);
		Node startNode = requestData.surface.WorldToGridPos(startPos);

		//UnityEngine.Debug.Log(startNode.gridX+ "-" + startNode.gridY + " startNode");
		Node targetNode = requestData.surface.WorldToGridPos(targetPos);
		startNode.parent = startNode;


		if (startNode.walkable && targetNode.walkable)
			{
			Heap<Node> openSet = new Heap<Node>(requestData.surface.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node>();
			openSet.Add(startNode);

			while (openSet.Count > 0)
				{
				Node currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);
				if (currentNode == targetNode)
					{
					sw.Stop();
					//UnityEngine.Debug.Log("Path found: " + sw.Elapsed.TotalMilliseconds + " ms");
					pathSuccess = true;
					break;
					}

				foreach (Node neighbour in requestData.surface.GetNeighbours(currentNode, 1, requestData.includeDiagonal))
					{
					if (!neighbour.walkable || closedSet.Contains(neighbour))
						{
						continue;
						}

					int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;
					if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
						{
						neighbour.gCost = newMovementCostToNeighbour;
						neighbour.hCost = GetDistance(neighbour, targetNode);
						neighbour.parent = currentNode;

						if (!openSet.Contains(neighbour))
							openSet.Add(neighbour);
						else
							openSet.UpdateItem(neighbour);
						}
					}
				}
			}
		yield return null;
		if (pathSuccess)
			{
			waypoints = RetracePath(startNode, targetNode, requestData.simplifyPath);
			}

		//UnityEngine.Debug.Log(requestData.targetNode.gridX+ "+++" + requestData.targetNode.gridY);
		CallBackData callBackData = new CallBackData { pathSuccessful = pathSuccess, waypoints = waypoints , targetNode = requestData.targetNode , distanceLimit = requestData.distanceLimit , dataSetCount = requestData.dataSetCount};
		NaviSurface.pathRequestInstance.FinishedProcessingPath(requestData, callBackData);
		//requestManager.FinishedProcessingPath(waypoints, pathSuccess);
		}

	//(Vector3[] path, PathRequest_Manager pathRequest, Pathfinder pathfinder, NavSurface surface, bool simplify, bool success)


	Vector3[] RetracePath(Node startNode, Node endNode, bool _simplifyPath = true)
		{
		List<Node> path = new List<Node>();
		List<Vector3> pathPoints = new List<Vector3>();
		Node currentNode = endNode;

		path.Add(endNode);
		pathPoints.Add(endNode.worldPosition);
		while (currentNode != startNode)
			{
				path.Add(currentNode);
				currentNode = currentNode.parent;
				pathPoints.Add(currentNode.worldPosition);
			}



		Vector3[] waypoints = SimplifyPath(path);


		Array.Reverse(waypoints);
		pathPoints.Reverse();


		pathPoints.RemoveAt(0);

		if (_simplifyPath)
			return waypoints;
		else
			return pathPoints.ToArray();

		}

	Vector3[] SimplifyPath(List<Node> path)
		{
		List<Vector3> waypoints = new List<Vector3>();
		Vector2 directionOld = Vector2.zero;

		for (int i = 1; i < path.Count; i++)
			{
			Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
			if (directionNew != directionOld)
				{
				waypoints.Add(path[i].worldPosition);
				}
			directionOld = directionNew;
			}
		return waypoints.ToArray();
		}


	int GetDistance(Node nodeA, Node nodeB)
		{
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		if (dstX > dstY)
			return 14 * dstY + 10 * (dstX - dstY);
		return 14 * dstX + 10 * (dstY - dstX);
		}
	}
