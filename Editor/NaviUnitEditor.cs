using System.Collections;
using System.Collections.Generic;
using System.Transactions;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NaviUnit))]
[CanEditMultipleObjects]

public class NaviUnitEditor : Editor
    {
   // SerializedProperty _navUnitDataRef;
   // SerializedProperty _navUnitDataSettings;
    //SerializedProperty _target;
   // public Transform target;

  //  public SO_NavUnitData navUnitData;

    void OnEnable()
        {
     //   _navUnitDataRef = serializedObject.FindProperty("navUnitDataRef");
      //  _navUnitDataSettings = serializedObject.FindProperty("naveUnitSettings");
      //  _target = serializedObject.FindProperty("target");
        }

    Vector3 tempTarget = Vector3.zero;
    public override void OnInspectorGUI()
        {
      //  DrawDefaultInspector();


        NaviUnit _targetScript = (NaviUnit)target;
       // EditorGUILayout.LabelField("Nav Unit");

      //  EditorGUILayout.Space(10);
       // _targetScript.target = EditorGUILayout.ObjectField(_targetScript.target, typeof(Transform),true) as Transform;

        SerializedProperty _target = serializedObject.FindProperty("target");
        EditorGUILayout.PropertyField(_target);

        EditorGUILayout.Space(10);
        //_targetScript.navUnitData = EditorGUILayout.ObjectField(_targetScript.navUnitData, typeof(SO_NavUnitData), true) as SO_NavUnitData;
        SerializedProperty navUnitData = serializedObject.FindProperty("navUnitData");
        EditorGUILayout.PropertyField(navUnitData);

        //_targetScript.stepForward = EditorGUILayout.Toggle("Step Forward", _targetScript.stepForward);
        SerializedProperty stepForward = serializedObject.FindProperty("stepForward");
        EditorGUILayout.PropertyField(stepForward);

        //_targetScript.autoUpdatePath = EditorGUILayout.Toggle("Auto Update Path", _targetScript.autoUpdatePath);
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

            // if (_navUnitDataRef != null)
            //      EditorGUILayout.PropertyField(_navUnitDataRef);
            // if(_navUnitDataRef != null)
            //  EditorGUILayout.PropertyField(_navUnitDataSettings);




            // EditorGUILayout.PropertyField(_target);
            serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
        // base.DrawDefaultInspector();
        }
    }
#endif