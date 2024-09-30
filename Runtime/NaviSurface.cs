using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Unity.VisualScripting;

[System.Serializable] public enum CollectObjects
    {
		All,
		Children
    }
[System.Serializable] public enum Plane
	{
		XY,
		ZX,
		ZY
	}
[System.Serializable] public enum NodeType
	{
		Square,
		Hexagon
	}
[System.Serializable] public enum PositionOffset
    {		
		TopLeft,
		TopCenter,
		TopRight,
		CenterLeft,
		Center,
		CenterRight,
		BottomLeft,
		BottomCenter,
		BottomRight,
	}
[System.Serializable] public class CallBackData
	{
		public Node targetNode;
		public int dataSetCount;
		public Vector3[] waypoints;
		public bool pathSuccessful;
		public int queueCount;
		public int distanceLimit;
	}
[System.Serializable] public class RequestData
    {
		public Pathfinder pathfinder;
		public PathRequest_Manager pathRequest;
		public NaviSurface surface;
		
		public int distanceLimit;
		public int dataSetCount;

		public Node startNode;
		public Node targetNode;
		public Vector3 pathStart;
		public Vector3 pathEnd;
		public bool includeDiagonal;
		public bool simplifyPath;
	}



/// <summary>
/// Grid-based nav surface that builds a 2D grid (on a chosen plane) of Nodes,
/// computes movement penalties (with optional blur), performs obstacle lookups,
/// and offers helpers for neighbor queries, viz, and world to grid mapping.
/// 
/// NOTE: Position offset only works when set to [Top Right]
/// The other options still need to be implemented;
/// </summary>
[System.Serializable]
public class NaviSurface : MonoBehaviour
{

#region PUB VARS
    /// public LayerMask unwalkableMask; 
    [Tooltip("Which world plane to lay the grid on.")]
    public Plane plane;


    [Tooltip("Visual/positional offset anchor of the grid within its bounds. NOTE: Only TopRight working atm.")]
    public PositionOffset positionOffset;

    [Tooltip("Square or Hexagon tiles.")]
    public NodeType nodeType;

    [Tooltip("Grid size in cells (X by Y).")]
    public Vector2 surfaceGridSize = new Vector2(10, 10);

    [Tooltip("Physics layers to consider when searching for obstacles and penalties.")]
    public LayerMask includeLayers;

    [Tooltip("Filter for what colliders count as obstacles (All vs Children of this surface).")]
    public CollectObjects collectObjects = new CollectObjects();

    [Tooltip("Half the node cell size. Node diameter is 2 * nodeRadius.")]
    public float nodeRadius = 2;
    ///	public SO_NavObstacleData[] obstacles;

    [Tooltip("Additional penalty applied near non-walkable obstacles.")]
    public int obstacleProximityPenalty = 10;

    [Tooltip("Draws the grid, penalties, and cell indices in the Scene view.")]
    public bool displayGridGizmos;

    [Tooltip("When true and a search is complete, draws nodes found within range.")]
    public bool showViableNodes;

    [Tooltip("Optional debug toggles for path requests.")]
    public bool showDebug = false;

    [Tooltip("Set true once current batch of path checks finishes.")]
    public bool setSearchComplete = false;

    [Tooltip("Optional visual pivot object for ground/grid scaling and offset.")]
    public GameObject groundVizPivot;

    // ----------------------------- Runtime / Internal State -----------------------------

    /// Mapping of layers to penalties (not fully used in this snippet);
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
    LayerMask walkableMask;

    /// <summary>2D grid of nodes (X by Y);</summary>
	//[HideInInspector]
    public Node[,] grid;

    /// Cached grid dimensions;
    int gridSizeX, gridSizeY;

    ///float nodeDiameter;

    /// <summary>Min/Max movement penalty after blurring;</summary>
    [HideInInspector] public int penaltyMin = int.MaxValue;
    [HideInInspector] public int penaltyMax = int.MinValue;

    /// Global/static references shared with pathfinding system;
    public static Pathfinder pathfinder;
    public static PathRequest_Manager pathRequestInstance;
    public static NaviSurface surface;
    ///public static bool simplifyPath = false;

    /// Drawing and range queries;
    public int discardPass = 0;
    public List<Node> nodesWithInRange = new List<Node>();
    public List<int> perPathMoves = new List<int>();
    public NaviSurfaceObserver naviSurfaceObserver;
    public WithInRangeCahce withInRangeCahce = new WithInRangeCahce();

