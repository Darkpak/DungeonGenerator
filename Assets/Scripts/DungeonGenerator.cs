using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private RectInt dungeonBounds;
    private RectInt roomA, roomB;
    private RectInt roomA1, roomA2;
    private RectInt roomB1, roomB2;
    public bool splitHorizontally = false;

    private void Start()
    {
        SplitRectInt(dungeonBounds, out roomA, out roomB, splitHorizontally);

        // Split roomA with the same direction
        SplitRectInt(roomA, out roomA1, out roomA2, splitHorizontally);

        // Split roomB with the opposite direction
        SplitRectInt(roomB, out roomB1, out roomB2, !splitHorizontally);
    }

    private void Update()
    {
        AlgorithmsUtils.DebugRectInt(roomA1, Color.green);
        AlgorithmsUtils.DebugRectInt(roomA2, Color.cyan);
        AlgorithmsUtils.DebugRectInt(roomB1, Color.magenta);
        AlgorithmsUtils.DebugRectInt(roomB2, Color.red);
    }

    public void SplitRectInt(RectInt rect, out RectInt roomA, out RectInt roomB, bool splitHorizontally)
    {
        if (splitHorizontally)
        {
            int splitY = rect.y + rect.height / 2;
            roomA = new RectInt(rect.x, rect.y, rect.width, splitY - rect.y);
            roomB = new RectInt(rect.x, splitY, rect.width, rect.yMax - splitY);
        }
        else
        {
            int splitX = rect.x + rect.width / 2;
            roomA = new RectInt(rect.x, rect.y, splitX - rect.x, rect.height);
            roomB = new RectInt(splitX, rect.y, rect.xMax - splitX, rect.height);
        }
    }
}