using UnityEngine;
using System.Collections;

public class scrMissile : MonoBehaviour
{
	public GameObject ExplosionPrefab;
	public GameObject Target;
	Vector3 origin;
	float speed = 20.0f;
	float flightTimer = 0.0f;
	float flightDuration;
	bool expired;
	
	float previousDistance;	// Distance from the last frame.


	// Use this for initialization
	void Start ()
	{
		origin = transform.position;
		flightDuration = Vector3.Distance(Target.transform.position, transform.position);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!expired)
		{
			// Check whether to expire.
			if (flightTimer >= flightDuration || Target == null)
			{
				scrExplosion explosion = ((GameObject)Instantiate (ExplosionPrefab, transform.position, Random.rotation)).GetComponent<scrExplosion>();
				GetComponent<TrailRenderer>().autodestruct = true;

				if (Target != null)
				{
					Target.GetComponent<scrEnemy>().Destroy();
					Destroy (Target);
				}

				expired = true;
			}
			else
			{
				// Get the distance to the target.
				float distance = Vector3.Distance(Target.transform.position, transform.position);

				// Increase the flight timer normally.
				flightTimer += speed * Time.deltaTime;

				// Accelerate. Always hit.
				speed += Time.deltaTime;

				// Lerp the position towards the target - it must always hit.
				transform.position = Vector3.Lerp (origin, Target.transform.position, flightTimer / flightDuration);
			}
		}
	}
}