    #endregion PUB VARS END

#region PRIV VARS
    Navi_Path path;
    float oldSize;
    int searchPass = 0;
    int lastframeNWIRCount = 0;
    List<Node> oldNodeList;
    List<Node> nodeNeighbours = new List<Node>();
    Transform oldT;
    Vector3 oldPos;
#endregion PRIV VARS END


		public float OldNavSize
			{
				set{ oldSize = value;  }
				get { return oldSize; }
			}
		public void OnEnable()
			{
				CreateGrid();
				pathfinder = new Pathfinder();
				pathRequestInstance = new PathRequest_Manager();
				surface = this;
				ResizeViz(groundVizPivot);
			}
		public void AddToObserver(NaviObstacle _naviObstacle)
			{
				naviSurfaceObserver.AddToSurfaceObstacles(_naviObstacle);
			}
		public void RemoveFromObserver(NaviObstacle _naviObstacle)
			{
				naviSurfaceObserver.RemoveFromSurfaceObstacles(_naviObstacle);
			}
		private void OnValidate()
			{
				CreateGrid();
				if(surfaceGridSize.x < 1)
					surfaceGridSize.x = 1;

				if(surfaceGridSize.y < 1)
					surfaceGridSize.y = 1;	
				ResizeViz(groundVizPivot);
			}
		private void ResizeViz(GameObject _viz)
			{
				if(_viz == null) return;
			
				Vector3 offsetCalc = new Vector3
					(
						plane == Plane.XY || plane == Plane.ZX ? 1 * nodeRadius * surfaceGridSize.x : 0.01f,
						plane == Plane.XY || plane == Plane.ZY ? 1 * nodeRadius * surfaceGridSize.y : 0.01f,
						plane == Plane.ZX ? 1 * nodeRadius * surfaceGridSize.y : plane == Plane.ZY ? -1 * nodeRadius * surfaceGridSize.x : 0.01f
					);

				_viz.transform.localScale = offsetCalc;
			}
		private void ReposViz(Vector3 _pos)
			{
				if(groundVizPivot)
					groundVizPivot.transform.GetChild(0).transform.localPosition = _pos;
        } 

		/// <summary>
		/// Queues up a path request with the global PathRequest_Manager.
		/// </summary>
		/// <param name="requestData">Request parameters (start/end, options, etc.).</param>
		/// <param name="callback">Callback invoked when the path is found or failed.</param>
        public static void RequestPath(RequestData requestData, Action<CallBackData> callback)
			{
				requestData.pathfinder = pathfinder;
				requestData.surface = surface;
				PathRequest_Manager.RequestPath(requestData, callback);
			}
		public int MaxSize
			{
				get
					{
						return gridSizeX * gridSizeY;
					}
			}
		private float RoundVal(float _val)
			{
				return (float)Math.Round(_val / nodeRadius, MidpointRounding.AwayFromZero) * nodeRadius;
			}

		/// <summary>
		/// Constrains coordinates to the active plane (XY, ZX, ZY);
		/// Optionally rounds axes to the node grid and adds an offset axis from a transform;
		/// </summary>
		/// <param name="_plane">Plane the surface is on;</param>
		/// <param name="_pos">World position to project/adjust;</param>
		/// <param name="_round">Round to nearest grid step using nodeRadius;</param>
		/// <param name="_offset">Optional transform used to fix the non-plane axis;</param>
		/// <returns>Adjusted position on the set plane;</returns>
		private Vector3 SwitchPlane(Plane _plane, Vector3 _pos, bool _round = false, Transform _offset = null)
			{
				switch (_plane)
					{
						case Plane.XY:
							_pos.x = _round ? RoundVal(_pos.x) : _pos.x;
							_pos.y = _round ? RoundVal(_pos.y) : _pos.y;
							_pos.z = _offset != null ? _offset.position.z : 0;
							break;

						case Plane.ZX:
							_pos.z = _round ? RoundVal(_pos.z) : _pos.z;
							_pos.x = _round ? RoundVal(_pos.x) : _pos.x;
							_pos.y = _offset != null ? _offset.position.y : 0;
							break;

						case Plane.ZY:
							_pos.z = _round ? RoundVal(_pos.z) : _pos.z;
							_pos.y = _round ? RoundVal(_pos.y) : _pos.y;
							_pos.x = _offset != null ? _offset.position.x : 0;
							break;

						default:
							Debug.Log("Ya'dun #$*%ed up");
							break;
						}

						return _pos;
				}

