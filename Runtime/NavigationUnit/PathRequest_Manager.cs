using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class PathRequest_Manager
{

	Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
	PathRequest currentPathRequest;
	public static bool isProcessingPath;
	//Pathfinder pathfinder;

	//PathRequest_Manager.RequestPath(pathRequest, pathfinder, surface, pathStart, pathEnd, simplifyPath, callback);
	//public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Pathfinder pathfinder, NavSurface surface, Action<Vector3[], bool> callback)


	public static void RequestPath( RequestData requestData, Action<CallBackData> callback)
		{

			if (requestData.surface == null)
				return;


			PathRequest newRequest = new PathRequest(requestData.pathStart, requestData.pathEnd, requestData.pathfinder, requestData.targetNode , callback);
			NaviSurface.pathRequestInstance.pathRequestQueue.Enqueue(newRequest);
			NaviSurface.pathRequestInstance.TryProcessNext(requestData);
			//return instance.pathfinding.isCalculatingPath;
		}

	void TryProcessNext(RequestData requestData)
		{
			if (!isProcessingPath && pathRequestQueue.Count > 0)
				{
				currentPathRequest = pathRequestQueue.Dequeue();
				isProcessingPath = true;

				//Debug.Log(currentPathRequest.targetNode.gridX + "+++" + currentPathRequest.targetNode.gridY);

				requestData.targetNode = currentPathRequest.targetNode;

				NaviSurface.pathfinder.StartFindPath(requestData, currentPathRequest.pathStart, currentPathRequest.pathEnd);
				}
		}

	public void FinishedProcessingPath( RequestData requestData, CallBackData callBackData)
		{
			currentPathRequest.callback(callBackData);
			isProcessingPath = false;
			NaviSurface.pathRequestInstance.TryProcessNext(requestData);
		}


	struct PathRequest
		{
			public Vector3 pathStart;
			public Vector3 pathEnd;
			public Action<CallBackData> callback;
			public Pathfinder pathfinder;
			public Node targetNode;

			public PathRequest(Vector3 _start, Vector3 _end, Pathfinder _pathfinder, Node _targetNode,  Action<CallBackData> _callback)
				{
					pathStart = _start;
					pathEnd = _end;
					callback = _callback;
					pathfinder = _pathfinder;
					targetNode = _targetNode;
				}

		}
	}
