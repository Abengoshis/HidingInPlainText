using UnityEngine;
using System.Collections;

public class scrCube : MonoBehaviour
{
	public bool Infected { get; private set; }


	public void Reset()
	{
		transform.parent = null;

		Infected = false;
	}

	// Use this for initialization
	void Start ()
	{
		Reset ();

		// Start inactive.
		gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
