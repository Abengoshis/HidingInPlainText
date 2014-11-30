using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrEnemyMaster : MonoBehaviour
{
	public static scrEnemyMaster Instance { get; private set; }

	public GameObject EnemyPrefab;

	public List<GameObject> Enemies { get; private set; }

	public void Create(Vector3 position)
	{
		Enemies.Add((GameObject)Instantiate(EnemyPrefab, position, Quaternion.identity));
	}

	// Use this for initialization
	void Start ()
	{
		Instance = this;

		Enemies = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		for (int i = 0; i < Enemies.Count; ++i)
		{

			// Destroy expired enemies.
			if (Enemies[i].GetComponent<scrEnemy>().Expired)
			{
				Destroy(Enemies[i]);
				Enemies.RemoveAt (i);
				--i;
			}
		}
	}
}
