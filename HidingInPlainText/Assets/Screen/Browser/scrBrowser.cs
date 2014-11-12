using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrBrowser : MonoBehaviour
{
	const string DELETED_LINE = "  <td class=\"diff-deletedline\"><div>";
	const string ADDED_LINE =   "  <td class=\"diff-addedline\"><div>";

	private Transform 	browserBox;
	private TextMesh 	browserText;

	public bool IsLoading { get; private set; }
	

	// Use this for initialization
	void Start ()
	{
		browserBox = transform.Find ("Browser Box");
		browserText = transform.Find ("Browser Text").GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Set the browser text's clipping parameters.
		browserText.renderer.material.SetFloat ("_ClipTop", browserBox.position.y + browserBox.localScale.y * 0.5f);
		browserText.renderer.material.SetFloat ("_ClipBottom", browserBox.position.y - browserBox.localScale.y * 0.5f);
	}
	
	
	public void BeginLoad(string url)
	{
		if (IsLoading)
			StopCoroutine("Load");
		else
			StartCoroutine(Load(url));
	}

	private IEnumerator Load(string url)
	{
		IsLoading = true;

		// Start receiving data from the page.
		WWW page = new WWW(url);

		browserText.text = "Loading...";

		// Wait until the page has finished receiving data.
		int numLoopsPerFrame = 200;
		int numLoops = 0;
		while (!page.isDone)
		{
			++numLoops;
			if (numLoops == numLoopsPerFrame)
			{
				numLoops = 0;
				yield return new WaitForEndOfFrame();
			}
		}

		Debug.Log (page.text);
		browserText.text = "Parsing...";

		// Parse the edit.
		List<string> deletedLines = new List<string>();
		List<string> addedLines = new List<string>();
		string[] lines = page.text.Split('\n');
		int numChecksPerFrame = 200;	// After this many lines, the parsing will stop until the next frame. The higher this number, the quicker the parsing, but the laggier the game.
		int numChecks = 0;
		foreach (string line in lines)
		{
			if (line.StartsWith(DELETED_LINE))
			{
				deletedLines.Add (line);
			}
			else if (line.StartsWith(ADDED_LINE))
			{
				addedLines.Add (line);
			}

			++numChecks;
			if (numChecks == numChecksPerFrame)
			{
				numChecks = 0;
				yield return new WaitForEndOfFrame();
			}
		}		

		// Build the text.
		string text = "DELETED\n";

		foreach (string line in deletedLines)
			text += line + "\n";

		text += "\nADDED\n";

		foreach (string line in addedLines)
			text += line + "\n";

		// Set the text.
		browserText.text = text;

		// Loading has finished.
		IsLoading = false;
	}
}
