using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrEnemyMaster : MonoBehaviour
{
	public static scrEnemyMaster Instance { get; private set; }
	public static int EnemyLayer { get; private set; }

	public GameObject WordEnemyPrefab;
	public GameObject EggTitanPrefab;

	public List<scrEnemy> FreeEnemies { get; private set; }	// Enemies that have had their nodes destroyed.
	
	// Use this for initialization
	void Start ()
	{
		Instance = this;
		FreeEnemies = new List<scrEnemy>();
		EnemyLayer = LayerMask.NameToLayer("Enemy");
	}

	public void Add(scrEnemy enemy)
	{
		FreeEnemies.Add(enemy);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (scrMaster.Loading)
			return;

		for (int i = FreeEnemies.Count - 1; i >= 0; --i)
		{
			if (FreeEnemies[i] == null)
			{
				FreeEnemies.RemoveAt(i);
			}
		}
	}


}
