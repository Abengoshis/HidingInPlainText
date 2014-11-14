using UnityEngine;
using System.Collections;

public class scrScrollbar : MonoBehaviour
{

	public Transform ChildBackground { get; private set; }
	public Transform ChildUp { get; private set; }
	public Transform ChildDown { get; private set; }
	public Transform ChildScroller { get; private set; }

	// A value from 0.0f to 1.0f where 0.0f is when the scroller is at the top of the bar and 1.0f is when the scroller is at the bottom of the bar.
	public float Amount { get; private set; }

	// The amount to move when using the up or down buttons.
	public float Interval = 1.0f;

	private float mouseDragOffset;

	// Local max and min y value the scroller is limited within.
	private float yLimit;

	// Use this for initialization
	void Start ()
	{
		ChildBackground = transform.Find ("Background");
		ChildUp = transform.Find ("Up");
		ChildDown = transform.Find ("Down");
		ChildScroller = transform.Find ("Scroller");

		// Put the up and down buttons at the top and bottom of the scroller.
		ChildUp.position = new Vector3(transform.position.x, transform.position.y + ChildBackground.localScale.y * 0.5f - ChildUp.localScale.y * 0.5f, ChildUp.position.z);
		ChildDown.position = new Vector3(transform.position.x, transform.position.y - ChildBackground.localScale.y * 0.5f + ChildUp.localScale.y * 0.5f, ChildUp.position.z);

		// Calculate the y limit.
		yLimit = ChildBackground.localScale.y * 0.5f - ChildUp.localScale.y;

		ChildScroller.GetComponent<scrSelectable>().OnLeftPressed += () =>
		{
			mouseDragOffset = ChildScroller.position.y - Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
		};

		ChildScroller.GetComponent<scrSelectable>().OnLeftHeld += () =>
		{
			ChildScroller.position = new Vector3(ChildScroller.position.x, mouseDragOffset + Camera.main.ScreenToWorldPoint(Input.mousePosition).y, ChildScroller.position.z);

			float top = ChildScroller.localPosition.y + ChildScroller.localScale.y * 0.5f;
			if (top > yLimit)
			{
				ChildScroller.Translate(0.0f, yLimit - top, 0.0f);
			}
			else
			{
				float bottom = ChildScroller.localPosition.y - ChildScroller.localScale.y * 0.5f;
				if (bottom < -yLimit)
				{
					ChildScroller.Translate(0.0f, -yLimit - bottom, 0.0f);
				}
			}

			CalculateAmount();

		};

		ChildUp.GetComponent<scrSelectable>().OnLeftHeld += () =>
		{
			ChildScroller.Translate(0.0f, Interval * Time.deltaTime, 0.0f);
			
			float top = ChildScroller.localPosition.y + ChildScroller.localScale.y * 0.5f;
			if (top > yLimit)
			{
				ChildScroller.Translate(0.0f, yLimit - top, 0.0f);
			}

			CalculateAmount();
		};

		ChildDown.GetComponent<scrSelectable>().OnLeftHeld += () =>
		{
			ChildScroller.Translate(0.0f, -Interval * Time.deltaTime, 0.0f);
			
			float bottom = ChildScroller.localPosition.y - ChildScroller.localScale.y * 0.5f;
			if (bottom < -yLimit)
			{
				ChildScroller.Translate(0.0f, -yLimit - bottom, 0.0f);
			}

			CalculateAmount();
		};

		SetSize (1.0f);
		ResetPosition ();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	private void CalculateAmount()
	{
		// Get the min and max local positions of the scroller.
		float yMax = yLimit - ChildScroller.localScale.y * 0.5f;
		float yMin = -yLimit + ChildScroller.localScale.y * 0.5f;
		
		Amount =  (yMax - ChildScroller.localPosition.y) / (yMax - yMin);
	}

	public void ResetPosition()
	{
		ChildScroller.localPosition = new Vector3(ChildScroller.localPosition.x, yLimit - ChildScroller.localScale.y * 0.5f, ChildScroller.localPosition.z);
	}

	public void SetSize(float proportion)
	{
		ChildScroller.localScale = new Vector3(ChildScroller.localScale.x, yLimit * 2.0f * proportion, ChildScroller.localScale.z);
	}
}
