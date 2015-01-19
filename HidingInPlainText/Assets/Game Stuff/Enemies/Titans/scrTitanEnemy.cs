using UnityEngine;
using System.Collections;

public class scrTitanEnemy : MonoBehaviour
{
	Transform[] wings = new Transform[4];

	public bool Active { get; private set; }

	// Use this for initialization
	void Start ()
	{
		Active = false;

		int index = 0;
		foreach (Transform t in transform.GetComponentsInChildren<Transform>())
		{
			if (t.name == "Wing")
			{
				wings[index] = t;
				++index;
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Active)
		{
			transform.Rotate (Vector3.up, Time.deltaTime, Space.Self);
		}
	}

	public void Activate()
	{
		StartCoroutine(ActivationSequence());
	}

	// Make the enemy activated.
	IEnumerator ActivationSequence()
	{

		// Move the wings out.
		for (float t = 0, tEnd = 10.0f; t < tEnd; t += Time.deltaTime)
		{
			foreach (Transform wing in wings)
			{
				if (wing.transform.localPosition.x > 0)
					wing.transform.localPosition = Vector3.Lerp (new Vector3(9.5f, 0.0f, 0.0f), Vector3.zero, Mathf.SmoothStep(0, 1, t / tEnd));
				else if (wing.transform.localPosition.x < 0)
					wing.transform.localPosition = Vector3.Lerp (new Vector3(-9.5f, 0.0f, 0.0f), Vector3.zero, Mathf.SmoothStep(0, 1, t / tEnd));
				else if (wing.transform.localPosition.z > 0)
					wing.transform.localPosition = Vector3.Lerp (new Vector3(0.0f, 0.0f, 9.5f), Vector3.zero, Mathf.SmoothStep(0, 1, t / tEnd));
				else if (wing.transform.localPosition.z < 0)
					wing.transform.localPosition = Vector3.Lerp (new Vector3(0.0f, 0.0f, -9.5f), Vector3.zero, Mathf.SmoothStep(0, 1, t / tEnd));
			}

			yield return new WaitForEndOfFrame();
		}

		// Finally, set the titan to be active.
		Active = true;
	}
}
