using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(Rigidbody))]
public class scrPlayer : MonoBehaviour
{
	public static scrPlayer Instance { get; private set; }

	public Material OpenGL;
	public GameObject MissilePrefab;

	public Vector2 AimPosition { get; private set; }
	public float AimRadius { get; private set; } // Fraction of the screen height that is the turning radius.
	public float AimDeadzone { get; private set; }
	public Ray AimRay { get; private set; }
	public float TurnSpeed { get; private set; }

	public float Acceleration { get; private set; }
	public float SpeedLimit { get; private set; }

	// Stuff for things the player looks at.
	public const float SCAN_DISTANCE = 50.0f;
	int scanLayer;
	GameObject nodeLookedAt;	// When node looked at changes, gui node info updates. Node infection updates constantly.

	public const int LOCKED_ENEMIES_MAX = 16;
	public const float ENEMY_SEARCH_DISTANCE = 150.0f;
	public List<GameObject> EnemiesLocked = new List<GameObject>();

	//Vector3 lastMousePosition;

	public GameObject Ship { get; private set; }
	public GameObject Core { get; private set; }
	//Vector3 shipOffset = new Vector3(0.0f, -0.6f, 1.0f);

	void Start ()
	{
		Instance = this;

		Ship = transform.Find ("Ship").gameObject;
		Core = Ship.transform.Find ("Core").gameObject;
		scanLayer = LayerMask.NameToLayer("Node");

		AimPosition = Vector2.zero;
		AimRadius = 0.5f;
		AimDeadzone = 0.1f;
		TurnSpeed = 100.0f;

		Acceleration = 50.0f;
		SpeedLimit = 80.0f;

		Camera.main.GetComponent<scrCamera>().PostRender += PostRender;
		Camera.main.transform.parent = transform;
		Camera.main.transform.localPosition = new Vector3(0.0f, 0.0f, -1.2f);
		Camera.main.transform.localRotation = Quaternion.identity;
		Camera.main.camera.enabled = true;

		Screen.lockCursor = true;
		//lastMousePosition = Input.mousePosition;
	}

	void Update()
	{
		if (scrMaster.Loading)
			return;

		Aim ();
		Select();


		Camera.main.fieldOfView = Mathf.Lerp (95, 110, transform.InverseTransformDirection(rigidbody.velocity).z / SpeedLimit);

		if (Input.GetKey(KeyCode.Escape))
			Application.Quit();
		
		if (Input.GetKey(KeyCode.LeftShift))
			rigidbody.velocity *= 10;
	}


	void FixedUpdate ()
	{
		if (scrMaster.Loading)
			return;

		Look ();
		Move ();
	}

	void Aim()
	{
		//Vector3 diff = Input.mousePosition - lastMousePosition;
		//AimPosition += new Vector2(diff.x / Screen.width, diff.y / Screen.height) * Settings.MouseSensitivity;
		//lastMousePosition = Input.mousePosition;

		AimPosition += new Vector2(Input.GetAxis("Mouse X") / Screen.width, Input.GetAxis("Mouse Y") / Screen.height) * Settings.MouseSensitivity;

		if (AimPosition.magnitude > AimRadius)
		{
			AimPosition = AimPosition.normalized * AimRadius;
		}

		AimRay = Camera.main.ViewportPointToRay((Vector3)(AimPosition + new Vector2(0.5f, 0.5f)));
	}

	void Select()
	{
		RaycastHit hit;

		// Look for a node.
		float nodeSearchDistance = 200.0f;
		
		// Look where the player is looking.
		if (Physics.SphereCast(AimRay, 6, out hit, nodeSearchDistance, 1 << scrNodeMaster.Instance.NodePrefab.layer))
		{
			nodeLookedAt = hit.transform.gameObject;
			scrGUI.Instance.SetNodeInfo(nodeLookedAt.GetComponent<scrNode>().Data);
		}
		
		// Update the gui node info position.
		if (nodeLookedAt != null)
		{
			scrGUI.Instance.SetNodeInfoPosition(nodeLookedAt.transform.position, nodeSearchDistance);
			scrGUI.Instance.SetNodeInfection(nodeLookedAt.GetComponent<scrNode>().GetInfectionAmount());
		}
		else
		{
			scrGUI.Instance.HideNodeInfo();
		}

		// ----

		if (Input.GetButton("Primary Weapon"))
		{
			// Look for an enemy.
			if (EnemiesLocked.Count < LOCKED_ENEMIES_MAX)
			{
				if (Physics.SphereCast(AimRay, 2, out hit, ENEMY_SEARCH_DISTANCE, 1 << scrEnemyMaster.EnemyLayer))
				{
					if (!EnemiesLocked.Contains(hit.transform.gameObject))
				    {
						EnemiesLocked.Add(hit.transform.gameObject);

						// Play lock sound.


					}
				}
			}
		}
		else if (Input.GetButtonUp("Primary Weapon"))
		{
			// Send out missiles to all locked enemies.
			foreach (GameObject e in EnemiesLocked)
			{
				GameObject missile = (GameObject)Instantiate(MissilePrefab, transform.position, Quaternion.identity);
				missile.GetComponent<scrMissile>().Target = e;	
			}

			// Clear all enemies.
			EnemiesLocked.Clear();
		}

		// Unlock enemies that are out of the field of view.
		for (int i = EnemiesLocked.Count - 1; i >= 0; --i)
		{
			if (EnemiesLocked[i] == null)
			{
				EnemiesLocked.RemoveAt(i);

				// Play unlock sound.

				continue;
			}

			Vector3 viewportPoint = Camera.main.WorldToViewportPoint(EnemiesLocked[i].transform.position);
			if (viewportPoint.z > ENEMY_SEARCH_DISTANCE)
			{
				EnemiesLocked.RemoveAt(i);

				// Play unlock sound.

				continue;
			}
		}

	}

