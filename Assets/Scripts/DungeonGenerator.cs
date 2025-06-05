using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Size")]
    [SerializeField] private RectInt dungeonArea;
    
    [Header("Room Settings")]
    [SerializeField] private List<RectInt> rooms = new List<RectInt>();
    public int minRoomSize = 3;
    

    void Start()
    { 
        //StartCoroutine(GenerateDungeon());
        GenerateDungeon();
    }

   
    [Button]
    //public IEnumerator GenerateDungeon()
    private void GenerateDungeon()
    {
        (RectInt roomA, RectInt roomB) = SplitHorizontally(dungeonArea);
        rooms.Add(roomA);
        rooms.Add(roomB);
        DebugDrawingBatcher.BatchCall( () =>
        {
            AlgorithmsUtils.DebugRectInt(roomA, Color.red);
            AlgorithmsUtils.DebugRectInt(roomB, Color.green);
        });
        //yield return new WaitForEndOfFrame();
    }

    private (RectInt, RectInt) SplitHorizontally(RectInt splitRoom)
    {
        RectInt roomA = splitRoom;
        RectInt roomB = splitRoom;
        roomA.height = (splitRoom.height / 2) + Random.Range(0, splitRoom.height);
        roomB.height -= (roomA.height - 1);
        roomB.y += roomA.height - 1;
        return (roomA, roomB);
    }
}
