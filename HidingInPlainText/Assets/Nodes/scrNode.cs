using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrNode : MonoBehaviour
{
	public static List<Vector3[]> CubePositions { get; private set; }	// All cube positions for each possible core size, precalculated. 

	/// <summary>
	/// Precomputes all local positions of cubes for each allowed size of core to reduce compuation during the game.
	/// </summary>
	public static void PrecomputeCubePositions()
	{
		CubePositions = new List<Vector3[]>();

		for (int core = 1; core <= CORE_SIZE_MAX; ++core)
		{
			Vector3[] positions = new Vector3[CalculateCubeCount(core)];

			for (int cube = 0, constructor = 0, shell = core + 2; cube < positions.Length; ++cube, ++constructor)	// Shell is the number of cubes along one dimension.
			{
				// Get the 3D coordinates that the constructor is at.
				int x = (constructor / shell) % shell;
				int y = constructor / (shell * shell);
				int z = constructor % shell;
				
				/* For example, a 'shell = 3' cube is made like: 
				 * x	y	z
				 * ----------
				 * 0	0	0
				 * 0	0	1
				 * 0	0	2
				 * 1	0	0
				 * 1	0	1
				 * 1	0	2
				 * 2	0	0
				 * 2	0	1
				 * 2	0	2
				 * 0	1	0
				 * 0	1	1
				 * 0	1	2
				 * 1	1	0
				 * 1	1	1
				 * 1	1	2
				 * 2	1	0...etc.
				 */
				
				// Check if the y layer is not the top or the bottom.
				if (y != 0 && y != shell - 1)
				{
					// Check if the x value is not the left or right.
					if (x != 0 && x != shell - 1)
					{
						// If the z value is not the front or back, set it to be the back.
						if (z != 0 && z != shell - 1)
						{
							// Determine the cubeConstructor for the back z at this x,y. Since z increases by 1 (and wraps around) every time cubeConstructor increases by 1, this is as simple as adding the z distance moved.
							constructor += (shell - 1 - z);
							
							// Set the z to the back.
							z = shell - 1;
						}
					}
				}
				
				// Set the position with the x,y,z coordinates.
				positions[cube] =  new Vector3(x, y, z) - Vector3.one * (shell - 1) * 0.5f;
				
				// Push the position out from the radius to give each cube separation from their neighbouring cubes and to make the node overall slightly rounded.
				positions[cube] += positions[cube].normalized * core * 0.2f;
			}

			CubePositions.Add(positions);
		}
	}

	public static int CalculateCubeCount(int coreSize)
	{
		return 6 * (coreSize + 1) * (coreSize + 1) + 2;
	}


	public const int CORE_SIZE_MAX = 5;
	public const float DURATION = 180.0f;
	public const int LINKS_MAX = 26;	// Number of links possible (also the number of 3d positions in a grid around one position.
	public const int LINK_VERTICES = 32;

	public Transform ChildCore { get; private set; }
	public float TimeLeft { get; private set; }	

	public bool FullyInfected { get; private set; }
	public bool Infected { get; private set; }
	float infectionPulseDelay = 10.0f;
	float infectionPulseTimer = 0.0f;
	int infectedCubeCount = 0;

	GameObject[] linkedNodes = new GameObject[LINKS_MAX];
	LineRenderer[] links = new LineRenderer[LINKS_MAX];
	public int CurrentLinks { get; private set; }

	public LinkedListNode<GameObject> Node { get; private set; }
	public LinkedListNode<GameObject>[] Cubes { get; private set; }	// All cubes of this node.							// if only there was a way to make node a friend class of the node master, then this would be safer. This is all for speed so when the node master destroys the node and wants to add the cube to the cube pool again it doesnt have to search. 
	List<Vector3[]>.Enumerator cubePositionEnumerator;	// Used when constructing the node to grab the cube positions without needing to iterate the list.
	int cubePositionIndex = 0;	// Used when constructing the node to grab cubes from the cube positions.

	Quaternion rotationAngle;	// Random angle which specifies rotational axes.
	float rotationSpeed;

	public void Init(LinkedListNode<GameObject> node, int coreSize, bool infected)
	{
		Node = node;
		TimeLeft = DURATION;

		// Get an enumerator to the list item containing the positions this node will use when being built.
		cubePositionEnumerator = CubePositions.GetEnumerator();
		for (int i = 0; i < coreSize; ++i)
			cubePositionEnumerator.MoveNext();
		cubePositionIndex = 0;
		Cubes = new LinkedListNode<GameObject>[cubePositionEnumerator.Current.Length];

		ChildCore.localScale = new Vector3(coreSize, coreSize, coreSize);

		FullyInfected = infected;
		Infected = infected;

		// If fully infected, all cubes are infected.
		if (FullyInfected)
		{
			infectedCubeCount = Cubes.Length;
			ChildCore.renderer.material = scrNodeMaster.Instance.InfectedNodeMaterial;
		}
		else
		{
			ChildCore.renderer.material = scrNodeMaster.Instance.UninfectedNodeMaterial;
		}

		// Set the rotation quaternion.
		rotationAngle = Random.rotationUniform;
		rotationSpeed = coreSize * 0.01f;
	}

	public void Reset()
	{
		// Unlink all nodes.
		for (int i = 0; i < LINKS_MAX; ++i)
		{
			linkedNodes[i] = null;
			links[i].enabled = false;
		}
		CurrentLinks = 0;

		Node = null;
		FullyInfected = false;
		Infected = false;
		Cubes = null;
		cubePositionIndex = 0;

		TimeLeft = 0;
		rotationAngle = Quaternion.identity;
		rotationSpeed = 0;
		infectionPulseTimer = 0.0f;
		infectedCubeCount = 0;
	}

	public void AddCube(LinkedListNode<GameObject> cube)
	{
		// Set the cube's parent to this node.
		cube.Value.transform.parent = this.transform;

		// Infect the cube if this node is a primary infection.
		if (Infected)
			cube.Value.GetComponent<scrCube>().Infect();

		// Position the cube with the precalculated array.
		cube.Value.transform.position = cubePositionEnumerator.Current[cubePositionIndex];

		// Store the linked list node for this cube.
		Cubes[cubePositionIndex] = cube;

		// Get ready for the next cube.
		++cubePositionIndex;
	}

	// Randomises the order of cubes so infection is more efficient. All swapping is done at initialisation so when infecting the cubes can just be iterated through.
	public void RandomizeCubes()
	{
		for (int i = 0; i < Cubes.Length; ++i)
		{
			int index = Random.Range (0, Cubes.Length);
			LinkedListNode<GameObject> temp = Cubes[index];
			Cubes[index] = Cubes[i];
			Cubes[i] = temp;
		}
	}

	public void Infect(int count)
	{
		Infected = true;

		if (infectedCubeCount + count > Cubes.Length)
		{
			count -= infectedCubeCount + count - Cubes.Length;
			FullyInfected = true;

			ChildCore.renderer.material = scrNodeMaster.Instance.InfectedNodeMaterial;

			scrNodeMaster.Instance.CreateLinks(Node);
		}

		for (int i = infectedCubeCount; i < infectedCubeCount + count; ++i)
		{
			Cubes[i].Value.GetComponent<scrCube>().Infect();
		}

		infectedCubeCount += count;
	}

	void InfectLinkedNodes()
	{
		for (int i = 0; i < CurrentLinks; ++i)
		{
			linkedNodes[i].GetComponent<scrNode>().Infect(Mathf.CeilToInt(infectedCubeCount * 0.01f));
		}
	}

	public void Link(GameObject node)
	{
		// Set the linked gameobject.
		linkedNodes[CurrentLinks] = node;

		// Set up the visual link.
		links[CurrentLinks].enabled = true;
		
		// Set the control point half way between the two points then pushed in a random direction perpendicular to the connecting line.
		float curve = 30.0f;
		Vector3 control = Vector3.Lerp (transform.position, node.transform.position, 0.5f) + curve * Vector3.Cross (node.transform.position - transform.position, Random.rotationUniform.eulerAngles).normalized;
		
		for (int i = 0; i < LINK_VERTICES; ++i)
		{
			float t = (float)i / (LINK_VERTICES - 1);
			float tInv = 1.0f - t;
			
			links[CurrentLinks].SetPosition(i, tInv * tInv * transform.position + 2 * tInv * t * control + t * t * node.transform.position);
		}

		++CurrentLinks;
	}

	// Use this for initialization
	void Start ()
	{
		ChildCore = transform.Find ("Core");

		for (int i = 0; i < LINKS_MAX; ++i)
		{
			GameObject childLink = new GameObject("Link");
			childLink.transform.parent = this.transform;

			LineRenderer line = childLink.AddComponent<LineRenderer>();
			line.material = scrNodeMaster.Instance.LinkMaterial;
			line.SetColors(scrNodeMaster.Instance.InfectedNodeMaterial.GetColor("_TintColor") + new Color(0.0f, 0.0f, 0.0f, 0.2f), scrNodeMaster.Instance.UninfectedNodeMaterial.GetColor("_TintColor") + new Color(0.0f, 0.0f, 0.0f, 0.2f));
			line.SetVertexCount(LINK_VERTICES);
			line.enabled = false;
			links[i] = line;
			linkedNodes[i] = null;
		}

		Reset ();

		// Start inactive.
		gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (FullyInfected)
		{
			// Clear redundant links.
			for (int i = 0; i < CurrentLinks; ++i)
			{
				if (!linkedNodes[i].activeSelf || linkedNodes[i].GetComponent<scrNode>().FullyInfected)
				{
					links[i].enabled = false;

					linkedNodes[i] = linkedNodes[CurrentLinks - 1];
					linkedNodes[CurrentLinks - 1] = null;

					LineRenderer temp = links[CurrentLinks - 1];
					links[CurrentLinks - 1] = links[i];
					links[i] = temp;


					--i;
					--CurrentLinks;
				}
			}

		}

		// Rotate.
		transform.rotation = Quaternion.Lerp(transform.rotation, transform.rotation * rotationAngle, rotationSpeed * Time.deltaTime);

		// t1CK t0CK 8r8k H34DS
		if (TimeLeft > 0)
		{
			TimeLeft -= Time.deltaTime;

			if (FullyInfected)
			{
				infectionPulseTimer += Time.deltaTime;
				if (infectionPulseTimer > infectionPulseDelay)
				{
					infectionPulseTimer = 0;
					InfectLinkedNodes();
				}
			}
		}
		else
		{
			TimeLeft = 0;

			// "Destroy" this node.
			scrNodeMaster.Instance.Destroy(Node);
		}
	}
}
