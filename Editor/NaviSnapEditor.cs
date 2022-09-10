using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

[CustomEditor(typeof(NaviSnap))]
[CanEditMultipleObjects]

public class NaviSnapEditor : Editor
{

    public override void OnInspectorGUI()
        {

            NaviSnap _targetScript = (NaviSnap)target;
            
            

            SerializedProperty _surface = serializedObject.FindProperty("surface");
            EditorGUILayout.PropertyField(_surface);

           // SerializedProperty _lastSnapPos = serializedObject.FindProperty("lastSnapPos");
           // EditorGUILayout.PropertyField(_lastSnapPos);
            serializedObject.ApplyModifiedProperties();
            
            _targetScript.snap = EditorGUILayout.Toggle("Continuous Snapping", _targetScript.snap);

            if(_targetScript.snap)
                {
                    _targetScript.Snap();
                }

            EditorGUILayout.Space(20);


            if(GUILayout.Button("Snap"))
                {
                    _targetScript.Snap();
                }
            EditorGUILayout.Space(20);

        }
    }

#endif