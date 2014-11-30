using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class scrBullet : MonoBehaviour
{
	public struct BulletInfo
	{
		public Color CoreColour;
		public Color GlowColour;
		public float Length;
		public float Girth;
		public float Speed;
		public float Damage;

		public BulletInfo(Color core, Color glow, float length, float girth, float speed, float damage)
		{
			CoreColour = core;
			GlowColour = glow;
			Length = length;
			Girth = girth;
			Speed = speed;
			Damage = damage;
		}
	}

	const float DISTANCE_MAX = 1000.0f;
	public float DistanceTravelled {get; private set; }
	public bool Expired { get; private set; }
	public BulletInfo Information { get; private set; }

	public Transform ChildCore { get; private set; }
	public Transform ChildGlowSmall { get; private set; }
	public Transform ChildGlowLarge { get; private set; }

	void Init(Vector3 position, BulletInfo information)
	{
		Information = information;
		DistanceTravelled = 0.0f;
		Expired = false;
		
		ChildCore.localScale = new Vector3(information.Girth, information.Girth, information.Length);
		ChildGlowSmall.localScale = new Vector3(information.Girth * 1.25f, information.Girth * 1.25f, information.Length + information.Girth * 0.25f);
		ChildGlowLarge.localScale = new Vector3(information.Girth * 1.5f, information.Girth * 1.5f, information.Length + information.Girth * 0.5f);
		
		ChildCore.renderer.material.color = information.CoreColour;
		ChildGlowSmall.renderer.material.SetColor("_TintColor", information.GlowColour);
		ChildGlowLarge.renderer.material.SetColor("_TintColor", information.GlowColour);

		transform.position = position + transform.forward * ChildCore.localScale.z * 0.5f;
		
		rigidbody.WakeUp();
		rigidbody.velocity = transform.forward * information.Speed;
	}

	public void Init(Vector3 position, Vector3 direction, BulletInfo information)
	{
		transform.LookAt(transform.position + direction, Vector3.up);
		Init (position, information);
	}

	public void Init(Vector3 position, Quaternion rotation, BulletInfo information)
	{
		transform.rotation = rotation;
		Init (position, information);
	}


	// Use this for initialization
	void Start ()
	{
		ChildCore = transform.Find ("Core");
		ChildGlowSmall = transform.Find ("Glow Small");
		ChildGlowLarge = transform.Find ("Glow Large");

		Expired = false;
		gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
	{
		DistanceTravelled += Information.Speed * Time.deltaTime;
		if (DistanceTravelled > DISTANCE_MAX)
			Expired = true;
	}
}
