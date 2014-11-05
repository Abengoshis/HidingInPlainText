using UnityEngine;
using System.Collections;

public class scrRenderTexture : MonoBehaviour
{
	int frames = 0;
	int frameSkip = 1;
	
	void Start ()
	{

	}

	void Update()
	{
		StartCoroutine(CaptureScreen());
	}

	IEnumerator CaptureScreen ()
	{
		yield return new WaitForEndOfFrame();

		++frames;

		if (frames > frameSkip)
		{
			frames = 0;
		
			Texture2D capture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

			capture.ReadPixels(new Rect(0.0f, 0.0f, Screen.width, Screen.height), 0, 0, false);
			capture.Apply();

			renderer.material.mainTexture = capture;
		}
	}
}