using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NavObstacleData", menuName = "Navi/SO/NavObstacle Data", order = 1)]
public class SO_NaviObstacleData : ScriptableObject
    {
        public bool walkable = true;
        public int penalty;
        [Range (1,5)]
        public int penaltySmoothing = 3;
        public bool dynamicObstacle = false;
        public float movementThreshold = 0.3f;
        int prevVal;
        private void OnValidate()
            {

                if (penaltySmoothing % 2 == 0)
                    {  
                        penaltySmoothing = prevVal;
                    }
                else
                    {
                        prevVal = penaltySmoothing;
                    }
            }
    }
