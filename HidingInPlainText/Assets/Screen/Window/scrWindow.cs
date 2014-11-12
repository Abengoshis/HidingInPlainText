using UnityEngine;
using System.Collections;

public class scrWindow : MonoBehaviour
{
	public Transform ChildTitleBar;
	public Transform ChildContent;

	public bool IsFocus { get; private set; }
	public bool IsCollapsed { get; private set; }

	private Vector2 mouseDragOffset;

	// Use this for initialization
	void Start ()
	{
		scrSelectable sel;

		ChildTitleBar = transform.Find ("Title Bar");
			sel = ChildTitleBar.GetComponent<scrSelectable>();
			
			sel.OnLeftPressed += () =>
			{
				IsFocus = true;
				mouseDragOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
			};

			sel.OnLeftReleased += () =>
			{
				IsFocus = false;
			};

			sel.OnLeftHeld += () =>
			{
				transform.position = (Vector3)(Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) + (Vector3)mouseDragOffset + new Vector3(0.0f, 0.0f, transform.position.z);
			};

			sel.OnRightPressed += () =>
			{
				IsCollapsed = !IsCollapsed;
				if (IsCollapsed)
				{
					
				}
				else
				{

				}
			};


		ChildContent = transform.Find ("Content");
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	



}
