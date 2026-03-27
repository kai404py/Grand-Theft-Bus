using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class NodeMaker : MonoBehaviour
{
    public Grid[,] grid;
    public Node nodePrefab;
    public List<Node> nodeList;
    public NPCController npc;

    public Tilemap[] doNotSpawnTilemaps;
    
    public Vector2 bboxMin = new Vector2(-100f, -100f);
    public Vector2 bboxMax = new Vector2(100f, 100f);
    
    public float nodeDensity = 1f;

    public int maxNPCs;
    
    private bool canDrawGizmos;

    private void Awake()
    {
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        float spacing = 1f / Mathf.Max(nodeDensity, 0.01f);

        int cols = Mathf.RoundToInt((bboxMax.x - bboxMin.x) / spacing);
        int rows = Mathf.RoundToInt((bboxMax.y - bboxMin.y) / spacing);

        cols = Mathf.Max(cols, 1);
        rows = Mathf.Max(rows, 1);

        grid = new Grid[cols, rows];

        for (int i = 0; i < cols; i++)
        for (int j = 0; j < rows; j++)
            grid[i, j] = new Grid();

        CreateNodes(spacing);
    }

    void CreateNodes(float spacing)
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                float worldX = bboxMin.x + x * spacing + spacing * 0.5f;
                float worldY = bboxMin.y + y * spacing + spacing * 0.5f;
                Vector2 worldPos = new Vector2(worldX - 0.5f, worldY - 0.5f);

                if (IsBlockedTile(worldPos)) continue;

                Node node = Instantiate(nodePrefab, worldPos, Quaternion.identity);
                nodeList.Add(node);
            }
        }

        CreateConnections(spacing);
    }

    bool IsBlockedTile(Vector2 worldPos)
    {
        foreach (Tilemap tilemap in doNotSpawnTilemaps)
        {
            if (tilemap == null) continue;
            
            Vector3Int center = tilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
            int radius = 1;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    Vector3Int checkCell = new Vector3Int(center.x + dx, center.y + dy, center.z);
                    if (tilemap.HasTile(checkCell)) return true;
                }
            }
        }
        return false;
    }

    void CreateConnections(float spacing)
    {
        float connectionDistance = spacing * 1.5f;

        for (int i = 0; i < nodeList.Count; i++)
        {
            for (int j = i + 1; j < nodeList.Count; j++)
            {
                if (Vector2.Distance(nodeList[i].transform.position, nodeList[j].transform.position) <= connectionDistance)
                {
                    if (!IsPathBlocked(nodeList[i].transform.position, nodeList[j].transform.position))
                    {
                        ConnectNodes(nodeList[i], nodeList[j]);
                        ConnectNodes(nodeList[j], nodeList[i]);
                    }
                }
            }
        }

        canDrawGizmos = true;
        SpawnAI();
    }

    bool IsPathBlocked(Vector2 from, Vector2 to)
    {
        int steps = 5;
        for (int s = 1; s < steps; s++)
        {
            Vector2 samplePoint = Vector2.Lerp(from, to, (float)s / steps);
            if (IsBlockedTile(samplePoint)) return true;
        }
        return false;
    }

    void ConnectNodes(Node from, Node to)
    {   
        if (from == to) { return; }
        
        from.connections.Add(to);
    }

    void SpawnAI()
    {
        for (int i = 0; i < maxNPCs; i++)
        {
            Node randNode = nodeList[Random.Range(0, nodeList.Count)];

            NPCController newNPC = Instantiate(npc, randNode.transform.position, Quaternion.identity);

            newNPC.currentNode = randNode;
        }
    }

    private void OnDrawGizmos()
    {
        if (canDrawGizmos)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < nodeList.Count; i++)
            {
                for (int j = 0; j < nodeList[i].connections.Count; j++)
                {
                    Gizmos.DrawLine(nodeList[i].transform.position, nodeList[i].transform.position);
                }
            }
        }
    }
}
