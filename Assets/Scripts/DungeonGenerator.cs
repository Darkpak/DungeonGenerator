using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private RectInt dungeonBounds;
    [SerializeField] private int minRoomWidth = 10;
    [SerializeField] private int minRoomHeight = 10;
    [SerializeField] private float splitDelay = 0.2f;

    private List<RectInt> rooms = new List<RectInt>();
    private List<Vector2Int> doors = new List<Vector2Int>();
    private List<(int, int)> connections = new List<(int, int)>();
    
    private HashSet<int> bspVisited = new HashSet<int>();

    private void Start()
    {
        StartCoroutine(GenerateDungeonCoroutine());
    }

    private IEnumerator GenerateDungeonCoroutine()
    {
        // Validate the dungeon bounds
        rooms = new List<RectInt> { dungeonBounds };
        Queue<RectInt> toSplit = new Queue<RectInt>(rooms);

        // Split rooms until all rooms are at least min size
        while (toSplit.Count > 0)
        {
            var room = toSplit.Dequeue();
            bool canSplitHorizontally = room.height >= minRoomHeight * 2;
            bool canSplitVertically = room.width >= minRoomWidth * 2;

            if (!canSplitHorizontally && !canSplitVertically)
                continue;

            // If both splits are possible, choose randomly
            bool splitHorizontally = canSplitHorizontally && (!canSplitVertically || RandomBool());
            SplitRectIntRandom(room, splitHorizontally, out RectInt a, out RectInt b);

            // Make sure the new rooms are valid
            rooms.Remove(room);
            rooms.Add(a);
            rooms.Add(b);
            toSplit.Enqueue(a);
            toSplit.Enqueue(b);

            yield return new WaitForSeconds(splitDelay);
        }

        // Build adjacency graph and place doors
        BuildRoomGraphAndDoors();
        
        // Check connectivity
        yield return StartCoroutine(CheckConnectivityCoroutine());

    }

    private void BuildRoomGraphAndDoors()
    {
        connections.Clear();
        doors.Clear();

        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                if (AreRoomsAdjacent(rooms[i], rooms[j], out Vector2Int doorPos))
                {
                    connections.Add((i, j));
                    doors.Add(doorPos);
                }
            }
        }
    }

    // BSP to check if all rooms are reachable
    private IEnumerator CheckConnectivityCoroutine()
    {
        bspVisited.Clear();
        if (rooms.Count == 0) yield break;

        var queue = new Queue<int>();
        queue.Enqueue(0);
        bspVisited.Add(0);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            // Highlight the current room that is being visited
            AlgorithmsUtils.DebugRectInt(rooms[current], Color.red, splitDelay);

            foreach (var (a, b) in connections)
            {
                int neighbour = -1;
                if (a == current && !bspVisited.Contains(b)) neighbour = b;
                if (b == current && !bspVisited.Contains(a)) neighbour = a;

                if (neighbour != -1)
                {
                    bspVisited.Add(neighbour);
                    queue.Enqueue(neighbour);

                    // Visualize the connection that is being explored
                    var doorPos = doors[connections.IndexOf((a, b))];
                    Debug.DrawLine(
                        new Vector3(doorPos.x, 0.1f, doorPos.y),
                        new Vector3(doorPos.x, 0.1f, doorPos.y) + Vector3.up * 3,
                        Color.yellow,
                        0.1f
                    );

                    yield return new WaitForSeconds(splitDelay);
                }
            }
        }

        // Final check if all rooms were visited
        Debug.Log(bspVisited.Count == rooms.Count ? "Dungeon is fully connected!" : "Dungeon is not fully connected!");
    }

    // Returns true and outputs a random door position if rooms share a wall
    private bool AreRoomsAdjacent(RectInt a, RectInt b, out Vector2Int doorPos)
    {
        doorPos = Vector2Int.zero;
        // Check vertical adjacency (left/right)
        if (a.xMax == b.xMin || b.xMax == a.xMin)
        {
            int yMin = Mathf.Max(a.yMin, b.yMin);
            int yMax = Mathf.Min(a.yMax, b.yMax) -1;
            
            if (yMin <= yMax)
            {
                // Randomly choose a y position within the valid range
                int y = Random.Range(yMin, yMax + 1);
                int x = a.xMax == b.xMin ? a.xMax : b.xMax;
                doorPos = new Vector2Int(x, y);
                return true;
            }
        }
        
        // Check horizontal adjacency (top/bottom)
        if (a.yMax == b.yMin || b.yMax == a.yMin)
        {
            // Ensure the yMin and yMax are within the bounds of both rooms
            int xMin = Mathf.Max(a.xMin, b.xMin);
            int xMax = Mathf.Min(a.xMax, b.xMax) - 1;
            
            if (xMin <= xMax)
            {
                // Randomly choose an x position within the valid range
                int x = Random.Range(xMin, xMax + 1);
                int y = a.yMax == b.yMin ? a.yMax : b.yMax;
                doorPos = new Vector2Int(x, y);
                return true;
            }
        }
        return false;
    }
    
    private void Update()
    {
        // Draw rooms
        for (int i = 0; i < rooms.Count; i++)
        {
            // If visited in BSP, draw red, else blue
            var color = bspVisited.Contains(i) ? Color.red : Color.blue;
            AlgorithmsUtils.DebugRectInt(rooms[i], color);
        }

        // Draw doors
        foreach (var door in doors)
        {
            Debug.DrawLine(
                new Vector3(door.x, 0.1f, door.y),
                new Vector3(door.x, 0.1f, door.y) + Vector3.up * 2,
                Color.cyan
            );
        }
    }
    
    private void SplitRectIntRandom(RectInt rect, bool splitHorizontally, out RectInt roomA, out RectInt roomB)
    {
        // Ensure the split respects minimum room dimensions
        if (splitHorizontally)
        {
            int minY = rect.y + minRoomHeight;
            int maxY = rect.yMax - minRoomHeight;
            int splitY = Random.Range(minY, maxY);
            roomA = new RectInt(rect.x, rect.y, rect.width, splitY - rect.y);
            roomB = new RectInt(rect.x, splitY, rect.width, rect.yMax - splitY);
        }
        else
        {
            int minX = rect.x + minRoomWidth;
            int maxX = rect.xMax - minRoomWidth;
            int splitX = Random.Range(minX, maxX);
            roomA = new RectInt(rect.x, rect.y, splitX - rect.x, rect.height);
            roomB = new RectInt(splitX, rect.y, rect.xMax - splitX, rect.height);
        }
    }

    private bool RandomBool()
    {
        return Random.value > 0.5f;
    }
}