
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(NaviSurface))]
[CanEditMultipleObjects]
public class NaviSurfaceEditor : Editor
    {
        public override void OnInspectorGUI()
            {

                serializedObject.Update();

                NaviSurface _targetScript = (NaviSurface)target;

                if(_targetScript.naviSurfaceObserver == null)
                    _targetScript.naviSurfaceObserver = _targetScript.GetComponent<NaviSurfaceObserver>();

                if (_targetScript.naviSurfaceObserver == null) 
                    _targetScript.naviSurfaceObserver = _targetScript.gameObject.AddComponent<NaviSurfaceObserver>();

                _targetScript.naviSurfaceObserver.SetNaviSurface(_targetScript);

                EditorGUILayout.Space(10);

                GUIStyle gridPosition = new GUIStyle();
                gridPosition.normal.textColor = Color.white;
                gridPosition.fontStyle = FontStyle.Bold;


                EditorGUILayout.LabelField("Surface Position", gridPosition);

                SerializedProperty plane = serializedObject.FindProperty("plane");
                EditorGUILayout.PropertyField(plane);

                SerializedProperty planeOffset = serializedObject.FindProperty("positionOffset");
                EditorGUILayout.PropertyField(planeOffset);

                EditorGUILayout.Space(10);

                GUIStyle gridSize = new GUIStyle();

                gridSize.normal.textColor = Color.white;
                gridSize.fontStyle = FontStyle.Bold;

                EditorGUILayout.LabelField("Grid Cells", gridSize);

                SerializedProperty nodeType = serializedObject.FindProperty("nodeType");
                EditorGUILayout.PropertyField(nodeType);

                SerializedProperty surfaceGridSize = serializedObject.FindProperty("surfaceGridSize");
                EditorGUILayout.PropertyField(surfaceGridSize);

                SerializedProperty nodeRadius = serializedObject.FindProperty("nodeRadius");
                EditorGUILayout.PropertyField(nodeRadius);

                EditorGUILayout.Space(10);


                GUIStyle navObstacles = new GUIStyle();
                navObstacles.normal.textColor = Color.white;
                navObstacles.fontStyle = FontStyle.Bold;
                EditorGUILayout.LabelField("Nav Obastacles", navObstacles);
                EditorGUI.BeginChangeCheck();
                SerializedProperty includeLayers = serializedObject.FindProperty("includeLayers");
                EditorGUILayout.PropertyField(includeLayers);
                SerializedProperty collectObjects = serializedObject.FindProperty("collectObjects");
                EditorGUILayout.PropertyField(collectObjects);
		
                SerializedProperty obstacleProximityPenalty = serializedObject.FindProperty("obstacleProximityPenalty");
                EditorGUILayout.PropertyField(obstacleProximityPenalty);


                _targetScript.showDebug = EditorGUILayout.Foldout(_targetScript.showDebug, "Debug Viz");


                if(_targetScript.showDebug)
                    {

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


                if(_targetScript.OldNavSize != _targetScript.surfaceGridSize.magnitude)
                    _targetScript.CreateGrid();
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                _targetScript.OldNavSize = _targetScript.surfaceGridSize.magnitude;
                serializedObject.ApplyModifiedProperties();
            }
    }
#endif