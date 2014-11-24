using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrNode : MonoBehaviour
{
	public static int CalculateCubeCount(int coreSize)
	{
		return 6 * (coreSize + 1) * (coreSize + 1) + 2;
	}

	public const float DURATION = 120.0f;
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
	int cubeIndex = 0;	// Used when constructing the node through AddCube. Represents the number of cubes when adding ends.
	int cubeConstructor = 0;	// A 1D value representing a 3D location, used when constructing the node through AddCube. Represents the volume of the node with the outer cube layer when adding ends.

	Quaternion rotationAngle;	// Random angle which specifies rotational axes.
	float rotationSpeed;

	public void Init(LinkedListNode<GameObject> node, int coreSize, bool infected)
	{
		Node = node;

		ChildCore.localScale = new Vector3(coreSize, coreSize, coreSize);

		// Set the size of the cubes array.
		Cubes = new LinkedListNode<GameObject>[CalculateCubeCount(coreSize)];
		cubeIndex = 0;
		cubeConstructor = 0;

		TimeLeft = DURATION;

		FullyInfected = infected;
		Infected = infected;

		// If fully infected, all cubes are infected.
		if (FullyInfected)
		{
			infectedCubeCount = Cubes.Length;
		}
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
		cubeIndex = 0;
		cubeConstructor = 0;
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

		// Store the linked list node for this cube.
		Cubes[cubeIndex] = cube;

		// Infect the cube if this node is a primary infection.
		if (Infected)
			cube.Value.GetComponent<scrCube>().Infect();

		/* Position the cube around the core. */

		// Get the number of cubes along one side of the node in one dimension.
		int shellSize = (int)ChildCore.localScale.x + 2;
		
		// Get the 3D coordinates that the cubeConstructor is at.
		int x = (cubeConstructor / shellSize) % shellSize;
		int y = cubeConstructor / (shellSize * shellSize);
		int z = cubeConstructor % shellSize;

		/* For example, a shellSize=3 cube is made like: 
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
		if (y != 0 && y != shellSize - 1)
	    {
			// Check if the x value is not the left or right.
			if (x != 0 && x != shellSize - 1)
			{
				// If the z value is not the front or back, set it to be the back.
				if (z != 0 && z != shellSize - 1)
				{
					// Determine the cubeConstructor for the back z at this x,y. Since z increases by 1 (and wraps around) every time cubeConstructor increases by 1, this is as simple as adding the z distance moved.
					cubeConstructor += (shellSize - 1 - z);

					// Set the z to the back.
					z = shellSize - 1;
				}
			}
		}
			
		// Position the cube with the x, y, z coordinates.
		cube.Value.transform.localPosition =  new Vector3(x, y, z) - Vector3.one * (shellSize - 1) * 0.5f;

		// Push the cube out from the radius to give each cube separation from their neighbouring cubes and to make them slightly rounded.
		cube.Value.transform.localPosition += cube.Value.transform.localPosition.normalized * ChildCore.localScale.x * 0.2f;

		// Set the rotation quaternion.
		rotationAngle = Random.rotationUniform;
		rotationSpeed = shellSize * 0.01f;

		// Increment the 1D cube constructor.
		++cubeConstructor;

		// Advance the cube index.
		++cubeIndex;
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
			line.SetColors(scrNodeMaster.Instance.InfectedMaterial.GetColor("_TintColor") + new Color(0.0f, 0.0f, 0.0f, 0.2f), ChildCore.renderer.material.GetColor("_TintColor") + new Color(0.0f, 0.0f, 0.0f, 0.2f));
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
