using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using log4net.Util;

public static class NaviEditorContext
{

    [MenuItem("GameObject/Navi/NaviSurface", false, 11)]
    public static void CreateNaviSurface()
        {
            GameObject goSurf = new GameObject("NaviSurface");
            GameObject goVizPivot = new GameObject("Pivot_VizSurf");
            GameObject goViz = GameObject.CreatePrimitive(PrimitiveType.Cube);

            
            goViz.transform.parent = goVizPivot.transform;
            goVizPivot.transform.parent = goSurf.transform;
            goViz.transform.position = new Vector3(0f, 0.5f, 0f);
            goVizPivot.transform.localScale = new Vector3(40f, 0.001f, 40f);

            goViz.hideFlags = HideFlags.HideInHierarchy;
            goVizPivot.hideFlags = HideFlags.HideInHierarchy;

            NaviSurface nSurf = goSurf.AddComponent<NaviSurface>();

            nSurf.groundVizPivot = goVizPivot;
            nSurf.plane = Plane.ZX;
            nSurf.positionOffset = PositionOffset.TopRight;
            nSurf.nodeType = NodeType.Square;
            nSurf.surfaceGridSize = new Vector2(20,20);
            nSurf.nodeRadius = 2;
            nSurf.includeLayers = -1;
            nSurf.collectObjects = CollectObjects.All;
            nSurf.obstacleProximityPenalty = 10;

            Selection.objects = new Object[] { goSurf };

        }
   //[MenuItem("Navi/MyMenu/Do Something", false, 0)]
    [MenuItem("GameObject/Navi/NaviUnit", false, 10)]
    public static void CreateNavUnit()
        {
            GameObject naviUnit = new GameObject("NaviUnit");
            GameObject naviUnitViz = GameObject.CreatePrimitive(PrimitiveType.Cube);
            naviUnitViz.name = "NaviUnitViz";
            NaviUnit nUnit= naviUnit.AddComponent<NaviUnit>();
            nUnit.autoUpdatePath = true;
            nUnit.stepForward = true;

            
            nUnit.transform.position = Vector3.zero;

            naviUnitViz.transform.parent = naviUnit.transform;
            naviUnitViz.transform.position = new Vector3(0,1,0);

            naviUnitViz.hideFlags = HideFlags.HideInHierarchy;
        }
    [MenuItem("GameObject/Navi/NaviObservable", false, 11)]
    public static void CreateNavObservable()
        {
            GameObject naviObservable = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            naviObservable.name = "NaviObservable";
            naviObservable.transform.position = new Vector3(8,0,8);
            naviObservable.transform.localScale = new Vector3(4,1,4);
            
            naviObservable.AddComponent<NaviObstacle>();
            
        }
    }

#endif