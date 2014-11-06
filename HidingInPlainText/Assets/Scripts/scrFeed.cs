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

		// Position the entry after the last entry in the list, if the list has any entries in it.
		if (liveEntries.Count != 0)
			entry.gameObject.transform.position = liveEntries.Last.Value.gameObject.transform.position - new Vector3(0.0f, liveEntrySpacing, 0.0f);
		else
			entry.gameObject.transform.position = liveEntryBox.position - new Vector3(0.0f, liveEntryBox.localScale.y * 0.5f + EntryPrefab.transform.localScale.y * 0.5f, 0) + new Vector3(0.0f, 0.0f, EntryPrefab.transform.position.z);

		// Add the entry to the end of the list.
		liveEntries.AddLast(entry);
	}

	void Start ()
	{
		parentScreen = transform.parent.GetComponent<scrScreen>();

		liveEntryBox = transform.Find ("Live Entry Box");
		liveEntries = new LinkedList<Entry>();
		liveEntrySpacing = EntryPrefab.transform.Find ("Background").localScale.y + 0.16f;
		liveEntrySpeed = 1f;

		selectedEntry = null;
		selectedEntryDestinationBox = transform.Find ("Selected Entry Box");
		selectedEntryOriginalPosition = Vector2.zero;
		selectedEntryMovementDuration = 1.0f;
		selectedEntryMovementTimer = 0.0f;

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

			// Check for the left mouse button.
			if ((parentScreen.MouseClicked & 1) > 0)
			{
				// Check whether the entry contains the mouse position.
				if (entryRect.Contains(parentScreen.MousePosition))
				{
					// Only one entry can be selected at a time.
					if (selectedEntry == null)
					{
						// Set the selected entry to the clicked one.
						selectedEntry = entryNode.Value;

						// Reset the movement timer. (this could be lowered to 0 when the selected item is accepted/rejected with a different animation).
						selectedEntryMovementTimer = 0;

						// Set the original position of the selected entry (required for animation).
						selectedEntryOriginalPosition = entryNode.Value.gameObject.transform.position;

						// Move to the next node.
						entryNode = entryNode.Next;

						// Remove the selected entry from the live feed.
						liveEntries.Remove (entryNode.Previous);

						// Skip the rest of the iteration.
						continue;
					}
					else
					{
						Debug.Log ("Can't select another entry yet.");
					}
				}
			}

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
