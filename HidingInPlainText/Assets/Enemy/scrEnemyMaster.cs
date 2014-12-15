using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrEnemyMaster : MonoBehaviour
{
	public static scrEnemyMaster Instance { get; private set; }

	public GameObject EnemyPrefab;

	public List<GameObject> Enemies { get; private set; }

	public void Create(GameObject owner, Vector3 position, string message)
	{
		GameObject enemy = (GameObject)Instantiate(EnemyPrefab, position, Quaternion.identity);
		enemy.GetComponent<scrEnemy>().Init(owner, message);
		Enemies.Add(enemy);
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
		if (scrMaster.Loading)
			return;

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
