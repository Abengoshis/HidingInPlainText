using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrNode : MonoBehaviour
{
	public const float DURATION = 60.0f;


	public LinkedListNode<Transform> Node { get; private set; }
	public LinkedListNode<Transform>[] Cubes { get; private set; }	// All cubes of this node.							// if only there was a way to make node a friend class of the node master, then this would be safer. This is all for speed so when the node master destroys the node and wants to add the cube to the cube pool again it doesnt have to search. 
	int cubeIndex;	// Used when constructing the node through AddCube. Represents the number of cubes when adding ends.
	
	public float TimeLeft { get; private set; }

	public void Init(LinkedListNode<Transform> node, int numCubes)
	{
		// Set the size of the cubes array.
		Cubes = new LinkedListNode<Transform>[numCubes];
		cubeIndex = 0;

		TimeLeft = DURATION;
	}

	public void Reset()
	{
		Node = null;
		Cubes = null;
		cubeIndex = 0;
		TimeLeft = 0;
	}

	public void AddCube(LinkedListNode<Transform> cube)
	{
		Cubes[cubeIndex] = cube;
		++cubeIndex;
	}

	public void Assemble()
	{

	}

	// Use this for initialization
	void Start ()
	{
		// Start inactive.
		gameObject.SetActive(false);

		Reset ();
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
			scrNodeMaster.Instance.Destroy(Node);
		}
	}
}
