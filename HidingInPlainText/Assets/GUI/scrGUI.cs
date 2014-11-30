using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrGUI : MonoBehaviour
{
	public static scrGUI Instance { get; private set; }

	struct Callout
	{
		public GameObject Text;
		public GameObject Background;
		public Vector3 Offset;

		public Callout(GameObject text)
		{
			Text = text;
			Background = text.transform.Find ("Background").gameObject;
			Offset = text.transform.position;
		}

		public void Show()
		{
			Text.SetActive(true);
			Background.SetActive(true);
		}

		public void Hide()
		{
			Background.SetActive(false);
			Text.SetActive(false);
		}

		public void SetText(string text)
		{
			Text.guiText.text = text;
			Rect backgroundRect = Background.guiTexture.pixelInset;
			backgroundRect.width = Text.guiText.GetScreenRect().width + Text.guiText.pixelOffset.x + 22;
			Background.guiTexture.pixelInset = backgroundRect;
		}
	}
	
	Dictionary<string, Callout> callouts;
	bool calloutsVisible = true;
	Transform calloutAnchor = null;


	public void ShowCallouts()
	{
		if (!calloutsVisible)
		{
			foreach (Callout c in callouts.Values)
				c.Show ();

			calloutsVisible = true;
		}
	}

	public void HideCallouts()
	{
		if (calloutsVisible)
		{
			foreach (Callout c in callouts.Values)
				c.Hide();

			calloutsVisible = false;
		}
	}

	public void UpdateCallouts(scrNode node)
	{
		Message message = node.Data;

		callouts["page_title"].SetText(message.page_title);
		callouts["change_size"].SetText(message.change_size.ToString() + " bytes");
		callouts["user"].SetText("uploaded by " + message.user);

		if (message.geo != null)
		{
			callouts["location"].SetText(message.geo.country_name + ", " + message.geo.region_name + ", " + message.geo.city);
			callouts["latitude"].SetText(message.geo.latitude.ToString());
			callouts["longitude"].SetText(message.geo.longitude.ToString());
		}
		else
		{
			callouts["location"].SetText("no data");
			callouts["latitude"].SetText("no data");
			callouts["longitude"].SetText("no data");
		}

		callouts["infection"].SetText(node.GetInfectionPercentage().ToString() + "% infected");
		callouts["corruption"].SetText(node.GetCorruptionPercentage().ToString() + "% corrupted");

		calloutAnchor = node.transform;
	}

	// Use this for initialization
	void Start ()
	{
		Instance = this;

		callouts = new Dictionary<string, Callout>();
		callouts.Add ("page_title", new Callout(transform.Find ("Node").Find ("Title").gameObject));
		callouts.Add ("change_size", new Callout(transform.Find ("Node").Find ("Size").gameObject));
		callouts.Add ("user", new Callout(transform.Find ("Node").Find ("User").gameObject));
		callouts.Add ("location", new Callout(transform.Find("Node").Find ("Location").gameObject));
		callouts.Add ("latitude", new Callout(transform.Find ("Node").Find ("Latitude").gameObject));
		callouts.Add ("longitude", new Callout(transform.Find ("Node").Find ("Longitude").gameObject));
		callouts.Add ("infection", new Callout(transform.Find ("Node").Find ("Infection").gameObject));
		callouts.Add ("corruption", new Callout(transform.Find ("Node").Find ("Corruption").gameObject));
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (calloutAnchor == null)
		{
			HideCallouts();
		}
		else
		{
			if (Vector3.Distance(scrPlayer.Instance.transform.position, calloutAnchor.position) > scrPlayer.SCAN_DISTANCE)
				calloutAnchor = null;

			if (Camera.main.WorldToViewportPoint(calloutAnchor.position).z <= 0)
				HideCallouts();
			else
			{
				ShowCallouts();
				foreach (Callout c in callouts.Values)
				{
					c.Text.transform.position = Camera.main.WorldToViewportPoint(calloutAnchor.position) + c.Offset - Vector3.one * 0.5f;
				}
			}
		}
	}
}
