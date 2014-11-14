using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrBrowser : MonoBehaviour
{
	const string DELETED_LINE = "  <td class=\"diff-deletedline\"><div>";
	const string ADDED_LINE =   "  <td class=\"diff-addedline\"><div>";
	const int LOOPS_PER_FRAME = 200;

	private Transform 	browserBox;
	private TextMesh 	browserText;
	private float		browserTextInitialLocalY;	// Initial local y position of the browserText.
	private Transform	browserScrollbar;



	public bool IsLoading { get; private set; }


	private float extentsCoefficient = 2.2f;	// browserText's y extent multiplies with this for scrolling calculations.

	// Use this for initialization
	void Start ()
	{
		browserBox = transform.Find ("Browser Box");
		browserText = transform.Find ("Browser Text").GetComponent<TextMesh>();
		browserScrollbar = transform.Find ("Scrollbar");

		browserTextInitialLocalY = browserText.transform.localPosition.y;
		browserScrollbar.position = new Vector3(browserBox.position.x + browserBox.localScale.x * 0.5f - browserScrollbar.GetComponent<scrScrollbar>().ChildBackground.localScale.x * 0.5f,
		                                        browserBox.position.y, browserScrollbar.position.z);

	}
	
	// Update is called once per frame
	void Update ()
	{
		// First check if the browser text is too big to be fully contained within the browser box.
		if (browserText.renderer.bounds.extents.y * extentsCoefficient >= browserBox.transform.localScale.y)
		{
			// Move the browser text with the scrollbar by moving it by a proportion of its height compared to the height of the browser box, scaled with the scrollbar amount.
			browserText.transform.localPosition = new Vector3(browserText.transform.localPosition.x,
			                                                  browserTextInitialLocalY + browserText.renderer.bounds.extents.y * extentsCoefficient * 0.85f * browserScrollbar.GetComponent<scrScrollbar>().Amount,
			                                                  browserText.transform.localPosition.z);
		}

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

	// Huge function to load text from a url. It's huge because its all a coroutine to avoid lag, and I can't split it into separate coroutines because you can't return values with coroutines. =_=
	private IEnumerator Load(string url)
	{
		IsLoading = true;

		// Reset the scrollbar to its maximum size.
		browserScrollbar.GetComponent<scrScrollbar>().SetSize(1.0f);
		browserScrollbar.GetComponent<scrScrollbar>().ResetPosition();

		// Reset the position of the text.
		browserText.transform.localPosition = new Vector3(browserText.transform.localPosition.x, browserTextInitialLocalY, browserText.transform.localPosition.z);
		
		// Start receiving data from the page.
		WWW page = new WWW(url);

		browserText.text = "Loading...";
		
		// Checks whether to yield.
		int numLoops = 0;

		// Wait until the page has finished receiving data.
		while (!page.isDone)
		{
			if (++numLoops == LOOPS_PER_FRAME)
			{
				numLoops = 0;
				yield return new WaitForEndOfFrame();
			}
		}

		browserText.text = "Parsing...";

		// Parse the edit.
		List<string> deletedLines = new List<string>();
		List<string> addedLines = new List<string>();
		string[] lines = page.text.Split('\n');

		numLoops = 0;
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

			if (++numLoops == LOOPS_PER_FRAME)
			{
				numLoops = 0;
				yield return new WaitForEndOfFrame();
			}
		}		

		// Build the text.
		string text = "DELETED" + System.Environment.NewLine;

		numLoops = 0;
		foreach (string line in deletedLines)
			text += line + System.Environment.NewLine;

		text += System.Environment.NewLine + "ADDED" + System.Environment.NewLine;

		foreach (string line in addedLines)
			text += line + System.Environment.NewLine;

		// Strip the text of HTML.
		char[] plain = new char[text.Length];
		int plainLength = 0;
		bool tag = false;

		numLoops = 0;
		for (int i = 0; i < text.Length; ++i)
		{
			char c = text[i];
			if (c == '<')
				tag = true;
			else if (c == '>')
				tag = false;
			else if (!tag)
				plain[plainLength++] = c;
			
			if (++numLoops == LOOPS_PER_FRAME)
			{
				numLoops = 0;
				yield return new WaitForEndOfFrame();
			}
		}

		// Replace the text with the new plain text.
		text = new string(plain, 0, plainLength);

		// Set the text with wrapping.
		browserText.text = "";
		float width = browserBox.localScale.x * 0.5f - browserScrollbar.GetComponent<scrScrollbar>().ChildBackground.localScale.x;
		string[] words = text.Split(' ');
		string lastText = "";

		numLoops = 0;
		for (int i = 0; i < words.Length; ++i)
		{
			// Add the word to the text.
			browserText.text += words[i] + " ";

			// Check if the text has got too wide.
			if (browserText.renderer.bounds.extents.x > width)
			{
				// Set the text to the last stored text, with the new word on a new line.
				browserText.text = lastText.TrimEnd(' ') + System.Environment.NewLine + words[i] + " ";

				// Check if the text is still too long, and iteratively hyphenate whichever annoyingly long word is the culprit.
				while (browserText.renderer.bounds.extents.x > width)
				{
					browserText.text = lastText;

					// Loop through each character.
					for (int j = 0; j < words[i].Length; ++j)
					{
						// Add the character.
						browserText.text += words[i][j];

						// If its now too long, shift the second from last character onwards to a new line, replace the last character with a newline and replace the second from last character with a hyphen.
						if (browserText.renderer.bounds.extents.x > width)
						{

							browserText.text += '-' + browserText.text[browserText.text.Length - 3] + browserText.text[browserText.text.Length - 2] + words[i][j];
							browserText.text = browserText.text.Insert (browserText.text.Length - 6, "-" + System.Environment.NewLine);
							browserText.text = browserText.text.Remove (browserText.text.Length - 6, 3);

							// Set the last text.
							lastText = browserText.text;

							// Remove the first part of the word and add it to the text.
							words[i] = words[i].Remove (0, j + 1);

							browserText.text += words[i] + " ";
							break;
						}

						if (++numLoops == LOOPS_PER_FRAME)
						{
							numLoops = 0;
							yield return new WaitForEndOfFrame();
						}
					}

					if (++numLoops == LOOPS_PER_FRAME)
					{
						numLoops = 0;
						yield return new WaitForEndOfFrame();
					}
				}
			}

			lastText = browserText.text;

			if (++numLoops == LOOPS_PER_FRAME)
			{
				numLoops = 0;
				yield return new WaitForEndOfFrame();
			}
		}

		// Set the scrollbar's size if the text is larger than the box..
		if (browserText.renderer.bounds.extents.y * extentsCoefficient >= browserBox.localScale.y)
		{
			browserScrollbar.GetComponent<scrScrollbar>().SetSize(browserBox.localScale.y / (browserText.renderer.bounds.extents.y * extentsCoefficient));
			browserScrollbar.GetComponent<scrScrollbar>().ResetPosition();
		}
		
		// Loading has finished.
		IsLoading = false;
	}
}
