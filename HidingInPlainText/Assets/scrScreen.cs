using UnityEngine;
using System.Collections;

public class scrScreen : MonoBehaviour
{
	public static Vector2 MousePosition { get; private set; }

	public static Transform AcceptButton { get; private set; }
	public static Transform RejectButton { get; private set; }
	public static Transform Feed { get; private set; }
	public static Transform Browser { get; private set; }

	// Gets the 2D area of the given transform. (Maybe make this relative to the screen. Unnecessary if screen is not rotated.
	public static Rect Calculate2DArea(Transform t)
	{
		return new Rect(t.position.x - 0.5f * t.localScale.x, t.position.y - 0.5f * t.localScale.y, t.localScale.x, t.localScale.y);
	}

	// Use this for initialization
	void Start ()
	{
		//AcceptButton = transform.Find ("Buttons").Find ("Accept");
		//RejectButton = transform.Find ("Buttons").Find ("Reject");
		//Feed = transform.Find("Feed");
		//Browser = transform.Find ("Browser");
	}
	
	// Update is called once per frame
	void Update ()
	{
		MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

	}
}
