using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;

[CustomEditor(typeof(NaviSnap))]
[CanEditMultipleObjects]

public class NaviSnapEditor : Editor
{
    public override void OnInspectorGUI()
        {
        
            NaviSnap _targetScript = (NaviSnap)target;
            
            SerializedProperty _surface = serializedObject.FindProperty("surface");
            EditorGUILayout.PropertyField(_surface);

            serializedObject.ApplyModifiedProperties();
            
            _targetScript.snap = EditorGUILayout.Toggle("Continuous Snapping", _targetScript.snap);
            _targetScript.snapOffset = EditorGUILayout.Vector3Field("Snap Offset", _targetScript.snapOffset);

            SerializedProperty snapEvent = serializedObject.FindProperty("snapEvent");            
            SerializedProperty snapOffset = serializedObject.FindProperty("snapOffset"); 
            EditorGUILayout.PropertyField(snapEvent);
 
             if(GUI.changed)
                 {
                     serializedObject.ApplyModifiedProperties();
                 }
            
            if (_targetScript.snap)
                {
                    _targetScript.Snap();
                    
                    _targetScript.snapEvent.Invoke();
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