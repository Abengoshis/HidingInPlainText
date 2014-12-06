using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(Rigidbody))]
public class scrEnemy : MonoBehaviour
{
	public GameObject Ship;
	public Transform ChildCore;
	public Transform ChildGlowSmall;
	public Transform ChildGlowLarge;

	const float VISION_ANGLE = 60.0f;
	const float VISION_DISTANCE = 200.0f;

	const float BULLET_SPEED = 100.0f;

	const float STEER_MAX = 30.0f;
	const float SPEED_MAX = 30.0f;
	
	const float FLOCK_SEPARATION = 10.0f;
	const float FLOCK_SEPARATION_RADIUS = 5.0f;
	const float FLOCK_ALIGNMENT = 1.0f;
	const float FLOCK_ALIGNMENT_RADIUS = 10.0f;
	const float FLOCK_COHESION = 1.0f;
	const float FLOCK_COHESION_RADIUS = 10.0f;
	const int 	FLOCK_LIMIT = 10;

	GameObject flockTarget = null;
	float shootDelay = 5.0f;
	float shootTimer = 0.0f;



	GameObject owner;	// The object that these enemies protect.

	public bool Expired { get; private set; }

	
	public void Init(GameObject owner)
	{
		this.owner = owner;
	}

	Vector3 GetInterceptDirection()
	{
		Vector3 playerPosition = scrPlayer.Instance.Ship.transform.position;
		Vector3 playerVelocity = scrPlayer.Instance.rigidbody.velocity;

		// Get direction.
		Vector3 direction = playerPosition - transform.position;

		// Get components of quadratic equation.
		float a = playerVelocity.x * playerVelocity.x + playerVelocity.y * playerVelocity.y + playerVelocity.z * playerVelocity.z - BULLET_SPEED * BULLET_SPEED;
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
			float t = direction.magnitude / BULLET_SPEED;
			solution = playerPosition + playerVelocity * t;
		}

		return solution - transform.position;
	}
	
	void Shoot()
	{
		shootTimer += Time.deltaTime;
		if (shootTimer >= shootDelay)
		{
			shootTimer = 0.0f;
			
			Vector3 direction = scrPlayer.Instance.Ship.transform.position - transform.position;
			float distance = direction.magnitude;
			
			// Do not shoot if the player is too far.
			if (distance == 0 || distance > VISION_DISTANCE)
				return;
			
			Vector3 aimDirection = GetInterceptDirection();

			// Shoot if the player is within the vision cone.
			if (Vector3.Angle (transform.forward, direction) < VISION_ANGLE)
			{
				scrBulletMaster.Instance.Create (false, transform.position, aimDirection, new scrBullet.BulletInfo(ChildCore.renderer.material.color,
				                                                                                                   ChildGlowLarge.renderer.material.GetColor("_TintColor"),
				                                                                                                   10.0f, 0.1f, BULLET_SPEED, 1.0f));
			}
		}
	}
	
	void Flock()
	{
		if (flockTarget != null)
		{
			// Steer towards the target.
			Vector3 direction = flockTarget.transform.position - transform.position;
			float distance = direction.magnitude;
			if (distance > 0)
			{
				Vector3 steer = direction.normalized * STEER_MAX * distance / 50 - rigidbody.velocity;
				rigidbody.AddForce(steer, ForceMode.Acceleration);
			}
		}

		// Add a random steer.
		rigidbody.AddForce(Random.Range (-STEER_MAX, STEER_MAX), Random.Range (-STEER_MAX, STEER_MAX), Random.Range (-STEER_MAX, STEER_MAX), ForceMode.Acceleration);

		Vector3 separation = Vector3.zero;
		int separationCount = 0;
		Vector3 alignment = Vector3.zero;
		int alignmentCount = 0;
		Vector3 cohesion = Vector3.zero;
		int cohesionCount = 0;
		
		foreach (GameObject enemy in scrEnemyMaster.Instance.Enemies)
		{
			float distance = Vector3.Distance(enemy.transform.position, transform.position);
			if (distance == 0)
				continue;

			if (separationCount < FLOCK_LIMIT && distance < FLOCK_SEPARATION_RADIUS)
			{		
				// Accumulate separation.
				Vector3 direction = transform.position - enemy.transform.position;
				direction.Normalize();
				direction /= distance;
				separation += direction;
				++separationCount;
			}
			
			if (alignmentCount < FLOCK_LIMIT && distance  < FLOCK_ALIGNMENT_RADIUS)
			{
				// Accumulate alignment.
				alignment += enemy.rigidbody.velocity;
				++alignmentCount;
			}
			
			if (cohesionCount < FLOCK_LIMIT && distance < FLOCK_COHESION_RADIUS)
			{
				// Accumulate cohesion.
				cohesion += enemy.transform.position;
				++cohesionCount;
			}
		}
		
		// Average the forces.
		if (separationCount != 0)
		{
			separation /= separationCount;
			if (separation.magnitude > STEER_MAX)
				separation = separation.normalized * STEER_MAX;

			rigidbody.AddForce(separation * FLOCK_SEPARATION, ForceMode.Acceleration);
		}

		if (alignmentCount != 0)
		{
			alignment /= alignmentCount;
			if (alignment.magnitude > STEER_MAX)
				alignment = alignment.normalized * STEER_MAX;

			rigidbody.AddForce(alignment * FLOCK_ALIGNMENT, ForceMode.Acceleration);
		}

		if (cohesionCount != 0)
		{
			cohesion /= cohesionCount;
			if (cohesion.magnitude > STEER_MAX)
				cohesion = cohesion.normalized * STEER_MAX;

			// Steer towards cohesion point.
			Vector3 direction = transform.position - cohesion;
			float distance = direction.magnitude;
			if (distance > 0)
			{
				Vector3 steer = direction.normalized * STEER_MAX - rigidbody.velocity;
				if (steer.magnitude > STEER_MAX)
					steer = steer.normalized * STEER_MAX;
				
				rigidbody.AddForce(steer, ForceMode.Acceleration);
			}
		}
	}

	// Use this for initialization
	void Start ()
	{
		ChildCore.renderer.material.color = new Color(1.0f, 0.5f, 0.0f, 1.0f);
		ChildGlowSmall.renderer.material.SetColor("_TintColor", new Color(1.0f, 0.5f, 0.0f, ChildGlowSmall.renderer.material.GetColor("_TintColor").a));
		ChildGlowLarge.renderer.material.SetColor("_TintColor", new Color(1.0f, 0.5f, 0.0f, ChildGlowLarge.renderer.material.GetColor("_TintColor").a));

		// Start with a random velocity;
		rigidbody.AddForce(Random.Range (-SPEED_MAX, SPEED_MAX), Random.Range (-SPEED_MAX, SPEED_MAX), Random.Range (-SPEED_MAX, SPEED_MAX), ForceMode.VelocityChange);

		flockTarget = scrPlayer.Instance.gameObject;
	
		shootTimer = Random.Range (0.0f, shootDelay);
	}
	
	// Update is called once per frame
	void Update ()
	{
		Flock ();
		Shoot ();

		transform.forward = Vector3.Lerp (transform.forward, rigidbody.velocity, 0.25f);

		if (rigidbody.velocity.magnitude > SPEED_MAX)
			rigidbody.velocity = rigidbody.velocity.normalized * SPEED_MAX;

	}
}
