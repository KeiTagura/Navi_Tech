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


[System.Serializable]
public class NaviSurface : MonoBehaviour
{

	// public LayerMask unwalkableMask;
	public Plane plane;
	public PositionOffset positionOffset;
	public NodeType nodeType;
	public Vector2 surfaceGridSize = new Vector2(10,10);
	public LayerMask includeLayers;
	public CollectObjects collectObjects = new CollectObjects();
	public float nodeRadius = 2;
	//	public SO_NavObstacleData[] obstacles;

	public int obstacleProximityPenalty = 10;
	Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
	LayerMask walkableMask;

	//[HideInInspector]
	public Node[,] grid;

	//float nodeDiameter;
	int gridSizeX, gridSizeY;

	[HideInInspector]
	public int penaltyMin = int.MaxValue;
	[HideInInspector]
	public int penaltyMax = int.MinValue;

	public static Pathfinder pathfinder;
	public static PathRequest_Manager pathRequestInstance;
	public static NaviSurface surface;
	//public static bool simplifyPath = false;
	public bool displayGridGizmos;
	public bool showViableNodes;

	Navi_Path path;
	public List<int> perPathMoves = new List<int>();
	public WithInRangeCahce withInRangeCahce = new WithInRangeCahce();
	int searchPass = 0;
	public int discardPass = 0;
	int lastframeNWIRCount = 0;
	public List<Node> nodesWithInRange = new List<Node>();
	public bool setSearchComplete = false;
	List<Node> nodeNeighbours = new List<Node>();
	Transform oldT;
	Vector3 oldPos;
	List<Node> oldNodeList;
	//Card_Interaction oldCard;
	public GameObject groundViz;
	public NaviSurfaceObserver naviSurfaceObserver;
	float oldSize;

    public bool showDebug = false;
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
			
