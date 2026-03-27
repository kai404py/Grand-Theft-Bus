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
        
        if (path.Count > 0)
        {
            int x = 0;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(path[x].transform.position.x, path[x].transform.position.y, -2), moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, path[x].transform.position) <= 0.1f)
            {
                currentNode = path[x];
                path.RemoveAt(x);
            }
        }
        else
        {
            Node[] nodes = FindObjectsOfType<Node>();
            while (path == null || path.Count == 0)
            {
                path = AStarManager.instance.GeneratePath(currentNode, nodes[Random.Range(0, nodes.Length)]);
            }
        }
    }
    
}
