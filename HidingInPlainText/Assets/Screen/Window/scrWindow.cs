using UnityEngine;
using System.Collections;

public class scrWindow : MonoBehaviour
{
	public Transform 	ChildTitleBar { get; private set; }
	public TextMesh 	ChildTitleText { get; private set; }
	public Transform 	ChildContent { get; private set; }

	public bool IsFocus { get; private set; }
	public bool IsCollapsed { get; private set; }
	public bool IsMinimised { get; private set; }

	private Vector2 mouseDragOffset;

	// Use this for initialization
	void Start ()
	{
		scrSelectable sel;
		IsCollapsed = false;

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
				if (IsFocus)
					transform.position = (Vector3)(Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) + (Vector3)mouseDragOffset + new Vector3(0.0f, 0.0f, transform.position.z);
			};

			sel.OnRightPressed += CollapseExpand;

		ChildTitleText = transform.Find ("Title Text").GetComponent<TextMesh>();
		ChildContent = transform.Find ("Content");

		// Place the text at the left side of the title bar.
		ChildTitleText.transform.position = new Vector3(ChildTitleBar.position.x - ChildTitleBar.localScale.x * 0.5f + 0.1f, ChildTitleBar.position.y, ChildTitleText.transform.position.z);
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	public void CollapseExpand()
	{
		IsCollapsed = !IsCollapsed;
		if (IsCollapsed)
		{
			foreach (Renderer r in ChildContent.GetComponentsInChildren<Renderer>())
				r.enabled = false;
		}
		else
		{
			foreach (Renderer r in ChildContent.GetComponentsInChildren<Renderer>())
				r.enabled = true;
		}
	}
	
	public void MinimiseMaximise()
	{
		IsMinimised = !IsMinimised;
		IsCollapsed = IsMinimised;	// Collapse if minimised, expand if maximised.
		IsFocus = !IsMinimised;	// Focus if maximised, defocus if minimised.

		if (IsMinimised)
		{
			foreach (Renderer r in GetComponentsInChildren<Renderer>())
				r.enabled = false;
		}
		else
		{
			foreach (Renderer r in GetComponentsInChildren<Renderer>())
				r.enabled = true;
		}
	}


}