			ResizeViz(groundViz);
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
			ResizeViz(groundViz);
        }
	private void ResizeViz(GameObject viz)
		{
			if(viz == null) return;
			
			Vector3 offsetCalc = new Vector3
				(
					plane == Plane.XY || plane == Plane.ZX ? 1 * nodeRadius * surfaceGridSize.x : 0.01f,
					plane == Plane.XY || plane == Plane.ZY ? 1 * nodeRadius * surfaceGridSize.y : 0.01f,
                    plane == Plane.ZX ? 1 * nodeRadius * surfaceGridSize.y : plane == Plane.ZY ? -1 * nodeRadius * surfaceGridSize.x : 0.01f
                );

			viz.transform.localScale = offsetCalc;
		}
    public static void RequestPath(RequestData requestData, Action<CallBackData> callback)
		{
			//requestData.pathRequest = pathRequest;
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
	public float RoundVal(float val)
		{
			return (float)Math.Round(val / nodeRadius, MidpointRounding.AwayFromZero) * nodeRadius;
		}
	public Vector3 SwitchPlane(Plane _plane, Vector3 pos, bool round = false, Transform offset = null)
		{
			switch (_plane)
				{
					case Plane.XY:
						pos.x = round ? RoundVal(pos.x) : pos.x;
						pos.y = round ? RoundVal(pos.y) : pos.y;
						pos.z = offset != null ? offset.position.z : 0;
						break;

					case Plane.ZX:
						pos.z = round ? RoundVal(pos.z) : pos.z;
						pos.x = round ? RoundVal(pos.x) : pos.x;
						pos.y = offset != null ? offset.position.y : 0;
						break;

					case Plane.ZY:
						pos.z = round ? RoundVal(pos.z) : pos.z;
						pos.y = round ? RoundVal(pos.y) : pos.y;
						pos.x = offset != null ? offset.position.x : 0;
						break;

					default:
						Debug.Log("Ya'dun #$*%ed up");
						break;
					}

					return pos;
			}
    public Vector3 CalcPositionOffset(PositionOffset posOffset, Vector3 pos)
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
							pos -= VectorOffset(calcOffset);
						break;
					case PositionOffset.BottomLeft:
							calcOffset.x = axisOffset + ((gridSizeY - 1) * nodeRadius );
							calcOffset.y = axisOffset + ((gridSizeY - 1) * nodeRadius);
							pos -= VectorOffset(calcOffset);

						break;
					case PositionOffset.BottomRight:
							calcOffset.x = axisOffset * -1;
							calcOffset.y = axisOffset + ((gridSizeY - 1) * nodeRadius);
							pos -= VectorOffset(calcOffset);
						break;
					case PositionOffset.Center:
							calcOffset.x = (gridSizeX * axisOffset) - axisOffset ;
							calcOffset.y = (gridSizeY * axisOffset) - axisOffset ;
							pos -= VectorOffset(calcOffset);
						break;
					case PositionOffset.CenterLeft:
							calcOffset.x = axisOffset + ((gridSizeY - 1) * nodeRadius );
							calcOffset.y = (gridSizeY * axisOffset) - axisOffset;

							pos -= VectorOffset(calcOffset);
						break;
					case PositionOffset.CenterRight:
							calcOffset.x = axisOffset * -1;
							calcOffset.y = (gridSizeY * axisOffset) - axisOffset;

							pos -= VectorOffset(calcOffset);
						break;
					case PositionOffset.TopCenter:
							calcOffset.x = (gridSizeX * axisOffset) - axisOffset;
							calcOffset.y = axisOffset - nodeRadius ;
							pos -= VectorOffset(calcOffset);

						break;
					case PositionOffset.TopLeft:
							calcOffset.x = axisOffset + ((gridSizeY - 1) * nodeRadius );
							calcOffset.y = axisOffset - nodeRadius ;

							pos -= VectorOffset(calcOffset);
						break;
					case PositionOffset.TopRight:
							calcOffset.x = axisOffset * -1;
							calcOffset.y = axisOffset - nodeRadius ;
							pos -= VectorOffset(calcOffset);
					break;
					default:
					break;
				}

			return pos;
        }
	public Vector3 VectorOffset(Vector2 offset)
		{
			Vector3 offsetCalc = new Vector3
				(
					plane == Plane.XY || plane == Plane.ZX ? offset.x : 0,
					plane == Plane.XY || plane == Plane.ZY ? offset.y : 0,
					plane == Plane.ZX ? offset.y : plane == Plane.ZY ? offset.x : 0
				);
			return offsetCalc;
		}
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
	public int  ObstacleSearch(Vector3 worldPoint, float searchRadius, out bool walkable)
        {
			int movementPenalty = 0;
			Vector3 point = worldPoint + Vector3.up * 80;
			Ray ray = new Ray(point, Vector3.down);
			RaycastHit hit;
				
			//	if(Physics.SphereCast(ray,searchRadius,out hit,100,includeLayers))					
			if(Physics.BoxCast(point, Vector3.one * searchRadius, Vector3.down, out hit, Quaternion.identity, 100, includeLayers))
				{
					if(collectObjects == CollectObjects.Children)
						if(!hit.collider.transform.IsChildOf(transform))
							{
								walkable = true;
								return movementPenalty;
									
							}

					if(hit.collider.TryGetComponent<NaviObstacle>(out NaviObstacle navObstacle))
						{
							navObstacle.lastRegisterdSurface = this;
							movementPenalty = navObstacle.obstacleData.penalty;
								
							walkable = navObstacle.obstacleData.walkable;

							if (!walkable)
							return	movementPenalty += obstacleProximityPenalty;
							else
							return movementPenalty;
						}
				}
			walkable = true;
			return movementPenalty;
        }
	public void BlurPenaltyMap(int blurSize)
		{
			if (gridSizeX <= blurSize || gridSizeY <= blurSize)
				return;

			int kernelSize = blurSize * 2 + 1;
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
	public List<Node> GetNeighbours(Node node, int distance = 1,  bool includeDiagonal = true)
		{
			List<Node> neighbours = new List<Node>();

			for (int x = -distance; x <= distance; x++)
				{
					for (int y = -distance; y <= distance; y++)
						{
							if (x == 0 && y == 0)
								continue;

							if(nodeType == NodeType.Hexagon)
								if((y == distance && x == -distance) || (y == distance && x == distance))
								continue;

							if (!includeDiagonal)
								if ((x == -distance || x == distance) && (y == -distance || y == distance))
									continue;

							int checkX = node.gridX + x;
							int checkY = node.gridY + y;

							if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
								{
									//if(grid[checkX, checkY].walkable)
									neighbours.Add(grid[checkX, checkY]);
								}
						}
				}
			return neighbours;
		}
	public Node WorldToGridPos(Vector3 worldPos, bool convertToLocalSpace = true)
		{
			float axisOffset = (nodeRadius * 0.5f);

			Vector3 offsetCalc = new Vector3
				(
					plane == Plane.XY || plane == Plane.ZX ? worldPos.x - axisOffset : worldPos.x,
					plane == Plane.XY || plane == Plane.ZY ? worldPos.y - axisOffset : worldPos.y,
					plane == Plane.ZX || plane == Plane.ZY ? worldPos.z - axisOffset : worldPos.z
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

			public List<Node> ReturnListWithInDistance(int dist)
            {
				switch (dist)
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
	public void OnPathFound(CallBackData callBackData)
		{
			searchPass++;

		if (callBackData.pathSuccessful)
			{

				//Debug.Log("checkNeh  - " + callBackData.distanceLimit);
				path = new Navi_Path(callBackData.waypoints, transform.position, 0, 0);

			//	if (path.lookPoints.Length <= callBackData.distanceLimit)
			//		nodesWithInRange.Add(callBackData.targetNode);


				if(path.lookPoints.Length <= 1)
                {
					withInRangeCahce.nodes1Distance.Add(callBackData.targetNode);
                }
			
				if (path.lookPoints.Length <= 2)
				{
				
					withInRangeCahce.nodes2Distance.Add(callBackData.targetNode);
				}
			
				if (path.lookPoints.Length <= 3)
				{
					
					withInRangeCahce.nodes3Distance.Add(callBackData.targetNode);
				}
			
				if (path.lookPoints.Length <= 4)
				{
				
					withInRangeCahce.nodes4Distance.Add(callBackData.targetNode);
				}

			nodesWithInRange = withInRangeCahce.ReturnListWithInDistance(callBackData.distanceLimit);
			//else
			//	discardPass++;

			//Debug.Log(path.lookPoints.Length);
			}
	//	else
		//	discardPass++;

				//Debug.Log(searchPass + " =-= " + callBackData.dataSetCount  + " =-= " + discardPass);

				if( callBackData.dataSetCount == (discardPass +  searchPass))
					{
						setSearchComplete = true;
					}
				else
					{
						setSearchComplete = false;
					}
		}
	public void ShowViableNodes( RectTransform card, bool show)
        {
			if(show)
				{
					showViableNodes = true;
				}
			else
				{
					showViableNodes = false;
				}
        }
	public void CheckPath(Vector3 origin, Vector3 destination, bool canCrossDiagonal, int count, Node node, int distance)
		{
			RequestData requestData = new RequestData 
				{ 

					pathStart = origin, 
					pathEnd = destination, 
					includeDiagonal = canCrossDiagonal, 
					simplifyPath = false, 
					targetNode = node, 
					distanceLimit = distance,
					dataSetCount = count
			
				};

			if (requestData.targetNode.walkable == false)
				{
					discardPass++;
					return;
				}
			path = null;
			NaviSurface.RequestPath(requestData , OnPathFound);
		}
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
}
