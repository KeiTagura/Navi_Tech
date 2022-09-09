using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(NaviSurface))]
[CanEditMultipleObjects]
public class NaviSurfaceEditor : Editor
{
    //public float oldVal = 0;
    public override void OnInspectorGUI()
        {
          DrawDefaultInspector();


        //EditorGUILayout.BeginHorizontal();
       // try
        //    {
       //     SectionName = EditorGUILayout.TextField(SectionName, SectionNameStyle);
        //    }
        //finally
        //    {
         ///   EditorGUILayout.EndHorizontal();
         //   }

        serializedObject.Update();



        NaviSurface _targetScript = (NaviSurface)target;

        if(_targetScript.naviSurfaceObserver == null)
        _targetScript.naviSurfaceObserver = _targetScript.GetComponent<NaviSurfaceObserver>();

        if (_targetScript.naviSurfaceObserver == null) _targetScript.naviSurfaceObserver = _targetScript.gameObject.AddComponent<NaviSurfaceObserver>();

        _targetScript.naviSurfaceObserver.SetNaviSurface(_targetScript);
        //EditorGUILayout.LabelField("Nav Surface");

        //_targetScript.displayGridGizmos = EditorGUILayout.Toggle("Display Gizmos", _targetScript.displayGridGizmos);
        
        
        //SerializedProperty displayGridGizmos = serializedObject.FindProperty("displayGridGizmos");
      //  EditorGUILayout.PropertyField(displayGridGizmos);

        EditorGUILayout.Space(10);

        GUIStyle gridPosition = new GUIStyle();
        //SectionNameStyle.fontSize = 35;
        gridPosition.normal.textColor = Color.white;
        gridPosition.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Surface Position", gridPosition);

        // _targetScript.plane = (Plane)EditorGUILayout.EnumPopup("Plane Orientation", _targetScript.plane);
        SerializedProperty plane = serializedObject.FindProperty("plane");
        EditorGUILayout.PropertyField(plane);
        // _targetScript.positionOffset = (PositionOffset)EditorGUILayout.EnumPopup("Plane Offset", _targetScript.positionOffset);

        SerializedProperty planeOffset = serializedObject.FindProperty("positionOffset");
        EditorGUILayout.PropertyField(planeOffset);



        EditorGUILayout.Space(10);

        GUIStyle gridSize = new GUIStyle();
        //SectionNameStyle.fontSize = 35;
        gridSize.normal.textColor = Color.white;
        gridSize.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Grid Cells", gridSize);


        SerializedProperty nodeType = serializedObject.FindProperty("nodeType");
        EditorGUILayout.PropertyField(nodeType);

        //  _targetScript.surfaceGridSize = EditorGUILayout.Vector2Field("Surface Grid Sizse", _targetScript.surfaceGridSize);
        SerializedProperty surfaceGridSize = serializedObject.FindProperty("surfaceGridSize");
        EditorGUILayout.PropertyField(surfaceGridSize);

        // _targetScript.nodeRadius = EditorGUILayout.FloatField("Node Radius", _targetScript.nodeRadius);
        SerializedProperty nodeRadius = serializedObject.FindProperty("nodeRadius");
        EditorGUILayout.PropertyField(nodeRadius);

        EditorGUILayout.Space(10);


        GUIStyle navObstacles = new GUIStyle();
        //SectionNameStyle.fontSize = 35;
        navObstacles.normal.textColor = Color.white;
        navObstacles.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Nav Obastacles", navObstacles);
        EditorGUI.BeginChangeCheck();
        //var layersSelection = EditorGUILayout.MaskField("Include Layers", ExtensionTools.LayerMaskToField(_targetScript.includeLayers), UnityEditorInternal.InternalEditorUtility.layers);
        SerializedProperty includeLayers = serializedObject.FindProperty("includeLayers");
        EditorGUILayout.PropertyField(includeLayers);
        /*

        if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_targetScript, "Layers changed");
                _targetScript.includeLayers = ExtensionTools.FieldToLayerMask(layersSelection);
            }
        */
        //_targetScript.collectObjects = (CollectObjects)EditorGUILayout.EnumPopup("Collect Objects", _targetScript.collectObjects);
        SerializedProperty collectObjects = serializedObject.FindProperty("collectObjects");
        EditorGUILayout.PropertyField(collectObjects);




        //_targetScript.obstacleProximityPenalty = EditorGUILayout.IntField("Obstascle Proximity Penalty", _targetScript.obstacleProximityPenalty);
        SerializedProperty obstacleProximityPenalty = serializedObject.FindProperty("obstacleProximityPenalty");
        EditorGUILayout.PropertyField(obstacleProximityPenalty);


        _targetScript.showDebug = EditorGUILayout.Foldout(_targetScript.showDebug, "Debug Viz");


        if(_targetScript.showDebug)
            {
                //_targetScript.displayGrid = EditorGUILayout.Toggle("Display Grid", _targetScript.displayGrid); //, "Display Grid"
                //_targetScript.displayPenaltyCost = EditorGUILayout.Toggle("Display Penalty Cost", _targetScript.displayPenaltyCost); //, "Display Penalty Cost"
                //_targetScript.displayGridPosition = EditorGUILayout.Toggle("Display Gird Position", _targetScript.displayGridPosition); //"Display Gird Position"

               if( GUILayout.Button("Display Grid"))
                    {
                        _targetScript.displayGrid = !_targetScript.displayGrid;
			            SceneView.RepaintAll();     
                    }
               if( GUILayout.Button("Display Penalty Cost"))
                    {
                        _targetScript.displayPenaltyCost = !_targetScript.displayPenaltyCost;
			            SceneView.RepaintAll();     
                    }
               if( GUILayout.Button("Display Gird Position"))
                    {
                        _targetScript.displayGridPosition = !_targetScript.displayGridPosition;
			            SceneView.RepaintAll();     
                    }
            }


        EditorGUILayout.Space(40);
        if (GUILayout.Button("Calculate NavSurface"))
            {
                _targetScript.CreateGrid();
            }
        EditorGUILayout.Space(20);

        //Debug.Log(oldVal + "  - " + _targetScript.surfaceGridSize.magnitude);

        if(_targetScript.OldNavSize != _targetScript.surfaceGridSize.magnitude)
            _targetScript.CreateGrid();
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();

        _targetScript.OldNavSize = _targetScript.surfaceGridSize.magnitude;
        serializedObject.ApplyModifiedProperties();
        }
    }
