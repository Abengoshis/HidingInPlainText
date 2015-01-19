using UnityEngine;
using System.Collections;

public class scrEnemy : MonoBehaviour
{
	public GameObject Ship;
	public GUIText ChildText;
	public GUITexture ChildTextBackground;
	public GameObject ExplosionPrefab;

	public Vector3 ViewPosition { get; private set; }
	protected float messageFadeDistance;

	protected GameObject owner;	// The object that spawned this enemy.

	
	public void Init(GameObject owner, string message)
	{
		this.owner = owner;
		ChildText.text = message;
		
		// Set the background size.
		Rect inset = ChildTextBackground.pixelInset;
		inset.width = ChildText.GetScreenRect().width;
		inset.x = -inset.width * 0.5f;
		ChildTextBackground.pixelInset = inset;
		
	}

	public void Free()
	{
		owner = scrPlayer.Instance.gameObject;
		scrEnemyMaster.Instance.Add(this);
	}

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	protected virtual void Update ()
	{
		// Check if in range to fade in the message.
		ChildText.gameObject.SetActive(true);
		float viewDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
		if (viewDistance < messageFadeDistance)
		{
			// Check if in front of the camera.
			ViewPosition = Camera.main.WorldToViewportPoint(transform.position);
			if (ViewPosition.z > 0)
			{
				ChildText.transform.position = ViewPosition;
				ChildText.transform.rotation = Quaternion.identity;
				
				ChildText.color = new Color(1, 1, 1, (1 - viewDistance / messageFadeDistance) * 2);
				ChildTextBackground.color = new Color(0, 0, 0, 0.25f * (1 - viewDistance / messageFadeDistance));
				
				ChildText.gameObject.SetActive(true);
			}
		}
	}

	protected Vector3 GetInterceptDirection(float bulletSpeed)
	{
		Vector3 playerPosition = scrPlayer.Instance.Ship.transform.position;
		Vector3 playerVelocity = scrPlayer.Instance.rigidbody.velocity;
		
		// Get direction.
		Vector3 direction = playerPosition - transform.position;
		
		// Get components of quadratic equation.
		float a = playerVelocity.x * playerVelocity.x + playerVelocity.y * playerVelocity.y + playerVelocity.z * playerVelocity.z - bulletSpeed * bulletSpeed;
		if (a == 0)
			a = 0.001f;
		float b = 2 * (playerVelocity.x * direction.x + playerVelocity.y * direction.y + playerVelocity.z * direction.z);
		float c = direction.x * direction.x + direction.y * direction.y + direction.z * direction.z;
		
		// Solve quadratic equation.
		bool solved = false;
		float q0 = 0.0f;
		float q1 = 0.0f;
		float eps = 0.00001f;
		if (Mathf.Abs(a) < eps)
		{
			if (Mathf.Abs(b) < eps)
			{
				if (Mathf.Abs(c) < eps)
				{
					solved = false;
				}
			}
			else
			{
				q0 = -c / b;
				q1 = -c / b;
				solved = false;
			}
		}
		else
		{
			float d = b * b - 4 * a * c;
			if (d >= 0)
			{
				d = Mathf.Sqrt(d);
				a = 2 * a;
				q0 = (-b + d) / a;
				q1 = (-b - d) / a;
				solved = true;
			}
		}
		
		// Find smallest positive solution.
		Vector3 solution = Vector2.zero;
		if (solved)
		{
			solved = false;
			float q = Mathf.Min(q0, q1);
			if (q < 0)
			{
				q = Mathf.Max(q0, q1);
			}
			if (q > 0)
			{
				solution = playerPosition + playerVelocity * q;
				solved = true;
			}
		}
		
		if (!solved)
		{
			// Fallback equation.
			float t = direction.magnitude / bulletSpeed;
			solution = playerPosition + playerVelocity * t;
		}
		
		return solution - transform.position;
	}

	public void Destroy()
	{
		// Explode.
		Instantiate(ExplosionPrefab, transform.position, Random.rotation);
	}
}
