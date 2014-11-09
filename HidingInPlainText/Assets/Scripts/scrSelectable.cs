using UnityEngine;
using System.Collections;

public class scrSelectable : MonoBehaviour
{
	public scrScreen parentScreen;

	private Color normalColour;
	private Color hoverColour;
	private Color selectColour;

	public bool Clicked { get; private set; }

	public delegate void method();
	public method OnSelect;
	
	void Start ()
	{
		Transform screen = transform.root.Find ("Screen");
		if (screen != null)
			parentScreen = screen.GetComponent<scrScreen>();
	}

	void Update ()
	{
		Clicked = false;

		if (parentScreen.Calculate2DArea(transform).Contains(parentScreen.MousePosition))
	    {
			if ((parentScreen.MouseClicked & 1) > 0)
			{
				Clicked = true;
				OnSelect ();
			}
		}
	}

}
