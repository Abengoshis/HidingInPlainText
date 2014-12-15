using UnityEngine;
using System.Collections;

public class scrCube : MonoBehaviour
{

	public bool Infected { get; private set; }
	//public GameObject Glow;

	float infectionTransitionDuration = 2.0f;
	float infectionTransitionTimer = 0.0f;
	bool infectionTransitionCompleted = false;

	float expandDuration = 4.0f;
	float expandTimer = 0.0f;

	public void Infect()
	{
		Infected = true;
		renderer.material = scrNodeMaster.Instance.FragmentInfectedMaterial;
		//renderer.material.SetColor("_MainColor", scrNodeMaster.INFECTED_FRAGMENT_COLOUR);
		//renderer.material.SetColor("_GlowColor", scrNodeMaster.INFECTED_GLOW_COLOUR);
		//renderer.material.SetFloat("_Shininess", 1.0f);
	}

	public void Reset()
	{
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.zero;
		
		Infected = false;
		infectionTransitionCompleted = false;
		infectionTransitionTimer = 0.0f;
		renderer.material = scrNodeMaster.Instance.FragmentUninfectedMaterial;
		//Glow.renderer.material = scrNodeMaster.Instance.CubeGlowUninfectedMaterial;
		//renderer.material.SetColor("_MainColor", scrNodeMaster.UNINFECTED_FRAGMENT_COLOUR);
		//renderer.material.SetColor("_GlowColor", scrNodeMaster.UNINFECTED_GLOW_COLOUR);
		//renderer.material.SetFloat("_Shininess", 0.0f);
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
		if (expandTimer < expandDuration)
		{
			expandTimer += Time.deltaTime;
			transform.localScale = Vector3.Lerp (Vector3.zero, Vector3.one, expandTimer / expandDuration);
		}

		if (Infected)
		{
			if (!infectionTransitionCompleted)
			{
				infectionTransitionTimer += Time.deltaTime;
				if (infectionTransitionTimer >= infectionTransitionDuration)
				{
					infectionTransitionCompleted = true;
					renderer.material = scrNodeMaster.Instance.FragmentInfectedMaterial;
					//Glow.renderer.material = scrNodeMaster.Instance.CoreInfectedMaterial;
				}
				else
				{
					float transition = infectionTransitionTimer / infectionTransitionDuration;
					renderer.material.SetColor("_MainColor", Color.Lerp(scrNodeMaster.UNINFECTED_FRAGMENT_COLOUR, scrNodeMaster.INFECTED_FRAGMENT_COLOUR, transition));
					renderer.material.SetColor("_GlowColor", Color.Lerp(Color.clear, scrNodeMaster.INFECTED_CORE_COLOUR, transition));
	               //	Glow.renderer.material.color = Color.Lerp(scrNodeMaster.UNINFECTED_CORE_COLOUR, scrNodeMaster.INFECTED_CORE_COLOUR, transition);
					renderer.material.SetFloat("_Shininess", transition);
				}
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
