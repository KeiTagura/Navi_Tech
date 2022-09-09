using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaviSnap : MonoBehaviour
    {
        public NaviSurface surface;
        public bool snap = false;
        public Vector3 lastSnapPos;
    
        private void OnEnable()
            {
              //  lastSnapPos = SnapToGrid.GetNearestGridPoint(transform.position, surface, out bool walkable);  
            }
       public void Snap()
            {

                if (surface == null)
                    return;
                Node node = surface.WorldToGridPos(transform.position);
                Vector3 pos = node.worldPosition;

                //Vector3 pos = SnapToGrid.GetNearestGridPoint(transform.position ,surface, out bool walkable);
                //Vector3 pos = surface.NodeFromWorldPoint(transform.position).worldPosition;
                //Debug.Log(walkable);

                if (node.walkable)
                    {
                        transform.position = pos;
                        lastSnapPos = pos;
                    }
                else
                    transform.position = lastSnapPos;
                    

            }


    }
