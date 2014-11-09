using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Camera is set up so the 'resolution' of the screen is 16x9.

public class scrFeed : MonoBehaviour
{
	private class Entry
	{
		public GameObject gameObject;
		
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
			this.gameObject.transform.Find ("Title").GetComponent<TextMesh>().text = Title;
			this.gameObject.transform.Find ("Size").GetComponent<TextMesh>().text = Size > 0 ? "+" + Size.ToString() : Size.ToString();	// - values already have a '-' before them when converted to a string.
		}

	}

	public GameObject EntryPrefab;

	private scrScreen parentScreen;

	private Transform 			liveEntryBox;
	private LinkedList<Entry> 	liveEntries;
	private float 				liveEntrySpacing;
	private float 				liveEntrySpeed;

	private Entry 		selectedEntry;
	private Transform 	selectedEntryDestinationBox;
	private Vector3 	selectedEntryOriginalPosition;
	private float 		selectedEntryMovementDuration;
	private float 		selectedEntryMovementTimer;
	

	public void AddEntry(string title, string url, int size)
	{
		// Create a new entry.
		Entry entry = new Entry(Instantiate(EntryPrefab) as GameObject, title, url, size);

		// Set the entry's selection event.
		entry.gameObject.GetComponentInChildren<scrSelectable>().OnSelect += SelectEntry;
		entry.gameObject.GetComponentInChildren<scrSelectable>().parentScreen = parentScreen;

		// Position the entry after the last entry in the list, if the list has any entries in it. PERHAPS CHANGE THIS SO THEY ARE ALWAYS IN FIXED INCREMENTS.
		if (liveEntries.Count == 0 || parentScreen.Calculate2DArea(liveEntries.Last.Value.gameObject.transform.Find ("Background")).yMin > parentScreen.Calculate2DArea(liveEntryBox).yMin)
		{
			// Place underneath the bottom of the feed box.
			entry.gameObject.transform.position = liveEntryBox.position - new Vector3(0.0f, liveEntryBox.localScale.y * 0.5f + liveEntrySpacing, -EntryPrefab.transform.position.z);
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
				if (entry.gameObject.GetComponentInChildren<scrSelectable>().Clicked)
				{
					// Set the selected entry.
					selectedEntry = entry;
					
					// Reset the movement timer. (this could be lowered to 0 when the selected item is accepted/rejected with a different animation).
					selectedEntryMovementTimer = 0;
					
					// Set the original position of the selected entry (required for animation).
					selectedEntryOriginalPosition = selectedEntry.gameObject.transform.position;

					// Remove the entry from the live feed.
					liveEntries.Remove (selectedEntry);

					WWW www = new WWW(selectedEntry.URL);
					while(!www.isDone);	// yield this

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
		parentScreen = transform.parent.GetComponent<scrScreen>();

		// Set up the live entry box.
		liveEntryBox = transform.Find ("Live Entry Box");
		liveEntries = new LinkedList<Entry>();
		liveEntrySpacing = EntryPrefab.transform.Find ("Background").localScale.y + 0.16f;
		liveEntrySpeed = 0.2f;	// Maybe allow players to speed this up.

		// Set up the selected entry box.
		selectedEntry = null;
		selectedEntryDestinationBox = transform.Find ("Selected Entry Box");
		selectedEntryOriginalPosition = Vector2.zero;
		selectedEntryMovementDuration = 1.0f;
		selectedEntryMovementTimer = 0.0f;

		// Add to button selection events.
		parentScreen.AcceptButton.GetComponentInChildren<scrSelectable>().OnSelect += AcceptSelectedEntry;
		parentScreen.RejectButton.GetComponentInChildren<scrSelectable>().OnSelect += RejectSelectedEntry;

//		AddEntry("Wikipedia Homepage", "http://www.wikipedia.com", 100);
//		AddEntry("Google", "http://www.google.com", 200); 
//		AddEntry("Youtube", "http://www.youtube.com", 300); 
//		AddEntry("Portfolio", "http://www.acsaye.wordpress.com", -100); 
//		AddEntry("Amazon", "http://www.amazon.com", -200); 
//		AddEntry("Blackboard Lincoln", "http://www.blackboard.lincoln.ac.uk", 10000); 
	}
	
	void Update ()
	{


		// Move the selected entry to the selected entry box.
		if (selectedEntry != null)
		{
			if (selectedEntryMovementTimer >= selectedEntryMovementDuration)
				selectedEntryMovementTimer = selectedEntryMovementDuration;
			else
				selectedEntryMovementTimer += Time.deltaTime;

			selectedEntry.gameObject.transform.position = Vector3.Lerp (selectedEntryOriginalPosition, selectedEntryDestinationBox.position, Mathf.SmoothStep(0.0f, 1.0f, selectedEntryMovementTimer / selectedEntryMovementDuration));
		}

		// Loop through all entries in the linked list.
		LinkedListNode<Entry> entryNode = liveEntries.First;
		while (entryNode != null)
		{
			Rect entryRect = parentScreen.Calculate2DArea(entryNode.Value.gameObject.transform.Find ("Background"));

			// Check if the entry has left the top of the live feed.
			if (entryRect.yMin > parentScreen.Calculate2DArea(liveEntryBox).yMax)
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
				
				// Go to the next entry.
				entryNode = entryNode.Next;
			}
		}
	}
}
