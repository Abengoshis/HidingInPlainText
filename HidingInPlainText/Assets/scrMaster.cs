using UnityEngine;
using System.Collections;

public class scrMaster : MonoBehaviour
{
	public static scrMaster Instance { get; private set; }

	// Use this for initialization
	void Start ()
	{
		Instance = this;

		Screen.lockCursor = true;

		Settings.Reset();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
