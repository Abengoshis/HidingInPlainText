using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrNode : MonoBehaviour
{
	public static int CalculateCubeCount(int coreSize)
	{
		return 6 * (coreSize + 1) * (coreSize + 1) + 2;
	}

	public const float DURATION = 60.0f;

	public Transform ChildCore { get; private set; }
	public float TimeLeft { get; private set; }

	public LinkedListNode<GameObject> Node { get; private set; }
	public LinkedListNode<GameObject>[] Cubes { get; private set; }	// All cubes of this node.							// if only there was a way to make node a friend class of the node master, then this would be safer. This is all for speed so when the node master destroys the node and wants to add the cube to the cube pool again it doesnt have to search. 
	int cubeIndex;	// Used when constructing the node through AddCube. Represents the number of cubes when adding ends.
	int cubeConstructor;	// A 1D value representing a 3D location, used when constructing the node through AddCube. Represents the volume of the node with the outer cube layer when adding ends.

	public void Init(LinkedListNode<GameObject> node, int coreSize)
	{
		ChildCore.localScale = new Vector3(coreSize, coreSize, coreSize);

		// Set the size of the cubes array.
		Cubes = new LinkedListNode<GameObject>[CalculateCubeCount(coreSize)];
		cubeIndex = 0;
		cubeConstructor = 0;

		TimeLeft = DURATION;
	}

	public void Reset()
	{
		Node = null;
		Cubes = null;
		cubeIndex = 0;
		cubeConstructor = 0;
		TimeLeft = 0;
	}

	public void AddCube(LinkedListNode<GameObject> cube)
	{
		// Set the cube's parent to this node.
		cube.Value.transform.parent = this.transform;

		// Store the linked list node for this cube.
		Cubes[cubeIndex] = cube;

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
		cube.Value.transform.localPosition += cube.Value.transform.localPosition.normalized * 0.5f;

		// Increment the 1D cube constructor.
		++cubeConstructor;

		// Advance the cube index.
		++cubeIndex;
	}

	// Use this for initialization
	void Start ()
	{
		ChildCore = transform.Find ("Core");

		Reset ();

		// Start inactive.
		gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
	{
		// t1CK t0CK 8r8k H34DS
		if (TimeLeft > 0)
		{
			TimeLeft -= Time.deltaTime;
		}
		else
		{
			TimeLeft = 0;

			// "Destroy" this node.
			//scrNodeMaster.Instance.Destroy(Node);
		}
	}
}
