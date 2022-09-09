using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NavUnitData", menuName = "Scriptables/NavUnit Data", order = 1)]
public class SO_NaviUnitData : ScriptableObject
    {
        [System.Serializable]
        public class Settings
            {
                public float speed = 20;
                public float turnSpeed = 3;
                public float turnDst = 5;
                public float stoppingDist = 10;
                public int stepLimit = 1;
                public float stepSpeed = 0.5f;
                public bool canCrossDiagonal = true;
                public bool simplifyPath = true;
            }

        public Settings settings = new Settings();
        
        
        
        
        
    }
