using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class NodeMaker : MonoBehaviour
{
    [Header("References")]
    public Node nodePrefab;
    public NPCController npc;
    public Tilemap[] doNotSpawnTilemaps;
    public Tilemap[] pathTilemaps;
    public Transform player;

    [Header("Node Settings")]
    public float spawnRadius = 30f;
    public float despawnRadius = 35f;

    [Header("Update Settings")]
    public float updateInterval = 0.5f;

    [Header("AI Settings")]
    public int maxNPCs = 5;
    public float npcDespawnRadius = 40f;

    private Dictionary<Vector3Int, Node> activeNodes = new Dictionary<Vector3Int, Node>();
    private bool canDrawGizmos = false;

    public List<Node> nodeList = new List<Node>();
    private List<NPCController> activeNPCs = new List<NPCController>();

    private void Awake()
    {
        if (player == null)
        {
            var bus = FindObjectOfType<BusController>();
            if (bus != null) player = bus.transform;
        }

        StartCoroutine(DynamicNodeUpdate());
    }

    private IEnumerator DynamicNodeUpdate()
    {
        while (true)
        {
            if (player != null)
            {
                DespawnDistantNodes();
                SpawnNearbyNodes();
                RebuildConnections();

                DespawnDistantNPCs();
                TopUpNPCs();

                canDrawGizmos = true;
            }

            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void SpawnNearbyNodes()
    {
        if (pathTilemaps == null || pathTilemaps.Length == 0) return;

        Vector2 playerPos = player.position;

        foreach (Tilemap tilemap in pathTilemaps)
        {
            if (tilemap == null) continue;

            Vector3Int minCell = tilemap.WorldToCell(new Vector3(playerPos.x - spawnRadius, playerPos.y - spawnRadius, 0));
            Vector3Int maxCell = tilemap.WorldToCell(new Vector3(playerPos.x + spawnRadius, playerPos.y + spawnRadius, 0));

            for (int cx = minCell.x; cx <= maxCell.x; cx++)
            {
                for (int cy = minCell.y; cy <= maxCell.y; cy++)
                {
                    Vector3Int cell = new Vector3Int(cx, cy, 0);

                    if (!tilemap.HasTile(cell)) continue;

                    if (activeNodes.ContainsKey(cell)) continue;

                    Vector3 worldPos3 = tilemap.GetCellCenterWorld(cell);
                    Vector2 worldPos = new Vector2(worldPos3.x, worldPos3.y);

                    if (Vector2.Distance(worldPos, playerPos) > spawnRadius) continue;

                    if (IsBlockedTile(worldPos)) continue;

                    Node node = Instantiate(nodePrefab, worldPos, Quaternion.identity);
                    activeNodes[cell] = node;
                    nodeList.Add(node);
                }
            }
        }
    }

    private void DespawnDistantNodes()
    {
        Vector2 playerPos = player.position;
        var toRemove = new List<Vector3Int>();

        foreach (var kvp in activeNodes)
        {
            if (Vector2.Distance(kvp.Value.transform.position, playerPos) > despawnRadius)
                toRemove.Add(kvp.Key);
        }

        foreach (var key in toRemove)
        {
            Node node = activeNodes[key];
            nodeList.Remove(node);
            Destroy(node.gameObject);
            activeNodes.Remove(key);
        }
    }

    private void DespawnDistantNPCs()
    {
        Vector2 playerPos = player.position;
        var toRemove = new List<NPCController>();

        foreach (NPCController npcInstance in activeNPCs)
        {
            if (npcInstance == null) { toRemove.Add(npcInstance); continue; }

            if (Vector2.Distance(npcInstance.transform.position, playerPos) > npcDespawnRadius)
                toRemove.Add(npcInstance);
        }

        foreach (NPCController npcInstance in toRemove)
        {
            activeNPCs.Remove(npcInstance);
            if (npcInstance != null) Destroy(npcInstance.gameObject);
        }
    }
    
    private void TopUpNPCs()
    {
        if (nodeList.Count == 0) return;

        int toSpawn = maxNPCs - activeNPCs.Count;

        for (int i = 0; i < toSpawn; i++)
        {
            Node randNode = nodeList[Random.Range(0, nodeList.Count)];
            NPCController newNPC = Instantiate(npc, randNode.transform.position, Quaternion.identity);
            newNPC.currentNode = randNode;
            activeNPCs.Add(newNPC);
        }
    }
    
    private void RebuildConnections()
    {
        float connectionDistance = 2f;

        foreach (Node node in nodeList)
            node.connections.Clear();

        for (int i = 0; i < nodeList.Count; i++)
        {
            for (int j = i + 1; j < nodeList.Count; j++)
            {
                if (Vector2.Distance(nodeList[i].transform.position, nodeList[j].transform.position) <= connectionDistance)
                {
                    if (!IsPathBlocked(nodeList[i].transform.position, nodeList[j].transform.position))
                    {
                        nodeList[i].connections.Add(nodeList[j]);
                        nodeList[j].connections.Add(nodeList[i]);
                    }
                }
            }
        }
    }
    
    private bool IsBlockedTile(Vector2 worldPos)
    {
        foreach (Tilemap tilemap in doNotSpawnTilemaps)
        {
            if (tilemap == null) continue;

            Vector3Int cell = tilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));

            if (tilemap.HasTile(cell)) return true;
        }
        return false;
    }

    private bool IsPathBlocked(Vector2 from, Vector2 to)
    {
        int steps = 5;
        for (int s = 1; s < steps; s++)
        {
            Vector2 sample = Vector2.Lerp(from, to, (float)s / steps);

            foreach (Tilemap tilemap in doNotSpawnTilemaps)
            {
                if (tilemap == null) continue;
                Vector3Int cell = tilemap.WorldToCell(new Vector3(sample.x, sample.y, 0));
                if (tilemap.HasTile(cell)) return true;
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        if (!canDrawGizmos) return;

        Gizmos.color = Color.blue;
        foreach (Node node in nodeList)
            foreach (Node connected in node.connections)
                Gizmos.DrawLine(node.transform.position, connected.transform.position);

        if (player != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.15f);
            Gizmos.DrawWireSphere(player.position, spawnRadius);
            Gizmos.color = new Color(1, 0, 0, 0.1f);
            Gizmos.DrawWireSphere(player.position, despawnRadius);
            Gizmos.color = new Color(1, 0.5f, 0, 0.1f);
            Gizmos.DrawWireSphere(player.position, npcDespawnRadius);
        }
    }
}