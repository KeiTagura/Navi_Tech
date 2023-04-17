using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NaviSnap : MonoBehaviour
    {
        public NaviSurface surface;
        public bool snap = false;
        public Vector3 lastSnapPos;
        public UnityEvent snapEvent = new UnityEvent();
        
        ///Make it snap to the nearest viable surfacegrid point when out of bounds

        public Vector3 snapOffset;

        private void OnEnable()
            {
                //lastSnapPos = SnapToGrid.GetNearestGridPoint(transform.position, surface, out bool walkable);  

                lastSnapPos = transform.position;
            }
       public void Snap()
            {
                
                
                if (surface == null || lastSnapPos == transform.position)
                    return;


                Node node = surface.WorldToGridPos(transform.position);
                Vector3 pos = node.worldPosition;
                
                
                
                //Vector3 pos = SnapToGrid.GetNearestGridPoint(transform.position ,surface, out bool walkable);
                //Vector3 pos = surface.NodeFromWorldPoint(transform.position).worldPosition;
                //Debug.Log(walkable);
                //Debug.Log(node.gridX + "-" + node.gridY);

                //snapEvent.Invoke();
                if (node.walkable)
                            {
                        transform.position = pos + snapOffset;
                        lastSnapPos = pos + snapOffset;
                    }
                else
                    transform.position = lastSnapPos;
                    

            }


    }