		/// <summary>
		/// Sets a position offset for the grid bounds based on the PositionOffset mode;
		/// Also repositions the viz pivot;
		/// </summary>
		/// <param name="_posOffset">Selected anchor modeP;</param>
		/// <param name="_pos">Current computed node position;</param>
		/// <returns>Offset-adjusted node position;</returns>
		private Vector3 CalcPositionOffset(PositionOffset _posOffset, Vector3 _pos)
			{
				float axisOffset = (nodeRadius * 0.5f);
				Vector3 calVal = new Vector3
					(
						plane == Plane.XY || plane == Plane.ZX ? axisOffset + ((gridSizeX * nodeRadius) * 0.5f) : transform.position.x,
						plane == Plane.XY || plane == Plane.ZY ? axisOffset + ((gridSizeY * nodeRadius) * 0.5f) : transform.position.y,
						plane == Plane.ZX ? axisOffset + ((gridSizeX * nodeRadius) * 0.5f) : plane == Plane.ZY ? axisOffset + ((gridSizeX * nodeRadius) * 0.5f) : transform.position.z
					);

				Vector2 calcOffset = Vector3.zero;

				switch (positionOffset)
					{
						case PositionOffset.BottomCenter:
								calcOffset.x = (gridSizeX * axisOffset) - axisOffset;
								calcOffset.y = axisOffset + ((gridSizeY - 1) * nodeRadius);
								_pos -= VectorOffset(calcOffset);
								ReposViz(new Vector3(0f, 0.5f, -0.5f));
							break;
						case PositionOffset.BottomLeft:
								calcOffset.x = axisOffset + ((gridSizeY - 1) * nodeRadius );
								calcOffset.y = axisOffset + ((gridSizeY - 1) * nodeRadius);
								_pos -= VectorOffset(calcOffset);
								ReposViz(new Vector3(-0.5f, 0.5f, -0.5f));

							break;
						case PositionOffset.BottomRight:
								calcOffset.x = axisOffset * -1;
								calcOffset.y = axisOffset + ((gridSizeY - 1) * nodeRadius);
								_pos -= VectorOffset(calcOffset);
								ReposViz(new Vector3(0.5f, 0.5f, -0.5f));
							break;
						case PositionOffset.Center:
								calcOffset.x = (gridSizeX * axisOffset) - axisOffset ;
								calcOffset.y = (gridSizeY * axisOffset) - axisOffset ;
								_pos -= VectorOffset(calcOffset);
								ReposViz(new Vector3(0f, 0.5f, 0f));
							break;
						case PositionOffset.CenterLeft:
								calcOffset.x = axisOffset + ((gridSizeY - 1) * nodeRadius );
								calcOffset.y = (gridSizeY * axisOffset) - axisOffset;
								_pos -= VectorOffset(calcOffset);
								ReposViz(new Vector3(-0.5f, 0.5f, 0f));
							break;
						case PositionOffset.CenterRight:
								calcOffset.x = axisOffset * -1;
								calcOffset.y = (gridSizeY * axisOffset) - axisOffset;

								_pos -= VectorOffset(calcOffset);
								ReposViz(new Vector3(0.5f, 0.5f, 0f));
							break;
						case PositionOffset.TopCenter:
								calcOffset.x = (gridSizeX * axisOffset) - axisOffset;
								calcOffset.y = axisOffset - nodeRadius ;
								_pos -= VectorOffset(calcOffset);
								ReposViz(new Vector3(0, 0.5f, 0.5f));
							break;
						case PositionOffset.TopLeft:
								calcOffset.x = axisOffset + ((gridSizeY - 1) * nodeRadius );
								calcOffset.y = axisOffset - nodeRadius ;
								_pos -= VectorOffset(calcOffset);
								ReposViz(new Vector3(-0.5f, 0.5f, 0.5f));
							break;
						case PositionOffset.TopRight:
								calcOffset.x = axisOffset * -1;
								calcOffset.y = axisOffset - nodeRadius ;
								_pos -= VectorOffset(calcOffset);
								ReposViz(new Vector3(0.5f, 0.5f, 0.5f));
						break;
						default:
						break;
					}

				return _pos;
			}

