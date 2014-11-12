using UnityEngine;
using System.Collections;

public class scrSelectable : MonoBehaviour
{
	private Color normalColour;
	private Color hoverColour;
	private Color selectedColour;

	public bool LeftSelected { get; private set; }
	public bool RightSelected { get; private set; }

	public delegate void method();
	public method OnLeftPressed;
	public method OnLeftHeld;
	public method OnLeftReleased;
	public method OnRightPressed;
	public method OnRightHeld;
	public method OnRightReleased;
	
	void Start ()
	{
		LeftSelected = false;
		normalColour = renderer.material.color;
		hoverColour = renderer.material.color + Color.white * 0.1f;
		selectedColour = renderer.material.color + Color.white * 0.2f;
	}

	void Update ()
	{
		// Hold left.
		if (Input.GetMouseButton(0) && LeftSelected)
		{
			Debug.Log (name + " Left Held.");

			if (OnLeftHeld != null)
				OnLeftHeld();
		}
		
		// Hold right.
		if (Input.GetMouseButton(1) && RightSelected)
		{
			Debug.Log (name + " Right Held.");

			if (OnRightHeld != null)
				OnRightHeld();
		}

		// Press and Release.
		RaycastHit hit;
		Physics.Raycast (Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
		if (hit.transform == transform)
	    {
			renderer.material.color = hoverColour;

			if (Input.GetMouseButtonDown (0))
			{
				if (!LeftSelected)
				{
					Debug.Log (name + " Left Clicked.");
					LeftSelected = true;

					if (OnLeftPressed != null)
						OnLeftPressed();
				}
			}

			if (Input.GetMouseButtonDown(1))
			{
				if (!RightSelected)
				{
					Debug.Log (name + " Right Clicked.");
					RightSelected = true;

					if (OnRightPressed != null)
						OnRightPressed();
				}
			}
		}
		else
		{
			renderer.material.color = normalColour;
		}

		if (Input.GetMouseButtonUp(0))
		{
			if (LeftSelected)
			{
				Debug.Log (name + " Left Released.");
				LeftSelected = false;
				
				if (OnLeftReleased != null)
					OnLeftReleased();
			}
		}
		
		if (Input.GetMouseButtonUp(1))
		{
			if (RightSelected)
			{
				Debug.Log (name + " Right Released.");
				RightSelected = false;
				
				if (OnRightReleased != null)
					OnRightReleased();
			}
		}

		if (LeftSelected || RightSelected)
		{
			renderer.material.color = selectedColour;
		}
	}

}