	void Look()
	{
		// Rotate towards the aiming position.
		Vector3 rotationToAdd = Vector3.Slerp (Vector3.zero, new Vector3(-AimPosition.y, AimPosition.x).normalized, (AimPosition.magnitude - AimDeadzone) / (AimRadius - AimDeadzone)) * TurnSpeed * Time.fixedDeltaTime;
		transform.Rotate (rotationToAdd);

		// Rotate through rotational inputs.
		transform.Rotate(Vector3.forward, Input.GetAxis("Rotational") * TurnSpeed * Time.fixedDeltaTime);

		// Rotate the ship.
		Quaternion targetRotation = Quaternion.LookRotation(new Vector3(AimPosition.x, AimPosition.y, -Camera.main.transform.localPosition.z * 0.5f)) *
									Quaternion.AngleAxis((Input.GetAxis("Rotational") - Input.GetAxis("Horizontal")) * TurnSpeed * 0.2f, Vector3.forward);
		Ship.transform.localRotation = Quaternion.RotateTowards(Ship.transform.localRotation, targetRotation, Quaternion.Angle(Ship.transform.localRotation, targetRotation) * 10 * Time.fixedDeltaTime);
	}

	void Move()
	{
		Vector3 velocityToAdd = Input.GetAxis("Vertical") * Ship.transform.forward + Input.GetAxis("Horizontal") * Ship.transform.right;
		velocityToAdd.Normalize();
		velocityToAdd *= Acceleration * Time.fixedDeltaTime;
		
		rigidbody.AddForce(velocityToAdd, ForceMode.VelocityChange);
		if (rigidbody.velocity.magnitude > SpeedLimit)
		{
			rigidbody.velocity = rigidbody.velocity.normalized * SpeedLimit;
		}
	}

	void PostRender()
	{
		OpenGL.SetPass(0);
		GL.PushMatrix ();

		GL.LoadOrtho();
		GL.MultMatrix(scrCamera.ScreenMatrix);

		Color colDeadZone = new Color(0.0f, 0.0f, 1.0f, 0.1f);
		Color colRadius = new Color(1.0f, 0.0f, 0.0f, 0.05f);

		GL.Begin (GL.LINES);

			GL.Color(colRadius);

			// Draw the aim circle outline.
			Vector3 vertex = new Vector3(0.0f, AimRadius);
			for (int i = 1, vertexCount = 32; i <= vertexCount; ++i)
			{
				GL.Vertex(vertex);
				
				vertex = new Vector3(AimRadius * Mathf.Sin ((float)i / vertexCount * 2 * Mathf.PI), AimRadius * Mathf.Cos ((float)i / vertexCount * 2 * Mathf.PI));	
				
				GL.Vertex(vertex);
			}
			
			GL.Color (colDeadZone);

			// Draw the aim deadzone circle.
			vertex = new Vector3(0.0f, AimDeadzone);
			for (int i = 1, vertexCount = 32; i <= vertexCount; ++i)
			{
				GL.Vertex(vertex);
				
				vertex = new Vector3(AimDeadzone * Mathf.Sin ((float)i / vertexCount * 2 * Mathf.PI), AimDeadzone * Mathf.Cos ((float)i / vertexCount * 2 * Mathf.PI));	
				
				GL.Vertex(vertex);
			}

			// Draw the cursor.
			Color colCursorCentre = Color.white;
			colCursorCentre.a = 1.0f;	
			GL.Color (colCursorCentre);
			GL.Vertex(new Vector3(AimPosition.x, 		 AimPosition.y - 0.01f));
			GL.Vertex(new Vector3(AimPosition.x, 		 AimPosition.y + 0.01f));
			GL.Vertex(new Vector3(AimPosition.x - 0.01f, AimPosition.y));
			GL.Vertex(new Vector3(AimPosition.x + 0.01f, AimPosition.y));
			Color colCursorOuter = Color.Lerp (colDeadZone, colRadius, AimPosition.magnitude / AimRadius);
			colCursorOuter.a = 1.0f;
			GL.Color (colCursorOuter);
			GL.Vertex(new Vector3(AimPosition.x - 0.02f, AimPosition.y - 0.02f));
			GL.Vertex(new Vector3(AimPosition.x - 0.03f, AimPosition.y - 0.02f));
			GL.Vertex(new Vector3(AimPosition.x - 0.03f, AimPosition.y - 0.02f));
			GL.Vertex(new Vector3(AimPosition.x - 0.03f, AimPosition.y + 0.02f));
			GL.Vertex(new Vector3(AimPosition.x - 0.03f, AimPosition.y + 0.02f));
			GL.Vertex(new Vector3(AimPosition.x - 0.02f, AimPosition.y + 0.02f));
			GL.Vertex(new Vector3(AimPosition.x + 0.02f, AimPosition.y - 0.02f));
			GL.Vertex(new Vector3(AimPosition.x + 0.03f, AimPosition.y - 0.02f));
			GL.Vertex(new Vector3(AimPosition.x + 0.03f, AimPosition.y - 0.02f));
			GL.Vertex(new Vector3(AimPosition.x + 0.03f, AimPosition.y + 0.02f));
			GL.Vertex(new Vector3(AimPosition.x + 0.03f, AimPosition.y + 0.02f));
			GL.Vertex(new Vector3(AimPosition.x + 0.02f, AimPosition.y + 0.02f));

		GL.End ();


		GL.PopMatrix();
	}

}
