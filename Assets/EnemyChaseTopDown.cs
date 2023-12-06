using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyChaseTopDown : MonoBehaviour
{
    public Tilemap tilemap;
    public Transform player;
    public float moveSpeed = 2f;

    private List<Vector3Int> path;

    void Update()
    {
        if (player != null)
        {
            // Find the path using Dijkstra's algorithm
            path = DijkstraPath(transform.position, player.position);

            // Move the enemy along the path
            MoveAlongPath();
        }
    }

    List<Vector3Int> DijkstraPath(Vector3 start, Vector3 end)
    {
        List<Vector3Int> finalPath = new List<Vector3Int>();
        Vector3Int startCell = tilemap.WorldToCell(start);
        Vector3Int endCell = tilemap.WorldToCell(end);

        Dictionary<Vector3Int, float> distance = new Dictionary<Vector3Int, float>();
        Dictionary<Vector3Int, Vector3Int> previous = new Dictionary<Vector3Int, Vector3Int>();
        PriorityQueue<Vector3Int> priorityQueue = new PriorityQueue<Vector3Int>();

        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(position);
            if (tile != null) // Assuming null tiles are obstacles
            {
                distance[position] = float.MaxValue;
                previous[position] = Vector3Int.zero; // Set a default previous position
                priorityQueue.Enqueue(position, float.MaxValue);
            }
        }

        distance[startCell] = 0;
        priorityQueue.Enqueue(startCell, 0);

        while (priorityQueue.Count > 0)
        {
            Vector3Int current = priorityQueue.Dequeue();

            foreach (Vector3Int neighbor in GetNeighbors(current))
            {
                float alt = distance[current] + 1; // Assuming each step has a cost of 1
                if (alt < distance[neighbor])
                {
                    distance[neighbor] = alt;
                    previous[neighbor] = current;
                    priorityQueue.Enqueue(neighbor, alt);
                }
            }
        }

        // Reconstruct the path
        Vector3Int currentCell = endCell;
        while (currentCell != Vector3Int.zero)
        {
            finalPath.Add(currentCell);
            currentCell = previous[currentCell];
        }

        finalPath.Reverse();
        return finalPath;
    }

    List<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>();

        Vector3Int[] directions =
        {
            new Vector3Int(1, 0, 0),  // Right
            new Vector3Int(-1, 0, 0), // Left
            new Vector3Int(0, 1, 0),  // Up
            new Vector3Int(0, -1, 0)  // Down
        };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighbor = cell + dir;
            if (tilemap.GetTile(neighbor) != null) // Assuming null tiles are obstacles
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    void MoveAlongPath()
    {
        if (path != null && path.Count > 0)
        {
            Vector3 targetPosition = tilemap.GetCellCenterWorld(path[0]);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                path.RemoveAt(0);
            }
        }
    }

    // Priority queue implementation for Dijkstra's algorithm
    public class PriorityQueue<T>
    {
        private List<KeyValuePair<T, float>> elements = new List<KeyValuePair<T, float>>();

        public int Count { get { return elements.Count; } }

        public void Enqueue(T item, float priority)
        {
            elements.Add(new KeyValuePair<T, float>(item, priority));
        }

        public T Dequeue()
        {
            int bestIndex = 0;

            for (int i = 1; i < elements.Count; i++)
            {
                if (elements[i].Value < elements[bestIndex].Value)
                {
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].Key;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }
}