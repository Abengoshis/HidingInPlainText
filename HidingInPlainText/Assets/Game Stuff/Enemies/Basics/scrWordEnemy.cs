using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(Rigidbody))]
public class scrWordEnemy : scrEnemy
{
	const float VISION_ANGLE = 60.0f;
	const float VISION_DISTANCE = 200.0f;

	const float STEER_MAX = 30.0f;
	const float SPEED_MAX = 30.0f;
	
	const float FLOCK_SEPARATION = 10.0f;
	const float FLOCK_SEPARATION_RADIUS = 5.0f;
	const float FLOCK_ALIGNMENT = 1.0f;
	const float FLOCK_ALIGNMENT_RADIUS = 10.0f;
	const float FLOCK_COHESION = 1.0f;
	const float FLOCK_COHESION_RADIUS = 10.0f;
	const int FLOCK_LIMIT = 10;

	GameObject flockTarget = null;
	float shootDelay = 5.0f;
	float shootTimer = 0.0f;

	scrBullet.BulletInfo bulletInfo = new scrBullet.BulletInfo(10.0f, 0.1f, 50.0f, 1.0f);

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
			
			Vector3 aimDirection = GetInterceptDirection(bulletInfo.Speed);

			// Shoot if the player is within the vision cone.
			if (Vector3.Angle (transform.forward, direction) < VISION_ANGLE)
			{
				scrBulletMaster.Instance.Create (transform.position, aimDirection, bulletInfo);
			}
		}
	}
	
	void Flock()
	{
		if (owner == null)
		{
			Debug.Log ("NO OWNER!?");
			return;
		}

		// Add a steer towards the owner which gets stronger the further away the enemy is from the owner.
		Vector3 directionToOwner = owner.transform.position - transform.position;
		rigidbody.AddForce(directionToOwner);

		List<scrEnemy> buddies;
		if (owner.GetComponent<scrNode>() != null)
		{
			buddies = owner.GetComponent<scrNode>().Enemies;
		}
		else
		{
			buddies = scrEnemyMaster.Instance.FreeEnemies;
		}

		if (buddies.Count == 0)
			return;

		if (flockTarget != null)
		{
			// Steer towards the target.	
			Vector3 direction = flockTarget.transform.position - transform.position;
			float distance = direction.magnitude;
			if (distance > 0 && distance <= VISION_DISTANCE)
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

		foreach (scrEnemy enemy in buddies)
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
		messageFadeDistance = 50.0f;
		flockTarget = scrPlayer.Instance.gameObject;	
		shootTimer = Random.Range (0.0f, shootDelay);
	}
	
	// Update is called once per frame
	protected override void Update ()
	{
		Flock ();
		Shoot ();

		transform.forward = Vector3.Lerp (transform.forward, rigidbody.velocity, 0.25f);

		if (rigidbody.velocity.magnitude > SPEED_MAX)
			rigidbody.velocity = rigidbody.velocity.normalized * SPEED_MAX;

		base.Update();
	}
}
