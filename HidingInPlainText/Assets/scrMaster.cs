using UnityEngine;
using System.Collections;

public class scrMaster : MonoBehaviour
{
	public static bool Loading { get; private set; }

	public static scrMaster Instance { get; private set; }

	public Material OpenGL;
	public GameObject NodeMaster;
	public GameObject BulletMaster;
	public GameObject EnemyMaster;
	public GameObject Brain;
	public GameObject Player;

	public AudioClip Music;

	// Use this for initialization
	void Start ()
	{
		Instance = this;

		Settings.Reset();

		guiText.pixelOffset -= new Vector2(0.0f, Screen.height * 0.3f);
		StartCoroutine(LoadAll());
	}

	IEnumerator LoadAll()
	{
		Loading = true;
		camera.enabled = true;
		GetComponent<GUILayer>().enabled = true;
		guiText.enabled = true;

		guiText.text = "Connecting To WikiMon WebSocket";
		yield return new WaitForEndOfFrame();
		while (scrWebSocketClient.Instance != null && !scrWebSocketClient.Instance.Connected)
		{
			if (scrWebSocketClient.Instance.Failed)
			{
				guiText.text = "Connection Failed";
				yield break;
			}
			yield return new WaitForEndOfFrame();
		}
		
		// Generate the pools.
		guiText.text = "Pooling Cores";
		yield return new WaitForSeconds(0.1f);
		yield return StartCoroutine(scrNodeMaster.Instance.LoadNodePool());
		
		guiText.text = "Pooling Fragments";
		yield return new WaitForSeconds(0.1f);
		yield return StartCoroutine(scrNodeMaster.Instance.LoadCubePool(6400));
		
		// Precompute the node and cube positions.
		guiText.text = "Precomputing Core Positions";
		yield return new WaitForSeconds(0.1f);
		scrNodeMaster.Instance.PrecomputeNodePositions();
		
		guiText.text = "Precomputing Fragment Positions";
		yield return new WaitForSeconds(0.1f);
		scrNode.PrecomputeCubePositions();

		guiText.text = "Pooling Projectiles";
		yield return new WaitForSeconds(0.1f);
		scrBulletMaster.Instance.LoadBulletPool(0, 1000);

		camera.enabled = false;
		GetComponent<GUILayer>().enabled = false;
		guiText.enabled = false;

		audio.clip = Music;
		audio.Play ();	

		Loading = false;
	}

	// Update is called once per frame
	void Update ()
	{
	
	}

	void OnPostRender()
	{
		OpenGL.SetPass(0);
		GL.PushMatrix ();
		GL.LoadOrtho();
		GL.MultMatrix(scrCamera.ScreenMatrix * Matrix4x4.TRS(new Vector3(0, 0.1f, 0), Quaternion.AngleAxis(Time.timeSinceLevelLoad * 10, Vector3.forward), Vector3.one));

		GL.Begin (GL.LINES);
		
		GL.Color(guiText.color);

		float k = 2.1f;
		float r = 0.08f + 0.005f * Mathf.Sin (Time.timeSinceLevelLoad);
		Vector3 vertex = new Vector3(r * k, 0.0f);
		for (int i = 1, vertexCount = 512, iterations = 10; i <= vertexCount; ++i)
		{
			GL.Vertex(vertex);

			float t = (float)i / (vertexCount / iterations) * 2 * Mathf.PI;
			vertex = new Vector3(r * (k + 1) * Mathf.Cos (t) - r * Mathf.Cos ((k + 1) * t),
			                     r * (k + 1) * Mathf.Sin (t) - r * Mathf.Sin ((k + 1) * t));
			                     
			
			GL.Vertex(vertex);
		}
		GL.End();


		GL.End ();
		GL.PopMatrix();
	}
}
