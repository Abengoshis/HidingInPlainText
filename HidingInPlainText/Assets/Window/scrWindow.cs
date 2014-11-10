using UnityEngine;
using System.Collections;

public class scrWindow : MonoBehaviour
{
	private Transform childTitleBar;
	private Transform childContent;

	private Vector2 mouseDragOffset;

	// Use this for initialization
	void Start ()
	{
		scrSelectable sel;

		childTitleBar = transform.Find ("Title Bar");
			sel = childTitleBar.GetComponent<scrSelectable>();
			
			sel.OnLeftPressed += () =>
			{
				mouseDragOffset = (Vector2)transform.position - scrScreen.MousePosition;
			};

			sel.OnLeftHeld += () =>
			{
				transform.position = scrScreen.MousePosition + mouseDragOffset;
			};


		childContent = transform.Find ("Content");
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	



}
