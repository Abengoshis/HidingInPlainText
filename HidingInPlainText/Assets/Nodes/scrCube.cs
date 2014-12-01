using UnityEngine;
using System.Collections;

public class scrCube : MonoBehaviour
{
	public bool Infected { get; private set; }

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
	
	}

	void OnTriggerEnter(Collider other)
	{
		other.transform.root.GetComponentInChildren<scrBullet>().Expired = true;
		gameObject.SetActive(false);
	}
}