		/// <summary>
		/// Converts a 2D offset (grid-space) to a world-space Vector3 respecting the active plane.
		/// </summary>
		private Vector3 VectorOffset(Vector2 _offset)
			{
				Vector3 offsetCalc = new Vector3
					(
						plane == Plane.XY || plane == Plane.ZX ? _offset.x : 0,
						plane == Plane.XY || plane == Plane.ZY ? _offset.y : 0,
						plane == Plane.ZX ? _offset.y : plane == Plane.ZY ? _offset.x : 0
					);
				return offsetCalc;
			}

		/// <summary>
		/// Builds the grid array, computes world positions for each node, queries obstacles/penalties,
		/// and applies a blur to movement penalties;
		/// </summary>
		public void CreateGrid()
			{

				grid = new Node[(int)surfaceGridSize.x, (int)surfaceGridSize.y];
				gridSizeX = (int)surfaceGridSize.x;
				gridSizeY = (int)surfaceGridSize.y;
				surfaceGridSize = new Vector2(Mathf.FloorToInt(surfaceGridSize.x), Mathf.FloorToInt(surfaceGridSize.y));

				for (int k = 0; k < surfaceGridSize.x; k++)
					{
						for (int g = 0; g < surfaceGridSize.y; g++)
							{
								Vector3 planeSwped = SwitchPlane(plane, transform.position, false, transform);
								float t = 0;
								float q = 0;

								if(nodeType == NodeType.Hexagon)
									{
										if(g%2==1) 
											t = nodeRadius * 0.5f;
											q = nodeRadius * 0.5f;
									}

								Vector3 nodePosSwaped = new Vector3
									(
										plane == Plane.XY || plane == Plane.ZX ? planeSwped.x + (g * nodeRadius) : transform.position.x,
										plane == Plane.XY || plane == Plane.ZY ? planeSwped.y + (k * nodeRadius) + t - q : transform.position.y,
										plane == Plane.ZX ? planeSwped.z + (k * nodeRadius) + t - q : plane == Plane.ZY ? planeSwped.z + (g * nodeRadius) : transform.position.z
									);

								nodePosSwaped = CalcPositionOffset(positionOffset, nodePosSwaped); 

								int movementPenalty = ObstacleSearch(nodePosSwaped, nodeRadius * 0.5f, out bool walkable);

								grid[k, g] = new Node(walkable, nodePosSwaped, k, g, movementPenalty);
							}
					}

				BlurPenaltyMap(3);
			}
		/// <summary>
		/// Raycast down from above the world point to find obstacles and movement penalties.
		/// </summary>
		/// <param name="_worldPoint">Node center position in world space.</param>
		/// <param name="_searchRadius">Half-size of the box cast used for detection.</param>
		/// <param name="_walkable">Returns true if the node is walkable.</param>
		/// <returns>Computed movement penalty for this node.</returns>
		private int  ObstacleSearch(Vector3 _worldPoint, float _searchRadius, out bool _walkable)
			{
				int movementPenalty = 0;
				Vector3 point = _worldPoint + Vector3.up * 80;
				Ray ray = new Ray(point, Vector3.down);
				RaycastHit hit;
				
				//	if(Physics.SphereCast(ray,searchRadius,out hit,100,includeLayers))					
				if(Physics.BoxCast(point, Vector3.one * _searchRadius, Vector3.down, out hit, Quaternion.identity, 100, includeLayers))
					{
						if(collectObjects == CollectObjects.Children)
							if(!hit.collider.transform.IsChildOf(transform))
								{
									_walkable = true;
									return movementPenalty;
									
								}

						if(hit.collider.TryGetComponent<NaviObstacle>(out NaviObstacle navObstacle))
							{
								navObstacle.lastRegisterdSurface = this;
								movementPenalty = navObstacle.obstacleData.penalty;
								
								_walkable = navObstacle.obstacleData.walkable;

								if (!_walkable)
								return	movementPenalty += obstacleProximityPenalty;
								else
								return movementPenalty;
							}
					}
				_walkable = true;
				return movementPenalty;
			}

