using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class scrPlayer : MonoBehaviour
{
	public Material OpenGL;

	public Vector2 AimPosition { get; private set; }
	public float AimRadius { get; private set; } // Fraction of the screen height that is the turning radius.
	public float AimDeadzone { get; private set; }
	public float TurnSpeed { get; private set; }

	public float Acceleration { get; private set; }
	public float SpeedLimit { get; private set; }

	public GameObject Ship { get; private set; }
	//Vector3 shipOffset = new Vector3(0.0f, -0.6f, 1.0f);

	void Start ()
	{
		Ship = transform.Find ("Ship").gameObject;
		//Ship.transform.parent = null;

		AimPosition = Vector2.zero;
		AimRadius = 0.5f;
		AimDeadzone = 0.1f;
		TurnSpeed = 100.0f;

		Acceleration = 100.0f;
		SpeedLimit = 50.0f;

		Camera.main.GetComponent<scrCamera>().PostRender += PostRender;
	}

	void Update()
	{

	}

	void FixedUpdate ()
	{
		Move ();
		Aim ();
		Shoot ();

		if (Input.GetKey(KeyCode.LeftShift))
		{
			rigidbody.velocity *= 50;
			Camera.main.fieldOfView = Mathf.Lerp (Camera.main.fieldOfView, 160, 0.1f);
		}
		else
		{
			Camera.main.fieldOfView = Mathf.Lerp (Camera.main.fieldOfView, 110, 0.1f);;
		}

	}

	void LateUpdate()
	{

	}

	void Shoot()
	{
		if (Input.GetButtonDown ("Primary Weapon"))
		{
			scrBulletMaster.Instance.Create (true, Ship.transform.position, Ship.transform.rotation, 
			                                 new scrBullet.BulletInfo(Ship.transform.Find ("Body").renderer.material.color,
			                         								  Ship.transform.Find ("Glow Small").renderer.material.GetColor("_TintColor"),
			                         								  10.0f, 0.1f, 100.0f, 1.0f));
		}
	}

	void Aim()
	{
		AimPosition += new Vector2(Input.GetAxis("Mouse X") / Screen.width, Input.GetAxis("Mouse Y") / Screen.height) * Settings.MouseSensitivity;

		if (AimPosition.magnitude > AimRadius)
		{
			AimPosition = AimPosition.normalized * AimRadius;
		}

		// Rotate towards the aiming position.
		Vector3 rotationToAdd = Vector3.Slerp (Vector3.zero, new Vector3(-AimPosition.y, AimPosition.x).normalized, (AimPosition.magnitude - AimDeadzone) / (AimRadius - AimDeadzone)) * TurnSpeed * Time.fixedDeltaTime;
		transform.Rotate (rotationToAdd);

		// Rotate through rotational inputs.
		transform.Rotate(Vector3.forward, Input.GetAxis("Rotational") * TurnSpeed * Time.fixedDeltaTime);

		// Rotate the ship.
		Quaternion targetRotation = Quaternion.LookRotation(new Vector3(AimPosition.x, AimPosition.y, -Camera.main.transform.localPosition.z * 0.5f)) *
									Quaternion.AngleAxis(Input.GetAxis("Rotational") * TurnSpeed * 0.2f, Vector3.forward);
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

		if (Input.GetAxis("Vertical") > 0)	 
			Ship.transform.GetComponentInChildren<ParticleSystem>().enableEmission = true;
		else
			Ship.transform.GetComponentInChildren<ParticleSystem>().enableEmission = false;
	}

	void PostRender()
	{
		OpenGL.SetPass(0);
		GL.PushMatrix ();
		GL.LoadOrtho();
		GL.MultMatrix(scrCamera.ScreenMatrix);

		Color colDeadZone = new Color(0.0f, 0.0f, 1.0f, 0.1f);
		Color colRadius = new Color(1.0f, 0.0f, 0.0f, 0.1f);
//		Color colArea = new Color(1.0f, 0.0f, 1.0f, 0.01f);
//		
//		GL.Begin(GL.TRIANGLES);
//
//			GL.Color (colArea);
//
//			// Draw the aim circle.
			Vector3 vertex = new Vector3(0.0f, AimRadius);
//			for (int i = 1, vertexCount = 32; i <= vertexCount; ++i)
//			{
//				GL.Vertex(vertex);
//
//				GL.Vertex (Vector3.zero);
//
//				vertex = new Vector3(AimRadius * Mathf.Sin ((float)i / vertexCount * 2 * Mathf.PI), AimRadius * Mathf.Cos ((float)i / vertexCount * 2 * Mathf.PI));	
//		
//				GL.Vertex(vertex);
//			}
//
//		GL.End();

		GL.Begin (GL.LINES);

			GL.Color(colRadius);

			// Draw the aim circle outline.
			vertex = new Vector3(0.0f, AimRadius);
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

//			// Draw lines towards the aim position.
//			for (int i = 0, vertexCount = 64; i < vertexCount; ++i)
//			{
//				GL.Color (colDeadZone);
//				GL.Vertex(new Vector3(AimDeadzone * Mathf.Sin ((float)i / vertexCount * 2 * Mathf.PI), AimDeadzone * Mathf.Cos ((float)i / vertexCount * 2 * Mathf.PI)));
//
//				GL.Color (Color.Lerp (colDeadZone, colRadius, AimPosition.magnitude / AimRadius));
//				GL.Vertex(AimPosition);
//			}

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
