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
	public static bool Loading { get; private set; }

	public static scrNodeMaster Instance { get; private set; }

	public static Color UNINFECTED_FRAGMENT_COLOUR;
	public static Color INFECTED_FRAGMENT_COLOUR;
	
	public static Color UNINFECTED_CORE_COLOUR;
	public static Color INFECTED_CORE_COLOUR;

	const float NODE_SPACING = 60.0f;
	const int NODES_PER_DIMENSION = 7;
	const int NODES_MAX = 80;
	const int NODES_MAX_UNINFECTED = 50;
	const int LOOPS_PER_FRAME = 50;	// Number of loops allowed per frame of a coroutine before yielding.

	public GameObject NodePrefab;
	public GameObject CubePrefab;

	public Material GridMaterial;
	public Material LinkMaterial;
	public Material FragmentUninfectedMaterial;
	public Material FragmentInfectedMaterial;
	public Material CoreUninfectedMaterial;
	public Material CoreInfectedMaterial;
	public Material NodeMaterial;

	LinkedList<GameObject> nodePool;	// All nodes that can spawn. Pooled for performance (fewer instantiations necessary).
	int inactiveNodeCount;	// The number of inactive (free) nodes at the start of the pool.
	
	LinkedList<GameObject> cubePool;	// All cubes that can be assigned to nodes. Pooled for performance (fewer instantiations necessary).
	int inactiveCubeCount;	// The number of inactive (free) cubes at the start of the pool.

	Vector3[] positions;
	int freePositionsCount;

	bool creating = false;
	Queue<Message> messageQueue = new Queue<Message>();

	public void ReceiveMessage(Message message)
	{
		messageQueue.Enqueue(message);
	}

	public IEnumerator Create(Message message, bool infected)
	{
		creating = true;

		// Don't create a node if there are no nodes available.
		if (inactiveNodeCount == 0)
		{
			Debug.Log ("There are no inactive nodes left to create a node for \"" + message.page_title + "\".");
			yield break;
		}

		// Don't create a node if there are no cubes available.
		if (inactiveCubeCount == 0)
		{
			Debug.Log ("There are no inactive cubes left to create a node for \"" + message.page_title + "\".");
			yield break;
		}

		// Set the size of the core based on the change_size of the message.
		int coreSize = Mathf.Min (Mathf.CeilToInt(Mathf.Log10(Mathf.Abs (message.change_size) + 2) * 3), scrNode.CORE_SIZE_MAX);

		// Get the number of cubes there would be for this core size.
		int numCubes = scrNode.CubePositions[coreSize - 1].Length;

		// Don't create a node if there aren't enough cubes available.
		if (inactiveCubeCount < numCubes)
		{
			Debug.Log ("Not enough inactive cubes (" + inactiveCubeCount + " available, " + numCubes + " needed) in the pool to create a node for \"" + message.page_title + "\".");
			yield break;
		}

		// Get the first inactive node in the node pool.
		LinkedListNode<GameObject> node = nodePool.First;

		// Activate the node and move it to the end of the list.
		ActivateNode(node);

		// Get the node script.
		scrNode nodeScript = node.Value.GetComponent<scrNode>();

		// Initialise the node.
		nodeScript.Init (node, message, coreSize, infected);

		int numLoops = 0;

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

			if (++numLoops == LOOPS_PER_FRAME)
			{
				numLoops = 0;
				yield return new WaitForEndOfFrame();
			}
		}

		// Position the node.
		node.Value.transform.position = GetRandomFreeNodePosition();

		// Randomise the cubes.
		nodeScript.RandomizeCubes();

		// Create links to infected nodes.
		CreateLinks(node);

		creating = false;
	}

	public IEnumerator Destroy(LinkedListNode<GameObject> node)
	{
		// Get the node script.
		scrNode nodeScript = node.Value.GetComponent<scrNode>();

		int numLoops = 0;

		// Clear the node's cubes and make them available to future nodes.
		for (int i = 0; i < nodeScript.Cubes.Length; ++i)
		{
			LinkedListNode<GameObject> cube = nodeScript.Cubes[i];

			// Reset the cube.
			cube.Value.GetComponent<scrCube>().Reset();

			// Deactivate the cube and add it to the front of the pool.
			DeactivateCube(cube);

			if (++numLoops == LOOPS_PER_FRAME)
			{
				numLoops = 0;
				yield return new WaitForEndOfFrame();
			}
		}

		// Reset the node.
		nodeScript.Reset();

		// Deactivate the node.
		DeactivateNode(node);
	}

	public void CreateLinks(LinkedListNode<GameObject> node)
	{
		scrNode nodeScript = node.Value.GetComponent<scrNode>();
		Bounds nodeBounds = new Bounds(node.Value.transform.position, new Vector3(NODE_SPACING * 2, NODE_SPACING * 2, NODE_SPACING * 2));

		LinkedList<GameObject>.Enumerator activeNode = nodePool.GetEnumerator();
		for (int i = 0; i < inactiveNodeCount; ++i)
			activeNode.MoveNext();

		while (activeNode.MoveNext() && nodeScript.CurrentLinks != scrNode.LINKS_MAX)
		{
			// Don't link to fully infected nodes.
			if (nodeScript.FullyInfected ^ activeNode.Current.GetComponent<scrNode>().FullyInfected)
			{
				// Check if the node is within range of this node.
				if (nodeBounds.Contains(activeNode.Current.transform.position))
				{
					if (nodeScript.FullyInfected)
						nodeScript.Link (activeNode.Current);
					else
						activeNode.Current.GetComponent<scrNode>().Link(node.Value);
				}
			}
		}
	}

	void ActivateNode(LinkedListNode<GameObject> node)
	{
		node.Value.SetActive(true);
		node.Value.transform.parent = null;

		nodePool.Remove (node);
		nodePool.AddLast(node);
		--inactiveNodeCount;
	}

	void DeactivateNode(LinkedListNode<GameObject> node)
	{
		node.Value.transform.parent = transform;
		node.Value.SetActive(false);

		nodePool.Remove (node);
		nodePool.AddFirst(node);
		++inactiveNodeCount;
	}

	void ActivateCube(LinkedListNode<GameObject> cube)
	{
		cube.Value.SetActive(true);
		cube.Value.transform.parent = null;

		cubePool.Remove(cube);
		cubePool.AddLast(cube);
		--inactiveCubeCount;
	}
	
	void DeactivateCube(LinkedListNode<GameObject> cube)
	{
		cube.Value.transform.parent = transform;
		cube.Value.SetActive(false);

		cubePool.Remove (cube);
		cubePool.AddFirst(cube);
		++inactiveCubeCount;
	}

	public void LoadNodePool()
	{
		nodePool = new LinkedList<GameObject>();
		
		inactiveNodeCount = NODES_MAX;

		for (int i = 0; i < NODES_MAX; ++i)
		{
			nodePool.AddLast((GameObject)Instantiate(NodePrefab));
			nodePool.Last.Value.transform.parent = transform;
		}
	}

	public void LoadCubePool(int numCubes)
	{
		cubePool = new LinkedList<GameObject>();

		inactiveCubeCount = numCubes;

		for (int i = 0; i < inactiveCubeCount; ++i)
		{
			cubePool.AddLast((GameObject)Instantiate (CubePrefab));
			cubePool.Last.Value.transform.parent = transform;
		}
	}

	public void PrecomputeNodePositions()
	{
		positions = new Vector3[NODES_PER_DIMENSION * NODES_PER_DIMENSION * NODES_PER_DIMENSION - 7];
		freePositionsCount = 0;

		int m = NODES_PER_DIMENSION / 2;

		// Create the positions, excluding 7 positions from the centre outwards orthogonally.
		for (int i = 0; i < NODES_PER_DIMENSION; ++i)
		{
			for (int j = 0; j < NODES_PER_DIMENSION; ++j)
			{
				for (int k = 0; k < NODES_PER_DIMENSION; k++)
				{
					if (!((i == m || i == m - 1 || i == m + 1) && j == m && k == m) &&
						!(i == m && (j == m || j == m - 1 || j == m + 1) && k == m) &&
					    !(i == m && j == m && (k == m || k == m - 1 || k == m + 1)))
					{
						positions[freePositionsCount] = new Vector3(i * NODE_SPACING - (NODES_PER_DIMENSION - 1) * NODE_SPACING * 0.5f, j * NODE_SPACING - (NODES_PER_DIMENSION - 1) * NODE_SPACING * 0.5f, k * NODE_SPACING - (NODES_PER_DIMENSION - 1) * NODE_SPACING * 0.5f);
						++freePositionsCount;
					}
				}
			}
		}

		//...
	}

	Vector3 GetRandomFreeNodePosition()
	{
		// Get a random free position.
		int index = Random.Range (0, freePositionsCount);
		Vector3 position = positions[index];

		// Decrement the free position count.
		--freePositionsCount;

		// Swap the random position with the first non-free position.
		positions[index] = positions[freePositionsCount];
		positions[freePositionsCount] = position;

		return position;
	}

	// Use this for initialization
	void Start ()
	{
		Instance = this;

		Camera.main.GetComponent<scrCamera>().PostRender += PostRender;

		UNINFECTED_FRAGMENT_COLOUR = FragmentUninfectedMaterial.color;
		UNINFECTED_CORE_COLOUR = CoreUninfectedMaterial.color;
		INFECTED_FRAGMENT_COLOUR = FragmentInfectedMaterial.GetColor("_MainColor");
		INFECTED_CORE_COLOUR = CoreInfectedMaterial.color;
	}

	// Update is called once per frame
	void Update ()
	{
		if (scrMaster.Loading)
			return;

		if (!creating)
		{
			if (messageQueue.Count != 0)
			{
				// Create infected nodes when a reversion, antivandalism or antispam is detected.
				// Do not create more than the max number of nodes, unless it would mean creating an infected node.
				Message message = messageQueue.Dequeue();
				string summary = message.summary != null ? message.summary.ToUpper () : "";
				if (summary.Contains("REVERT") || summary.Contains ("REVERSION") || summary.Contains("VANDAL") || summary.Contains("SPAM") || message.user.ToUpper() == "CLUEBOT NG")
				{
					StartCoroutine(Create (message, true));
				}
				else
				{
					if (message.is_anon && nodePool.Count - inactiveNodeCount < NODES_MAX_UNINFECTED)
					{
						StartCoroutine(Create (message, false));
					}
				}
			}
		}
	}

	void PostRender()
	{
		GL.PushMatrix();
		GridMaterial.SetPass(0);
		GL.Color(new Color(0.01f, 0.01f, 0.01f));

		GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
		GL.modelview = Camera.main.worldToCameraMatrix;


		GL.Begin (GL.LINES);

		int min = (int)(-(NODES_PER_DIMENSION) * 0.5f * NODE_SPACING);
		int max = -min;

		for (float i = min; i <= max; i += NODE_SPACING)
		{
			for (float j = min; j <= max; j += NODE_SPACING)
			{
				// Create x,y,z0 - x,y,z1 lines.
				GL.Vertex(new Vector3(i, j, min));
				GL.Vertex(new Vector3(i, j, max));

				// Create x,y0,z - x,y1,z lines.
				GL.Vertex(new Vector3(i, min, j));
				GL.Vertex(new Vector3(i, max, j));

				// Create x0,y,z - x1,y,z lines.
				GL.Vertex(new Vector3(min, i, j));
				GL.Vertex(new Vector3(max, i, j));
			}

		}

		GL.End();

		GL.PopMatrix();
	}
}
