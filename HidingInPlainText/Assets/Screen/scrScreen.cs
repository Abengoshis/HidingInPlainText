using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrScreen : MonoBehaviour
{
	#region Static
	public static scrFeed Feed { get; private set; }
	public static scrBrowser Browser { get; private set; }

	// Gets the 2D area of the given transform. (Maybe make this relative to the screen. Unnecessary if screen is not rotated.
	public static Rect Calculate2DArea(Transform t)
	{
		return new Rect(t.position.x - 0.5f * t.localScale.x, t.position.y - 0.5f * t.localScale.y, t.localScale.x, t.localScale.y);
	}
	#endregion


	public List<GameObject> Windows;

	// Use this for initialization
	void Start ()
	{
		for (int i = 0; i < Windows.Count; ++i)
		{
			Windows[i].transform.position = new Vector3(Windows[i].transform.position.x, Windows[i].transform.position.y, -(Windows.Count - i));
			Windows[i].GetComponent<scrWindow>().ChildTitleBar.GetComponent<scrSelectable>().OnLeftPressed += ReorderWindows;
		}

		Feed = GameObject.Find ("Window (Feed)").transform.Find ("Content").GetComponent<scrFeed>();
		Browser = GameObject.Find ("Window (Browser)").transform.Find ("Content").GetComponent<scrBrowser>();
	}
	
	// Update is called once per frame
	void Update ()
	{






	}

	void ReorderWindows()
	{
		// Reorder windows.
		for (int i = 0; i < Windows.Count; ++i)
		{
			if (Windows[i].GetComponent<scrWindow>().IsFocus)
			{
				// Remove the window and place it on the top.
				GameObject window = Windows[i];
				Windows.Remove (window);
				Windows.Insert (0, window);
				break;
			}
		}

		// Refresh Z order.
		for (int i = 0; i < Windows.Count; ++i)
		{
			Windows[i].transform.position = new Vector3(Windows[i].transform.position.x, Windows[i].transform.position.y, -(Windows.Count - i));
		}
	}
}
