using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

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

            EditorGUILayout.Space(20);
            if(GUILayout.Button("Snap"))
                {
                    _targetScript.Snap();
                }

        }
    }

#endif