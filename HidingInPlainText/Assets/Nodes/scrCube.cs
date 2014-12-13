using UnityEngine;
using System.Collections;

public class scrCube : MonoBehaviour
{

	public bool Infected { get; private set; }

	float infectionTransitionDuration = 2.0f;
	float infectionTransitionTimer = 0.0f;
	bool infectionTransitionCompleted = false;


	public void Infect()
	{
		Infected = true;
		renderer.material.SetColor("_MainColor", scrNodeMaster.INFECTED_MAIN_COLOUR);
		renderer.material.SetColor("_GlowColor", scrNodeMaster.INFECTED_GLOW_COLOUR);
		renderer.material.SetFloat("_Shininess", 1.0f);
	}

	public void Reset()
	{
		transform.rotation = Quaternion.identity;
		
		Infected = false;
		infectionTransitionCompleted = false;
		infectionTransitionTimer = 0.0f;
		renderer.material.SetColor("_MainColor", scrNodeMaster.UNINFECTED_MAIN_COLOUR);
		renderer.material.SetColor("_GlowColor", scrNodeMaster.UNINFECTED_GLOW_COLOUR);
		renderer.material.SetFloat("_Shininess", 0.0f);
	}

	// Use this for initialization
	void Start ()
	{
		Reset ();

		// Start inactive.
		gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Infected)
		{
			if (!infectionTransitionCompleted)
			{
				infectionTransitionTimer += Time.deltaTime;
				if (infectionTransitionTimer >= infectionTransitionDuration)
				{
					infectionTransitionCompleted = true;
				}

				float transition = infectionTransitionTimer / infectionTransitionDuration;
				renderer.material.SetColor("_MainColor", Color.Lerp(scrNodeMaster.UNINFECTED_MAIN_COLOUR, scrNodeMaster.INFECTED_MAIN_COLOUR, transition));
               	renderer.material.SetColor("_GlowColor",  Color.Lerp(scrNodeMaster.UNINFECTED_GLOW_COLOUR, scrNodeMaster.INFECTED_GLOW_COLOUR, transition));
				renderer.material.SetFloat("_Shininess", transition);
			}
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.transform.root.GetComponentInChildren<scrBullet>() != null)
		{
			other.transform.root.GetComponentInChildren<scrBullet>().Expired = true;
			gameObject.SetActive(false);
		}
	}
}
