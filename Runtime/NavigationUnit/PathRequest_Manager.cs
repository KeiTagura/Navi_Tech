using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Manages a FIFO queue of path requests and processes them one at a time.
/// Works with <see cref="Pathfinder"/> and the active <see cref="NaviSurface"/>.
/// </summary>
public class PathRequest_Manager
	{

		Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
		PathRequest currentPathRequest;
		public static bool isProcessingPath;

		//Pathfinder pathfinder;
		//PathRequest_Manager.RequestPath(pathRequest, pathfinder, surface, pathStart, pathEnd, simplifyPath, callback);
		//public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Pathfinder pathfinder, NavSurface surface, Action<Vector3[], bool> callback)

		/// <summary>
		/// Enqueue a new path request and attempt to start processing if idle;
		/// </summary>
		/// <param name="_requestData">Params for the path search (start/end, options, surface);</param>
		/// <param name="_callback">Callback invoked when the search completes;</param>
		public static void RequestPath( RequestData _requestData, Action<CallBackData> _callback)
			{

				if (_requestData.surface == null)
					return;

				PathRequest newRequest = new PathRequest(_requestData.pathStart, _requestData.pathEnd, _requestData.pathfinder, _requestData.targetNode , _callback);
				NaviSurface.pathRequestInstance.pathRequestQueue.Enqueue(newRequest);
				NaviSurface.pathRequestInstance.TryProcessNext(_requestData);
			}

		/// <summary>
		/// If not already processing and the queue is non-empty, dequeue the next request and start it;
		/// </summary>
		/// <param name="_requestData">Request data object to pass through to the pathfinder;</param>
		void TryProcessNext(RequestData _requestData)
			{
				if (!isProcessingPath && pathRequestQueue.Count > 0)
					{
						currentPathRequest = pathRequestQueue.Dequeue();
						isProcessingPath = true;

						//Debug.Log(currentPathRequest.targetNode.gridX + "+++" + currentPathRequest.targetNode.gridY);
						_requestData.targetNode = currentPathRequest.targetNode;

						NaviSurface.pathfinder.StartFindPath(_requestData, currentPathRequest.pathStart, currentPathRequest.pathEnd);
					}
			}

		/// <summary>
		/// Called by the pathfinder when a path search completes;
		/// Dispatches the callback, clears the isProcessing flag, and tries the next request;
		/// </summary>
		/// <param name="_requestData">Original request data (passed along for chaining);</param>
		/// <param name="_callBackData">Result payload (success flag, waypoints, etc.);</param>
		public void FinishedProcessingPath( RequestData _requestData, CallBackData _callBackData)
			{
				currentPathRequest.callback(_callBackData);
				isProcessingPath = false;
				NaviSurface.pathRequestInstance.TryProcessNext(_requestData);
			}

		 /// <summary>
        /// Makes a new path request;
        /// </summary>
        /// <param name="_start">World start position;</param>
        /// <param name="_end">World end position;</param>
        /// <param name="_pathfinder">Pathfinder to run the search;</param>
        /// <param name="_targetNode">Target node for bookkeeping/feedback;</param>
        /// <param name="_callback">Callback to invoke when finished;</param>
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
