using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Camera is set up so the 'resolution' of the screen is 16x9.

public class scrFeed : MonoBehaviour
{
	private class Entry
	{
		public GameObject gameObject;
		public Transform BackgroundObject { get; private set; }
		public Transform TitleObject { get; private set; }
		public Transform SizeObject { get; private set; }
		
		public string 	Title;
		public string 	URL;
		public int 		Size;

		public Entry(GameObject gameObject, string title, string url, int size)
		{
			Title = title;
			URL = url;
			Size = size;
			
			// Assign the gameobject and set its 3D text values.
			this.gameObject = gameObject;
			TitleObject = this.gameObject.transform.Find ("Title");
			TextMesh tm = TitleObject.GetComponent<TextMesh>();
			tm.text = Title;
			TitleObject.renderer.material.SetColor("_Color", tm.color);
			TitleObject.renderer.material.SetTexture("_MainTex", tm.font.material.mainTexture);


			SizeObject = this.gameObject.transform.Find ("Size");
			tm = SizeObject.GetComponent<TextMesh>();
			tm.text = Size > 0 ? "+" + Size.ToString() : Size.ToString();	// - values already have a '-' before them when converted to a string.
			SizeObject.renderer.material.SetColor("_Color", tm.color);
			SizeObject.renderer.material.SetTexture("_MainTex", tm.font.material.mainTexture);

			BackgroundObject = this.gameObject.transform.Find ("Background");
		}

	}

	public GameObject EntryPrefab;
	
	private Entry 		selectedEntry;

	private Transform 			liveFeedBox;
	private LinkedList<Entry> 	liveEntries;
	private float 				liveEntrySpacing;
	private float 				liveEntrySpeed;
	


	public void AddEntry(string title, string url, int size)
	{
		// Create a new entry.
		Entry entry = new Entry(Instantiate(EntryPrefab) as GameObject, title, url, size);

		// Set the parent of the entry to this transform.
		entry.gameObject.transform.parent = transform;

		// Set the entry's selection event.
		entry.gameObject.GetComponentInChildren<scrSelectable>().OnLeftPressed += SelectEntry;

		// Position the entry after the last entry in the list, if the list has any entries in it. PERHAPS CHANGE THIS SO THEY ARE ALWAYS IN FIXED INCREMENTS.
		if (liveEntries.Count == 0 || scrScreen.Calculate2DArea(liveEntries.Last.Value.BackgroundObject).yMin > scrScreen.Calculate2DArea(liveFeedBox).yMin)
		{
			// Place underneath the bottom of the feed box.
			entry.gameObject.transform.position = liveFeedBox.position - new Vector3(0.0f, liveFeedBox.localScale.y * 0.5f + liveEntrySpacing, -EntryPrefab.transform.position.z);
		}
		else
		{
			// Place after the last item.
			entry.gameObject.transform.position = liveEntries.Last.Value.gameObject.transform.position - new Vector3(0.0f, liveEntrySpacing, 0.0f);
		}

		// Add the entry to the end of the list.
		liveEntries.AddLast(entry);
	}

	// Selects an entry.
	void SelectEntry()
	{
		if (selectedEntry == null)
		{
			foreach (Entry entry in liveEntries)
			{
				if (entry.gameObject.GetComponentInChildren<scrSelectable>().LeftSelected)
				{
					// Set the selected entry.
					selectedEntry = entry;

					// Set the feed top and bottom too far for the shader to clip them based off its world position (should probably not do this, instead either assign a different material or just change some text or something)
					selectedEntry.BackgroundObject.renderer.material.SetFloat ("_ClipTop", 100);
					selectedEntry.BackgroundObject.renderer.material.SetFloat ("_ClipBottom", -100);

					// Begin loading a url.
					scrScreen.Browser.BeginLoad (selectedEntry.URL);

					// Remove the entry from the live feed.
					liveEntries.Remove (selectedEntry);

					return;
				}
			}
		}
		else
		{
			Debug.Log ("Can't select more than one entry at a time.");
		}
		
		
	}

	// Deselects the current entry.
	void DeselectEntry()
	{
		if (selectedEntry != null)
		{
			Destroy (selectedEntry.gameObject);
			selectedEntry = null;
		}
		else
		{
			Debug.Log ("There is no entry to deselect.");
		}
	}

	void AcceptSelectedEntry()
	{
		if (selectedEntry != null)
		{

			DeselectEntry();
		}
		else
		{
			Debug.Log ("There is no entry to accept.");
		}
	}

	void RejectSelectedEntry()
	{
		if (selectedEntry != null)
		{
			
			DeselectEntry();
		}
		else
		{
			Debug.Log ("There is no entry to reject.");
		}
	}


	void Start ()
	{
		selectedEntry = null;

		// Set up the live feed box.
		liveFeedBox = transform.Find ("Live Feed Box");
		liveEntries = new LinkedList<Entry>();
		liveEntrySpacing = EntryPrefab.transform.Find ("Background").localScale.y + 0.02f;
		liveEntrySpeed = 0.1f;	// Maybe allow players to speed this up.

		// Add to button selection events.
		//scrScreen.AcceptButton.GetComponentInChildren<scrSelectable>().OnLeftPressed += AcceptSelectedEntry;
		//scrScreen.RejectButton.GetComponentInChildren<scrSelectable>().OnLeftPressed += RejectSelectedEntry;

//		AddEntry("Wikipedia Homepage", "http://www.wikipedia.com", 100);
//		AddEntry("Google", "http://www.google.com", 200); 
//		AddEntry("Youtube", "http://www.youtube.com", 300); 
//		AddEntry("Portfolio", "http://www.acsaye.wordpress.com", -100); 
//		AddEntry("Amazon", "http://www.amazon.com", -200); 
//		AddEntry("Blackboard Lincoln", "http://www.blackboard.lincoln.ac.uk", 10000); 
	}
	
	void Update ()
	{

		// Loop through all entries in the linked list.
		LinkedListNode<Entry> entryNode = liveEntries.First;
		while (entryNode != null)
		{
			Rect entryRect = scrScreen.Calculate2DArea(entryNode.Value.BackgroundObject);

			// Check if the entry has left the top of the live feed.
			if (entryRect.yMin > scrScreen.Calculate2DArea(liveFeedBox).yMax)
			{
				// Destroy the node's gameobject.
				Destroy (entryNode.Value.gameObject);

				// Check if any entries are left.
				if (entryNode.Next != null)
				{
					// Go to the next entry.
					entryNode = entryNode.Next;

					// Delete the previous entry.
					liveEntries.Remove(entryNode.Previous);
				}
				else
				{
					liveEntries.Remove(entryNode);
					entryNode = null;
				}
			}
			else
			{
				// Move the entry upwards in the feed box.
				entryNode.Value.gameObject.transform.Translate(0.0f, liveEntrySpeed * Time.deltaTime, 0.0f);

				// Pass the live feed box's top and bottom to the shader.
				foreach (Renderer r in entryNode.Value.gameObject.GetComponentsInChildren<Renderer>())
				{
					r.material.SetFloat ("_ClipTop", liveFeedBox.position.y + liveFeedBox.localScale.y * 0.5f);
					r.material.SetFloat ("_ClipBottom", liveFeedBox.position.y - liveFeedBox.localScale.y * 0.5f);
				}

				// Go to the next entry.
				entryNode = entryNode.Next;
			}
		}
	}
}
