using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent, ExecuteInEditMode]
public class NaviSurfaceObserver : MonoBehaviour
{
    List<NaviObstacle> naviObstacles = new List<NaviObstacle>();
    NaviSurface naviSurface;

    public void SetNaviSurface(NaviSurface _naviSurface)
        {
            naviSurface = _naviSurface;
        }

    public void AddToSurfaceObstacles(NaviObstacle naviObstacle)
        {
            if (naviObstacles.Contains(naviObstacle)) return;

            InitObstacleObservation();
            naviObstacles.Add(naviObstacle);
        }
    public void RemoveFromSurfaceObstacles(NaviObstacle naviObstacle)
        {
            if (!naviObstacles.Contains(naviObstacle)) return;

            naviObstacles.Remove(naviObstacle);
        }

    public void InitObstacleObservation()
        {

            for (int i = 0; i < naviObstacles.Count; i++)
                {
                    if (naviObstacles[i].obstacleData != null)
                    if (!naviObstacles[i].LastPos.Equals(naviObstacles[i].transform.position))
                        {
                            naviSurface.CreateGrid();
                        }
                }
        }

    void Update()
        {
            for (int i = 0; i < naviObstacles.Count; i++)
                {
                    
                    //Debug.Log("00");
                    if (naviObstacles[i].obstacleData != null)
                    if (naviObstacles[i].obstacleData.dynamicObstacle)
                        {
                              //  Debug.Log("01");

                            float movedDist = Mathf.Abs(Vector3.Distance(naviObstacles[i].LastPos, naviObstacles[i].transform.position));
                            float scaleDelta = Mathf.Abs(naviObstacles[i].LastCombiScale - naviObstacles[i].GetCombiScale());
                                
                           // Debug.Log(movedDist);
                            if(movedDist > 0 || scaleDelta > 0)
                           //// if(movedDist > naviObstacles[i].obstacleData.movementThreshold)
                            //if(!naviObstacles[i].LastPos().Equals(naviObstacles[i].transform.position))
                                {
                                   // Debug.Log("02");
                                    naviSurface.CreateGrid();
                                    naviObstacles[i].LastPos = naviObstacles[i].transform.position;
                                    naviObstacles[i].LastCombiScale = naviObstacles[i].GetCombiScale();
                                }


                        }
                }
        }
}
