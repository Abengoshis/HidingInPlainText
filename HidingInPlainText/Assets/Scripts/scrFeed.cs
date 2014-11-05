using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Camera is set up so the 'resolution' of the screen is 16x9.

public class scrFeed : MonoBehaviour
{
	private class Entry
	{
		public GameObject gameObject;

		public string Title;
		public string URL;
		public int Size;

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

		public Rect GetRect()
		{
			return new Rect(gameObject.transform.position.x - gameObject.transform.localScale.x * 0.5f, gameObject.transform.position.y - gameObject.transform.localScale.y * 0.5f, gameObject.transform.localScale.x, gameObject.transform.localScale.y);
		}
	}

	public GameObject EntryPrefab;

	private LinkedList<Entry> entryData = new LinkedList<Entry>();
	private float entrySpacing;
	private float entryScrollSpeed;

	public void AddEntry(string title, string url, int size)
	{
		// Create a new entry.
		Entry entry = new Entry(Instantiate(EntryPrefab) as GameObject, title, url, size);

		// Position the entry after the last entry in the list, if the list has any entries in it.
		if (entryData.Count != 0)
			entry.gameObject.transform.position = entryData.Last.Value.gameObject.transform.position - new Vector3(0.0f, entrySpacing, 0.0f);

		// Add the entry to the end of the list.
		entryData.AddLast(entry);
	}
	

	void Start ()
	{
		entrySpacing = EntryPrefab.transform.Find ("Background").localScale.y + 0.16f;
		entryScrollSpeed = 0.1f;

		AddEntry("Wikipedia Homepage", "http://www.wikipedia.com", 100);
		AddEntry("Google", "http://www.google.com", 200); 
		AddEntry("Youtube", "http://www.youtube.com", 300); 
		AddEntry("Portfolio", "http://www.acsaye.wordpress.com", -100); 
		AddEntry("Amazon", "http://www.amazon.com", -200); 
		AddEntry("Blackboard Lincoln", "http://www.blackboard.lincoln.ac.uk", 10000); 
	}

	void Update ()
	{
		LinkedListNode<Entry> eNode = entryData.First;
		while (eNode != null)
		{
			// Replace this with something like FeedBox.Top. An ultimate PC script should have a Feed, a Browser etc. 
			if (eNode.Value.GetRect().yMin > GameObject.Find ("Screen").transform.position.y + GameObject.Find ("Screen").transform.localScale.y * 0.5f)
			{
				Destroy (eNode.Value.gameObject);

				LinkedListNode<Entry> eNodeToRemove = eNode;

				eNode = eNode.Next;
				
				entryData.Remove(eNodeToRemove);
			}
			else
			{
				// Move the entry upwards.
				eNode.Value.gameObject.transform.Translate(0.0f, entryScrollSpeed * Time.deltaTime, 0.0f);
				
				// Go to the next entry.
				eNode = eNode.Next;
			}
		}


//		// Feed needs to slowly scroll up and then jump back as soon as an entry is deleted, however the values must be such that as soon as item 0 goes out of the screen, item 0 is deleted, item 1 becomes the new item 0 and the feed scrolling value jumps back so that item 0 is where item 1 is. Technically the feed only moves up ONE space, then jumps back and the new item replaces the old.
//		for (int i = 0; i < entries.Count; ++i)
//		{
//			float offsetFromEntryZero = i * -entrySpacing;
//			float offsetFromDeletionTime = deletionCycleTimer;
//
//			float offset = offsetFromEntryZero + offsetFromDeletionTime;
//
//			entries[i].gameObject.transform.position = Vector3.Lerp (entries[i].gameObject.transform.position, new Vector3(entries[i].gameObject.transform.position.x, offset, entries[i].gameObject.transform.position.z), 0.1f);
//		}
	}
}
