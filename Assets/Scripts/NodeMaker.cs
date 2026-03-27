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

    public Tilemap roadMap;
    public Tilemap pathMap;
    
    public int mapWidth;
    public int mapHeight;

    public int maxNPCs;
    
    
    private bool canDrawGizmos;

    private void Awake()
    {
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        grid = new Grid[mapWidth, mapHeight];

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = new Grid();
            }
        }
        
        CreateNodes();
    }

    void CreateNodes()
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                //if (grid[x, y] == grid.Floor)
                //{
                Node node = Instantiate(nodePrefab, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                nodeList.Add(node);
                //}
            }
        }
        
        CreateConnections();
    }

    void CreateConnections()
    {
        for (int i = 0; 1 < nodeList.Count; i++)
        {
            for (int j = i + 1; j < nodeList.Count; j++)
            {
                if (Vector2.Distance(nodeList[i].transform.position, nodeList[j].transform.position) <= 1.0f)
                {
                    ConnectNodes(nodeList[i], nodeList[j]);
                    ConnectNodes(nodeList[j], nodeList[i]);
                }
            }
        }
        canDrawGizmos = true;
        SpawnAI();
    }

    void ConnectNodes(Node from, Node to)
    {   
        if (from == to) { return; }
        
        from.connections.Add(to);
    }

    void SpawnAI()
    {
        Node randNode = nodeList[Random.Range(0, nodeList.Count)];
        
        NPCController newNPC = Instantiate(npc, randNode.transform.position, Quaternion.identity);
        
        newNPC.currentNode = randNode;
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
