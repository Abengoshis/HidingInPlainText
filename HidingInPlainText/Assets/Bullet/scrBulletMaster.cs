using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrBulletMaster : MonoBehaviour
{
	public static scrBulletMaster Instance { get; private set; }

	public GameObject BulletPrefab;

	LinkedList<GameObject> playerBulletPool;	// All player bullets that can spawn. Pooled for performance (fewer instantiations necessary).
	int inactivePlayerBulletCount;	// The number of inactive (free) player bullets at the start of the pool.

	LinkedList<GameObject> enemyBulletPool;	// All player bullets that can spawn. Pooled for performance (fewer instantiations necessary).
	int inactiveEnemyBulletCount;	// The number of inactive (free) player bullets at the start of the pool.
	
	scrBullet Create(bool player)
	{
		// Get the first inactive bullet in the correct bullet pool and activate it.
		LinkedListNode<GameObject> bullet;
		if (player)
		{
			bullet = playerBulletPool.First;
			ActivatePlayerBullet(bullet);
		}
		else
		{
			bullet = enemyBulletPool.First;
			ActivateEnemyBullet(bullet);
		}
		
		// Get the bullet script.
		return bullet.Value.GetComponent<scrBullet>();
	}

	public void Create(bool player, Vector3 position, Vector3 direction, scrBullet.BulletInfo information)
	{
		// Initialise the bullet.
		Create (player).Init (position, direction, information);
	}

	public void Create(bool player, Vector3 position, Quaternion rotation, scrBullet.BulletInfo information)
	{
		// Initialise the bullet.
		Create (player).Init (position, rotation, information);
	}

	void ActivatePlayerBullet(LinkedListNode<GameObject> bullet)
	{
		bullet.Value.SetActive(true);
		bullet.Value.transform.parent = null;
		bullet.Value.rigidbody.WakeUp();

	    playerBulletPool.Remove (bullet);
	    playerBulletPool.AddLast(bullet);
	    --inactivePlayerBulletCount;
	}
	    
    void DeactivatePlayerBullet(LinkedListNode<GameObject> bullet)
	{
		bullet.Value.transform.parent = transform;
		bullet.Value.rigidbody.Sleep ();
		bullet.Value.SetActive(false);
	
		playerBulletPool.Remove (bullet);
		playerBulletPool.AddFirst(bullet);
		++inactivePlayerBulletCount;
	}

	void ActivateEnemyBullet(LinkedListNode<GameObject> bullet)
	{
		bullet.Value.SetActive(true);
		bullet.Value.transform.parent = null;
		bullet.Value.rigidbody.WakeUp();

		enemyBulletPool.Remove (bullet);
		enemyBulletPool.AddLast(bullet);
		--inactiveEnemyBulletCount;
	}
	
	void DeactivateEnemyBullet(LinkedListNode<GameObject> bullet)
	{
		bullet.Value.transform.parent = transform;
		bullet.Value.rigidbody.Sleep ();
		bullet.Value.SetActive(false);

		enemyBulletPool.Remove (bullet);
		enemyBulletPool.AddFirst(bullet);
		++inactiveEnemyBulletCount;
	}

	void LoadBulletPool(int numPlayerBullets, int numEnemyBullets)
	{
		int playerLayer = LayerMask.NameToLayer("Player");
		playerBulletPool = new LinkedList<GameObject>();
		for (int i = 0; i < numPlayerBullets; ++i)
		{
			playerBulletPool.AddLast((GameObject)Instantiate (BulletPrefab));
			playerBulletPool.Last.Value.transform.parent = transform;
			playerBulletPool.Last.Value.rigidbody.Sleep ();

			foreach (Transform g in playerBulletPool.Last.Value.GetComponentsInChildren<Transform>())
				g.gameObject.layer = playerLayer;
		}

		int enemyLayer = LayerMask.NameToLayer("Player");
		enemyBulletPool = new LinkedList<GameObject>();
		for (int i = 0; i < numEnemyBullets; ++i)
		{
			enemyBulletPool.AddLast((GameObject)Instantiate (BulletPrefab));
			enemyBulletPool.Last.Value.transform.parent = transform;
			enemyBulletPool.Last.Value.rigidbody.Sleep ();

			foreach (Transform g in enemyBulletPool.Last.Value.GetComponentsInChildren<Transform>())
				g.gameObject.layer = enemyLayer;
		}
	}

	// Use this for initialization
	void Start ()
	{
		Instance = this;

		LoadBulletPool(50, 1000);
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Manage the player bullet pool.
		LinkedListNode<GameObject> bullet = playerBulletPool.First;
		for (int i = 0; i < playerBulletPool.Count; ++i)
		{
			if (!bullet.Value.activeSelf)
			{
				bullet = bullet.Next;
				continue;
			}

			if (bullet.Next != null)
			{
				bullet = bullet.Next;

				if (bullet.Previous.Value.GetComponent<scrBullet>().Expired)
				{
					DeactivatePlayerBullet(bullet.Previous);
				}
			}
			else
			{
				if (bullet.Value.GetComponent<scrBullet>().Expired)
				{
					DeactivatePlayerBullet(bullet);
				}
			}
		}

		// Manage the enemy bullet pool.
		bullet = enemyBulletPool.First;
		for (int i = 0; i < enemyBulletPool.Count; ++i)
		{
			if (!bullet.Value.activeSelf)
			{
				bullet = bullet.Next;
				continue;
			}

			if (bullet.Next != null)
			{
				bullet = bullet.Next;
				
				if (bullet.Previous.Value.GetComponent<scrBullet>().Expired)
				{
					DeactivateEnemyBullet(bullet.Previous);
				}
			}
			else
			{
				if (bullet.Value.GetComponent<scrBullet>().Expired)
					DeactivateEnemyBullet(bullet);
			}
		}
	}
}
