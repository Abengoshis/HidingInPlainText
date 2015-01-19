using UnityEngine;
using System.Collections;

public class scrCamera : MonoBehaviour
{
	public static Matrix4x4 ScreenMatrix { get; private set; }

	public delegate void method ();
	public method PostRender;


	// Use this for initialization
	void Start ()
	{
		ScreenMatrix = 	Matrix4x4.TRS(new Vector3(0.5f, 0.5f), Quaternion.identity, new Vector3((float)Screen.height / Screen.width, 1.0f, 1.0f));
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	void OnPostRender()
	{
		if (PostRender != null)
			PostRender();
	}
}
