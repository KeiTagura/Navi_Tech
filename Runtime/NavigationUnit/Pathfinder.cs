using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

/// <summary>
/// A* pathfinder that runs as a Unity coroutine:
/// - Translates world positions to grid Nodes via the active <see cref="NaviSurface"/>.
/// - Uses a binary heap (open set) and a hash set (closed set).
/// - Supports diagonal motion and per-node movement penalties.
/// - Returns waypoints (optionally simplified) via the path request manager callback.
/// </summary>
public class Pathfinder 
	{
		public void StartFindPath(RequestData _requestData, Vector3 _startPos, Vector3 _targetPos)
			{
				if(_requestData.surface == null)
				return;

				_requestData.surface.StartCoroutine(FindPath(_requestData, _startPos, _targetPos));
			}

		/// <summary>
		/// Coroutine that performs an A* search and dispatches the results;
		/// </summary>
		/// <param name="_requestData">Path request parameters and options;</param>
		/// <param name="_startPos">World start position.</param>
		/// <param name="_targetPos">World target position;</param>
		/// <returns>Enumerator for Unity's coroutine scheduler;</returns>

        public IEnumerator FindPath(RequestData _requestData, Vector3 _startPos, Vector3 _targetPos)
			{

				Stopwatch sw = new Stopwatch();
				sw.Start();

				Vector3[] waypoints = new Vector3[0];
				bool pathSuccess = false;
				Node startNode = _requestData.surface.WorldToGridPos(_startPos);
				Node targetNode = _requestData.surface.WorldToGridPos(_targetPos);
				startNode.parent = startNode;


				if (startNode.walkable && targetNode.walkable)
					{
						Heap<Node> openSet = new Heap<Node>(_requestData.surface.MaxSize);
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

								foreach (Node neighbour in _requestData.surface.GetNeighbours(currentNode, 1, _requestData.includeDiagonal))
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
						waypoints = RetracePath(startNode, targetNode, _requestData.simplifyPath);
					}

				//UnityEngine.Debug.Log(requestData.targetNode.gridX+ "+++" + requestData.targetNode.gridY);
				CallBackData callBackData = new CallBackData { pathSuccessful = pathSuccess, waypoints = waypoints , targetNode = _requestData.targetNode , distanceLimit = _requestData.distanceLimit , dataSetCount = _requestData.dataSetCount};
				NaviSurface.pathRequestInstance.FinishedProcessingPath(_requestData, callBackData);
				//requestManager.FinishedProcessingPath(waypoints, pathSuccess);
			}

		/// <summary>
		/// Walks back from end to start via <see cref="Node.parent"/> to make a path,
		/// can also simplifies id, and ensures start -> end ordering;
		/// </summary>
		/// <param name="_startNode">Start node;</param>
		/// <param name="_endNode">End node.</param>
		/// <param name="_simplifyPath">If true, collinear steps are collapsed.</param>
		/// <returns>WorldSpace waypoints from start to end.</returns>
		Vector3[] RetracePath(Node _startNode, Node _endNode, bool _simplifyPath = true)
			{
				List<Node> path = new List<Node>();
				List<Vector3> pathPoints = new List<Vector3>();
				Node currentNode = _endNode;

				path.Add(_endNode);
				pathPoints.Add(_endNode.worldPosition);
				while (currentNode != _startNode)
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

		/// <summary>
		/// Reduces a node path to corner waypoints by only adding a point when direction changes;
		/// Works in grid space using (gridX, gridY) deltass;
		/// </summary>
		/// <param name="_path">Nodes from end → start (as built in <see cref="RetracePath"/>).</param>
		Vector3[] SimplifyPath(List<Node> _path)
			{
				List<Vector3> waypoints = new List<Vector3>();
				Vector2 directionOld = Vector2.zero;

				for (int i = 1; i < _path.Count; i++)
					{
						Vector2 directionNew = new Vector2(_path[i - 1].gridX - _path[i].gridX, _path[i - 1].gridY - _path[i].gridY);
						if (directionNew != directionOld)
							{
								waypoints.Add(_path[i].worldPosition);
							}
						directionOld = directionNew;
					}
				return waypoints.ToArray();
			}

		/// <summary>
		/// Octile distance (A* heuristic and step cost);
		/// </summary>
		/// <param name="_nodeA">First node.</param>
		/// <param name="_nodeB">Second node.</param>
		/// <returns>Scaled integer distance between nodes.</returns>
		int GetDistance(Node _nodeA, Node _nodeB)
			{
				int dstX = Mathf.Abs(_nodeA.gridX - _nodeB.gridX);
				int dstY = Mathf.Abs(_nodeA.gridY - _nodeB.gridY);

				if (dstX > dstY)
					return 14 * dstY + 10 * (dstX - dstY);
				return 14 * dstX + 10 * (dstY - dstX);
			}
	}