		/// <summary>
		/// Box blur over movement penalties to avoid sharp cost being disjointed.
		/// </summary>
		/// <param name="_blurSize">Kernel radius (actual kernel = 2*blurSize + 1).</param>
		private void BlurPenaltyMap(int _blurSize)
			{
				if (gridSizeX <= _blurSize || gridSizeY <= _blurSize)
					return;

				int kernelSize = _blurSize * 2 + 1;
				int kernelExtents = (kernelSize - 1) / 2;

				int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
				int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

				for (int y = 0; y < gridSizeY; y++)
					{
						for (int x = -kernelExtents; x <= kernelExtents; x++)
							{
								int sampleX = Mathf.Clamp(x, 0, kernelExtents);
								penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
							}

						for (int x = 1; x < gridSizeX; x++)
							{
								int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
								int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

								penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
							}
					}

				for (int x = 0; x < gridSizeX; x++)
					{
						for (int y = -kernelExtents; y <= kernelExtents; y++)
							{
								int sampleY = Mathf.Clamp(y, 0, kernelExtents);
								penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
							}

						int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
						grid[x, 0].movementPenalty = blurredPenalty;

						for (int y = 1; y < gridSizeY; y++)
							{
								int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
								int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);

								penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
								blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
								grid[x, y].movementPenalty = blurredPenalty;

								if (blurredPenalty > penaltyMax)
									{
										penaltyMax = blurredPenalty;
									}
								if (blurredPenalty < penaltyMin)
									{
										penaltyMin = blurredPenalty;
									}
							}
					}
			}

		/// <summary>
		/// Returns neighbors around a node within a <paramref name="_distance"/>.
		/// For hex grids, filters two diagonal combinations to match hex adjacency.
		/// </summary>
		/// <param name="_node">Center node.</param>
		/// <param name="_distance">Neighborhood radius in cells.</param>
		/// <param name="_includeDiagonal">If false, excludes corner-diagonal neighbors.</param>
		public List<Node> GetNeighbours(Node _node, int _distance = 1,  bool _includeDiagonal = true)
			{
				List<Node> neighbours = new List<Node>();

				for (int x = -_distance; x <= _distance; x++)
					{
						for (int y = -_distance; y <= _distance; y++)
							{
								if (x == 0 && y == 0)
									continue;

								if(nodeType == NodeType.Hexagon)
									if((y == _distance && x == -_distance) || (y == _distance && x == _distance))
									continue;

								if (!_includeDiagonal)
									if ((x == -_distance || x == _distance) && (y == -_distance || y == _distance))
										continue;

								int checkX = _node.gridX + x;
								int checkY = _node.gridY + y;

								if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
									{
										neighbours.Add(grid[checkX, checkY]);
									}
							}
					}
				return neighbours;
			}

		/// <summary>
		/// Converts a world position to its closest Node on this grid.
		/// </summary>
		/// <param name="_worldPos">The position of the current object to be snapped.</param>
		/// <param name="_convertToLocalSpace">Should the returned node be in the NaviSurface's local position (defaulted to FALSE)</param>
		/// <returns></returns>
		public Node WorldToGridPos(Vector3 _worldPos, bool _convertToLocalSpace = true)
			{
				float axisOffset = (nodeRadius * 0.5f);

				Vector3 offsetCalc = new Vector3
					(
						plane == Plane.XY || plane == Plane.ZX ? _worldPos.x - axisOffset : _worldPos.x,
						plane == Plane.XY || plane == Plane.ZY ? _worldPos.y - axisOffset : _worldPos.y,
						plane == Plane.ZX || plane == Plane.ZY ? _worldPos.z - axisOffset : _worldPos.z
					);

				float gridcellYSize =  (surfaceGridSize.y * nodeRadius) / (surfaceGridSize.y ) ;
				float gridcellXSize =  (surfaceGridSize.x * nodeRadius) / surfaceGridSize.x  ;
				float gridcellZSize = plane == Plane.ZX ? gridcellYSize : plane == Plane.ZY ? gridcellXSize : 0;
				float gridX = Mathf.Abs(Mathf.Round(((offsetCalc.x ) - transform.position.x) / gridcellXSize));
				float gridY = Mathf.Abs(Mathf.Round(((offsetCalc.y ) - transform.position.y) / gridcellYSize));
				float gridZ = Mathf.Abs(Mathf.Round(((offsetCalc.z ) - transform.position.z) / (plane == Plane.ZX ? gridcellYSize : gridcellXSize)));

				gridX = plane == Plane.ZY ? gridZ : gridX;
				gridY = plane == Plane.ZX ? gridZ : gridY;
			
				Node target = null;
			
				if( grid.GetLength(0)-1 < (int)gridX || grid.GetLength(1)-1 < (int)gridY || (int)gridY < 0 || (int)gridX < 0)
					return new Node(true);

				target = grid[(int)gridY, (int)gridX];
			
			
				//if(target != null)
				return target;
			}

		/// <summary>
		/// Returns the last computed neighbor list;
		/// </summary>
		public List<Node> GetNodeNeighours()
			{
				return nodeNeighbours;
			}

		/// <summary>
		/// FIX: Card Interaction 
		/// </summary>
		/*
		public List<Node> GetNeighboursWithinDistance(Transform t, Card_Interaction card, int distance, bool canCrossDiagonal)
			{
				if(t == oldT && t.position == oldPos)
					return nodesWithInRange = withInRangeCahce.ReturnListWithInDistance(distance); 

				else //(card != oldCard)
				{
					nodeNeighbours.Clear();
					nodesWithInRange.Clear();
					searchPass = 0;
					discardPass = 0;
					setSearchComplete = false;
					showViableNodes = false;
					withInRangeCahce.ClearAll();
				}

			

				lastframeNWIRCount = 0;

				perPathMoves.Clear();
		
				Node node = surface.WorldToGridPos(t.position);

				RequestData requestData  = new RequestData 
					{ 
						pathStart = t.position, 
						includeDiagonal = canCrossDiagonal, 
						simplifyPath = false, 
						//targetNode = node, 
						distanceLimit = distance,
						dataSetCount = nodeNeighbours.Count
					};

				for (int i = 0; i < nodeNeighbours.Count; i++)
					{
						requestData.targetNode = nodeNeighbours[i];
						requestData.pathEnd = nodeNeighbours[i].worldPosition;
						CheckPath(t.position, nodeNeighbours[i].worldPosition, canCrossDiagonal, nodeNeighbours.Count, nodeNeighbours[i], distance);
					}
			
				oldT = t;
				oldPos = t.position;
				oldNodeList = nodeNeighbours;
				oldCard = card;

				return nodeNeighbours;
			}
		*/


		/// <summary>
		/// Helper class to hold sets of nodes within distances 1..4 from a center.
		/// </summary>
		public class WithInRangeCahce
			{
				public List<Node> nodes1Distance = new List<Node>();
				public List<Node> nodes2Distance = new List<Node>();
				public List<Node> nodes3Distance = new List<Node>();
				public List<Node> nodes4Distance = new List<Node>();

				public void ClearAll()
					{
						nodes1Distance.Clear();
						nodes2Distance.Clear();
						nodes3Distance.Clear();
						nodes4Distance.Clear();
					}

				public int CountAll()
					{
						return nodes1Distance.Count +
						nodes2Distance.Count +
						nodes3Distance.Count +
						nodes4Distance.Count;
					}

				public List<Node> ReturnListWithInDistance(int _dist)
				{
					switch (_dist)
						{
						case 0:
							return new List<Node>();
						case 1:
							return nodes1Distance;
						case 2:
							return nodes2Distance;
						case 3:
							return nodes3Distance;
						case 4:
							return nodes4Distance;
						default:
							Debug.LogError("Distance value exceeds possible range.");
							return new List<Node>();
					}
				}
			}

		/// <summary>
		/// Callback invoked when a path is found (or failed):
		/// Updates within-range nodes and toggles search completion when all results done;
		/// </summary>
		private void OnPathFound(CallBackData _callBackData)
			{
				searchPass++;

			if (_callBackData.pathSuccessful)
				{

					//Debug.Log("checkNeh  - " + callBackData.distanceLimit);
					path = new Navi_Path(_callBackData.waypoints, transform.position, 0, 0);

				//	if (path.lookPoints.Length <= callBackData.distanceLimit)
				//		nodesWithInRange.Add(callBackData.targetNode);


					if(path.lookPoints.Length <= 1)
					{
						withInRangeCahce.nodes1Distance.Add(_callBackData.targetNode);
					}
			
					if (path.lookPoints.Length <= 2)
					{
				
						withInRangeCahce.nodes2Distance.Add(_callBackData.targetNode);
					}
			
					if (path.lookPoints.Length <= 3)
					{
					
						withInRangeCahce.nodes3Distance.Add(_callBackData.targetNode);
					}
			
					if (path.lookPoints.Length <= 4)
					{
				
						withInRangeCahce.nodes4Distance.Add(_callBackData.targetNode);
					}

				nodesWithInRange = withInRangeCahce.ReturnListWithInDistance(_callBackData.distanceLimit);
				//else
				//	discardPass++;

				//Debug.Log(path.lookPoints.Length);
				}
		//	else
			//	discardPass++;

					//Debug.Log(searchPass + " =-= " + callBackData.dataSetCount  + " =-= " + discardPass);

					if( _callBackData.dataSetCount == (discardPass +  searchPass))
						{
							setSearchComplete = true;
						}
					else
						{
							setSearchComplete = false;
						}
			}
		private void ShowViableNodes( RectTransform _card, bool _show)
			{
				if(_show)
					{
						showViableNodes = true;
					}
				else
					{
						showViableNodes = false;
					}
			}

		/// <summary>
		/// Queues a path check from origin to destination and tracks per-node distance limits;
		/// Early outs and skips non-walkable nodes;
		/// </summary>
		/// <param name="_origin">World start position;</param>
		/// <param name="_destination">World end position;</param>
		/// <param name="_canCrossDiagonal">Allow diagonal moves</param>
		/// <param name="_count">Total nodes being checked in this batch;</param>
		/// <param name="_node">Target node for this path test;</param>
		/// <param name="_distance">Max distance to check;</param>
		public void CheckPath(Vector3 _origin, Vector3 _destination, bool _canCrossDiagonal, int _count, Node _node, int _distance)
			{
				RequestData requestData = new RequestData 
					{ 
						pathStart = _origin, 
						pathEnd = _destination, 
						includeDiagonal = _canCrossDiagonal, 
						simplifyPath = false, 
						targetNode = _node, 
						distanceLimit = _distance,
						dataSetCount = _count
					};

				if (requestData.targetNode.walkable == false)
					{
						discardPass++;
						return;
					}

				path = null;
				NaviSurface.RequestPath(requestData , OnPathFound);
			}


