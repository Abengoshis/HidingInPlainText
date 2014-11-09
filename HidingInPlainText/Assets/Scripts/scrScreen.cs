using UnityEngine;
using System.Collections;

public class scrScreen : MonoBehaviour
{
	public Vector2 	MousePosition { get; private set; }
	public int		MouseClicked { get; private set; }

	public Transform AcceptButton { get; private set; }
	public Transform RejectButton { get; private set; }
	public Transform Feed { get; private set; }
	public Transform Browser { get; private set; }

	// Gets the 2D area of the given transform. (Maybe make this relative to the screen. Unnecessary if screen is not rotated.
	public Rect Calculate2DArea(Transform t)
	{
		return new Rect(t.position.x - 0.5f * t.localScale.x, t.position.y - 0.5f * t.localScale.y, t.localScale.x, t.localScale.y);
	}

	// Use this for initialization
	void Start ()
	{
		MousePosition = Vector2.zero;
		MouseClicked = 0;

		AcceptButton = transform.Find ("Buttons").Find ("Accept");
		RejectButton = transform.Find ("Buttons").Find ("Reject");
		Feed = transform.Find("Feed");
		Browser = transform.Find ("Browser");
	}
	
	// Update is called once per frame
	void Update ()
	{
		MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		MouseClicked = (Input.GetMouseButton(0) ? 1 : 0) | (Input.GetMouseButton (1) ? 2 : 0);

	}
}
