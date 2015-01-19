using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrBulletMaster : MonoBehaviour
{
	public static scrBulletMaster Instance { get; private set; }

	public GameObject BulletPrefab;

	LinkedList<GameObject> playerBulletPool;	// All player bullets that can spawn. Pooled for performance (fewer instantiations necessary).
	int inactivePlayerBulletCount;	// The number of inactive (free) player bullets at the start of the pool.

	LinkedList<GameObject> bulletPool;	// All player bullets that can spawn. Pooled for performance (fewer instantiations necessary).
	int inactiveBulletCount;	// The number of inactive (free) player bullets at the start of the pool.
	
	scrBullet Create()
	{
		// Get the first inactive bullet in the bullet pool and activate it.
		LinkedListNode<GameObject> bullet = bulletPool.First;
		ActivateBullet(bullet);

		// Get the bullet script.
		return bullet.Value.GetComponent<scrBullet>();
	}

	public void Create(Vector3 position, Vector3 direction, scrBullet.BulletInfo information)
	{
		// Initialise the bullet.
		Create ().Init (position, direction, information);
	}

	public void Create(Vector3 position, Quaternion rotation, scrBullet.BulletInfo information)
	{
		// Initialise the bullet.
		Create ().Init (position, rotation, information);
	}

	void ActivateBullet(LinkedListNode<GameObject> bullet)
	{
		bullet.Value.SetActive(true);
		bullet.Value.transform.parent = null;
		bullet.Value.rigidbody.WakeUp();

		bulletPool.Remove (bullet);
		bulletPool.AddLast(bullet);
		--inactiveBulletCount;
	}
	
	void DeactivateBullet(LinkedListNode<GameObject> bullet)
	{
		bullet.Value.transform.parent = transform;
		bullet.Value.rigidbody.Sleep ();
		bullet.Value.SetActive(false);

		bulletPool.Remove (bullet);
		bulletPool.AddFirst(bullet);
		++inactiveBulletCount;
	}

	public void LoadBulletPool(int numPlayerBullets, int numEnemyBullets)
	{
		bulletPool = new LinkedList<GameObject>();
		for (int i = 0; i < numEnemyBullets; ++i)
		{
			bulletPool.AddLast((GameObject)Instantiate (BulletPrefab));
			bulletPool.Last.Value.rigidbody.Sleep ();
		}
	}

	// Use this for initialization
	void Start ()
	{
		Instance = this;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (scrMaster.Loading)
			return;
		
		LinkedListNode<GameObject> bullet = bulletPool.First;
		for (int i = 0; i < bulletPool.Count; ++i)
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
					DeactivateBullet(bullet.Previous);
			}
			else
			{
				if (bullet.Value.GetComponent<scrBullet>().Expired)
					DeactivateBullet(bullet);
			}
		}
	}
}
