<div id="header" align="center">
  <img src="https://github.com/KeiTagura/Navi_Tech/blob/main/Art/MonSli.gif" width="82"/>
  <h3 align="center">KeiTagura</h3>
</div>

<h1>
  Tech Navi
</h1>



<div align="center">
  <img src="https://github.com/KeiTagura/Navi_Tech/blob/main/Art/Navi.png" width="400" />
</div>

<p align="center">
  A turn and/or grid based AI Navigation system.
</p>


## ⚡ Quick setup

1. Add a `NavSurface` PreFab to the scene.
2. Add a `NavUnit` PreFab onto the NavSurface, and creat/add a Scritpable `SO_NaviUnitData` to it (Right Click > Create > Scriptables > NaviUnit Data).
3. Add `NaviObstacle` onto `NaviSurface`, and set create/add a Scriptable `SO_NaviObstacleData` to it (Right Click > Create > Scriptables > NaviObstacle Data).



## 🔧 Navi Components


<div id="header" align="center">
  <h3 align="center">Navi Surface</h3>
</div>



<div align="center">
  <img align="center" src="https://github.com/KeiTagura/Navi_Tech/blob/main/Inspector_NaviSurface.png" width="300" />

  <div align="center">

  |         Parameter          |                    Details                      |
  | :------------------------: | :---------------------------------------------: |
  |          `Plane`           |         Sets the axis plane for pathing         |
  |     `Position Offset`      |Sets the position offset for the grid as a hole  |
  |                            |                                                 |
  |        `NodeType`          |    Hexagon or Square grid layout/offset         |
  |    `Surface Grid Size`     |       Set the number of the grid cells          |
  |       `Node Radius`        |     Sets the size of the each grid cell         |
  |                            |                                                 |
  |      `Include Layers`      |Set what layers should be scanned for obstacles  |
  |    `Collection Objects`    |     Set local or global scan for obstacles      |
  |`Obstacle Proximity Penalty`|Set the pentaly cost for pathing near an obstacle|
  |                            |                                                 |
  |      `Display Grid`        |          Toggle debug grid visablity            |
  |  `Display Penalty Cost`    |         Toggle debug penalty visablity          |
  |  `Display Grid Position`   |      Toggle debug grid position visablity       |


  </div>
</div>

</br>
</br>
</br>
</br>
</br>

```csharp

///
/// WorldToGridPos returns nearest grid point
///
public Node WorldToGridPos(Vector3 worldPos, bool convertToLocalSpace = true);
```



```csharp
///
/// NaviSurface via code
///

NaviSurface nSurface;
GameObject obj;

void Start()
  {
      nSurface = GetComponent<NaviSurface>();
      
      Node nearestNode = nSurface.WorldToGridPos(object.transform.position);
      
      obj.transform.position = node.worldPosition;
  }
```



```csharp
///
/// Use this to (Re)Create/(Re)Calculate the NaviSurface Node space
/// Is called on inital Navsurface create/modification as well when obstales postions have moved.
///

public void CreateGrid()
```

</br>

***
<div id="header" align="center">
  <h3 align="center">Navi Unit</h3>
</div>



<div align="center">
  <img align="center" src="https://github.com/KeiTagura/Navi_Tech/blob/main/Inspector_NaviUnit.png" width="300" />

  <div align="center">

  |         Parameter          |                    Details                          |
  | :------------------------: | :-------------------------------------------------: |
  |          `Target`          |           Traget transform to move towards          |
  |       `Nav Unit Data`      |  Contains the setting for the Nave Units behaviour  |
  |        `Step Forward`      |  Is toggled if the unit should move this turn       |
  |      `Auto Update Path`    |Set if Navi Unit should auto update path continuously|
  |                            |                                                     |


  </div>
</div>

</br>
</br>
</br>
</br>



```csharp
//Manualy sets the new target postion via world position
 public void SetDestination(Vector3 val);
```

```csharp
//Manualy sets the new target by setting a target transform to follow
 public void SetTarget(Transform val);
```

```csharp
///
///Example for controlling the NaviUnit via code
///

NaviUnit nUnit;
Vector3 newPos;
Transform targetTransform;


void Start()
  {
      nUnit = GetComponent<NaviUnit>();

      if(!nUnit) return;

      nUnit.SetDestination(newPos);      
      nUnit.SetTarget(targetTransform);
  }


```

</br>

***
<div id="header" align="center">
  <h3 align="center">Navi Obstacle</h3>
</div>
