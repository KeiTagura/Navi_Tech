<div id="header" align="center">
  <img src="https://github.com/KeiTagura/Navi_Tech/blob/main/Art/MonSli.gif" width="82"/>
  <h3 align="center">KeiTagura</h3>
</div>

<h1>
  Tech Navi
  <img src="https://media.giphy.com/media/hvRJCLFzcasrR4ia7z/giphy.gif" width="30px"/>
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

<div align="center">
  <img src="https://github.com/KeiTagura/Navi_Tech/blob/main/Inspector_NaviSurface.png" width="300" />
</div>


<br/>


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





