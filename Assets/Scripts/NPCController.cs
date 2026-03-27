using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public Node currentNode;
    public List<Node> path = new List<Node>();
    private bool isPickedUp = false;
    private AudioSource audioSource;
    public AudioClip deathSound;
    public float moveSpeed;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPickedUp) return;
    
        if (other.CompareTag("Bus"))
        {
            isPickedUp = true;
            StartCoroutine(PickupSequence());
        }
    }

    private IEnumerator PickupSequence()
    {
        path.Clear();

        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    private void Update()
    {
        CreatePath();
    }

    public void CreatePath()
    {
        if (isPickedUp) return;

        if (path == null) path = new List<Node>();

        path.RemoveAll(node => node == null);

        if (path.Count > 0)
        {
            Node next = path[0];
            if (next == null)
            {
                path.RemoveAt(0);
                return;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(next.transform.position.x, next.transform.position.y, -2),
                moveSpeed * Time.deltaTime
            );

            if (Vector2.Distance(transform.position, next.transform.position) <= 0.1f)
            {
                currentNode = next;
                path.RemoveAt(0);
            }
        }
        else
        {
            Node[] nodes = FindObjectsOfType<Node>();
            if (nodes.Length == 0) return;

            if (currentNode == null)
                currentNode = nodes[Random.Range(0, nodes.Length)];

            int attempts = 0;
            while (path.Count == 0 && attempts < 10)
            {
                Node target = nodes[Random.Range(0, nodes.Length)];
                if (target == currentNode) { attempts++; continue; }

                List<Node> result = AStarManager.instance.GeneratePath(currentNode, target);
                if (result != null) path = result;
                attempts++;
            }
        }
    }
}