#if UNITY_EDITOR

		public bool displayGridPosition = true;
		public bool displayPenaltyCost = true;
		public bool displayGrid = true;
		void OnDrawGizmos()
			{
				Vector3 nodePosSwaped = new Vector3();
				float scale = 0.2f;

				switch (plane)
					{
						case Plane.XY:
						nodePosSwaped = (new Vector3(0,0,1) * scale) + new Vector3(1,1,0);
							break;
						case Plane.ZX:
						nodePosSwaped = (new Vector3(0,1,0) * scale) +new Vector3(1, 0, 1);
							break; 
						case Plane.ZY:
						nodePosSwaped = (new Vector3(1,0,0) * scale) +new Vector3(0, 1, 1);
							break;
						default:
						Debug.Log("Ya'dun fked up somewhere");
							break;
					}

				if (grid != null && (displayGrid || displayPenaltyCost || displayGridPosition))
					{
						foreach (Node n in grid)
							{
								//Gizmos.color = Color.Lerp(new Color(1,1,1,0.25f), new Color(0,0,0,0.95f), Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));
								Gizmos.color = Color.Lerp(new Color(1,1,1,0.0625f), new Color(0, 0, 0, 1f), Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty*1.125f));
								Gizmos.color = (n.walkable) ? Gizmos.color : new Color(1,0,0, 0.5f);
								if(displayPenaltyCost)
									Gizmos.DrawCube(n.worldPosition, Vector3.Scale(Vector3.one * (nodeRadius) * 0.75f, nodePosSwaped));
								if(displayGrid)
									Gizmos.DrawWireCube(n.worldPosition, Vector3.Scale(Vector3.one * (nodeRadius) * 0.9f, nodePosSwaped));
								Handles.color =  Color.blue;
								if(displayGridPosition)
									Handles.Label(n.worldPosition, "[" + n.gridX + "," + n.gridY + "]");
							}
					}
				if( showViableNodes && setSearchComplete )
					{
						foreach (Node n in nodesWithInRange)
							{
								Gizmos.color = Color.blue;
								Gizmos.DrawCube(n.worldPosition, Vector3.Scale(Vector3.one * (nodeRadius) * 0.8f, nodePosSwaped));
							}
					}
				lastframeNWIRCount = nodesWithInRange.Count;
			}
#endif
    }
