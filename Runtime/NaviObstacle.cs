using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, DisallowMultipleComponent]
public class NaviObstacle : MonoBehaviour
{
    public SO_NaviObstacleData obstacleData;

    Vector3 lastPos;
    float lastScale;
    [HideInInspector]
    public NaviSurface lastRegisterdSurface;
    NaviSurface[] navsurfaces;

    public Vector3 LastPos
        {
            set { lastPos = value; }
            get { return lastPos; }
        }
    public float LastCombiScale
        {
            set { lastScale = value; }
            get { return lastScale; }
        }
    public float GetCombiScale()
        { 
            return transform.localScale.magnitude + transform.lossyScale.magnitude;
        }
    private void OnEnable()
        {
            lastPos = transform.position;
            lastScale = transform.localScale.magnitude + transform.lossyScale.magnitude; 
            navsurfaces = FindObjectsOfType<NaviSurface>();
            for (int i = 0; i < navsurfaces.Length; i++)
                {
                    navsurfaces[i].AddToObserver(this);
                }
        }

    private void OnDisable()
        {
            if(navsurfaces == null)return;
            if(navsurfaces.Length <= 0) return;

            for (int i = 0; i < navsurfaces.Length; i++)
                {
                    navsurfaces[i].RemoveFromObserver(this);
                }
        }
    /*
    private void _Update()
        {
            if(lastRegisterdSurface)
            if(obstacleData.dynamicObstacle)
                {
                    float movedDist = Vector3.Distance(lastPos, transform.position);
                    if (movedDist > obstacleData.movementThreshold)
                        {
                            lastRegisterdSurface.CreateGrid();
                        }

                }
            lastPos = transform.position;
        }
    */
    }
