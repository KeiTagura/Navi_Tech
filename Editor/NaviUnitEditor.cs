
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NaviUnit))]
[CanEditMultipleObjects]

public class NaviUnitEditor : Editor
    {
        Vector3 tempTarget = Vector3.zero;
        public override void OnInspectorGUI()
            {
                NaviUnit _targetScript = (NaviUnit)target;
                SerializedProperty _target = serializedObject.FindProperty("target");
                EditorGUILayout.PropertyField(_target);

                EditorGUILayout.Space(10);
                SerializedProperty navUnitData = serializedObject.FindProperty("navUnitData");
                EditorGUILayout.PropertyField(navUnitData);

                SerializedProperty stepForward = serializedObject.FindProperty("stepForward");
                EditorGUILayout.PropertyField(stepForward);

                SerializedProperty autoUpdatePath = serializedObject.FindProperty("autoUpdatePath");
                EditorGUILayout.PropertyField(autoUpdatePath);

                EditorGUILayout.Space(10);

                if (GUILayout.Button("UpdatePath"))
                    {
                        if(_targetScript.target)
                            _targetScript.SetTarget(_targetScript.target);
                        else
                            _targetScript.SetDestination(tempTarget);

                        _targetScript.stepForward = true;
                    }
        
                EditorGUILayout.Space(20);

                    serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
    }
#endif