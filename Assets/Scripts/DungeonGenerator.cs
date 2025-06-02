using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public bool splitHorizontally;
    RectInt room = new RectInt(0, 0, 100, 50);

    void Start()
    { 
        
    }

    void Update()
    {
        AlgorithmsUtils.DebugRectInt(room, Color.red);
    }
}
