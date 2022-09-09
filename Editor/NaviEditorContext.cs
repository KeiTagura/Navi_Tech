using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public static class NaviEditorContext
{
    [MenuItem("GameObject/Navi/NaviSurface", false, 11)]
    public static void CreateNaviSurface()
        {
        GameObject.Instantiate(Resources.Load("NaviSurface"), Vector3.zero, Quaternion.identity);

        }
   //[MenuItem("Navi/MyMenu/Do Something", false, 0)]
    [MenuItem("GameObject/Navi/NaviUnit", false, 10)]
    public static void CreateNavUnit()
        {

        }
    [MenuItem("GameObject/Navi/NaviObservable", false, 11)]
    public static void CreateNavObservable()
        {

        }
}
