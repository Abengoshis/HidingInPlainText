using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* I really want to minimised computation here, so instead of arrays I've used doubly linked lists and access/reorder elements using linked list nodes.
 * By tightly coupling this with the scrNode class, when a node wants to destroy itself it can simply supply the node master with its linked list node and 
 * this can be repositioned, rather than having to search through the entire pool of nodes, then swap data via a temp variable. A pool has been used because
 * it assigns all memory at the start and sets a limit on the amount of things that can be in the game world. Instantiation and destruction are intensive
 * processes, so reordering, activating and deactivating items in a pool should save on computation at the expense of potentially wasted memory. */

public class scrNodeMaster : MonoBehaviour
{
	public static scrNodeMaster Instance { get; private set; }

	public GameObject NodePrefab;
	public GameObject CubePrefab;

	LinkedList<GameObject> nodePool;	// All nodes that can spawn. Pooled for performance (fewer instantiations necessary).
	int inactiveNodeCount;	// The number of inactive (free) nodes at the start of the pool.
	
	LinkedList<GameObject> cubePool;	// All cubes that can be assigned to nodes. Pooled for performance (fewer instantiations necessary).
	int inactiveCubeCount;	// The number of inactive (free) cubes at the start of the pool.

	public void Create(Message message)
	{
		// Don't create a node if there are no nodes available.
		if (inactiveNodeCount == 0)
		{
			Debug.Log ("There are no inactive nodes left to create a node for \"" + message.page_title + "\".");
			return;
		}

		// Don't create a node if there are no cubes available.
		if (inactiveCubeCount == 0)
		{
			Debug.Log ("There are no inactive cubes left to create a node for \"" + message.page_title + "\".");
			return;
		}

		// Set the size of the core based on the change_size of the message.
		int coreSize = message.change_size;

		// Get the number of cubes there would be for this core size.
		int numCubes = scrNode.CalculateCubeCount(coreSize);

		// Don't create a node if there aren't enough cubes available.
		if (inactiveCubeCount < numCubes)
		{
			Debug.Log ("Not enough inactive cubes (" + inactiveCubeCount + " available, " + numCubes + " needed) in the pool to create a node for \"" + message.page_title + "\".");
			return;
		}

		// Get the first inactive node in the node pool.
		LinkedListNode<GameObject> node = nodePool.First;

		// Activate the node and move it to the end of the list.
		ActivateNode(node);

		// Get the node script.
		scrNode nodeScript = node.Value.GetComponent<scrNode>();

		// Initialise the node.
		nodeScript.Init (node, coreSize);

		// Loop through the cube pool and assign deactivated cubes to the node.
		LinkedListNode<GameObject> cube = cubePool.First;
		for (int i = 0; i < numCubes; ++i)
		{
			// Get the next cube before the cube is rearranged.
			LinkedListNode<GameObject> next = cube.Next;

			// Activate the cube and move it to the end of the pool.
			ActivateCube(cube);

			// Add the cube to the node.
			nodeScript.AddCube(cube);

			// Move to the next cube in the pool.
			cube = next;
		}

		// Position the node.

		// Link the node to surrounding nodes.
	}

	public void Destroy(LinkedListNode<GameObject> node)
	{
		// Get the node script.
		scrNode nodeScript = node.Value.GetComponent<scrNode>();

		// Clear the node's cubes and make them available to future nodes.
		for (int i = 0; i < nodeScript.Cubes.Length; ++i)
		{
			LinkedListNode<GameObject> cube = nodeScript.Cubes[i];

			// Reset the cube.
			cube.Value.GetComponent<scrCube>().Reset();

			// Deactivate the cube and add it to the front of the pool.
			DeactivateCube(cube);
		}

		// Reset the node.
		nodeScript.Reset();

		// Deactivate the node.
		DeactivateNode(node);
	}

	void ActivateNode(LinkedListNode<GameObject> node)
	{
		node.Value.SetActive(true);
		nodePool.Remove (node);
		nodePool.AddLast(node);
		--inactiveNodeCount;
	}

	void DeactivateNode(LinkedListNode<GameObject> node)
	{
		node.Value.SetActive(false);
		nodePool.Remove (node);
		nodePool.AddFirst(node);
		++inactiveNodeCount;
	}

	void ActivateCube(LinkedListNode<GameObject> cube)
	{
		cube.Value.SetActive(true);
		cubePool.Remove(cube);
		cubePool.AddLast(cube);
		--inactiveCubeCount;
	}
	
	void DeactivateCube(LinkedListNode<GameObject> cube)
	{
		cube.Value.SetActive(false);
		cubePool.Remove (cube);
		cubePool.AddFirst(cube);
		++inactiveCubeCount;
	}

	// Use this for initialization
	void Start ()
	{
		Instance = this;

		// Generate the pools.
		int numNodes = 10;
		int numCubes = 1000;

		nodePool = new LinkedList<GameObject>();
		cubePool = new LinkedList<GameObject>();

		for (int i = 0; i < numNodes; ++i)
		{
			nodePool.AddLast((GameObject)Instantiate(NodePrefab));
		}

		inactiveNodeCount = numNodes;
		
		for (int i = 0; i < numCubes; ++i)
		{
			cubePool.AddLast((GameObject)Instantiate (CubePrefab));
		}

		inactiveCubeCount = numCubes;
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
}
