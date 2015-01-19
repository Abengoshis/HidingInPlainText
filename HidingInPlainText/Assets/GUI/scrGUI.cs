using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class scrGUI : MonoBehaviour
{
	public static scrGUI Instance { get; private set; }

	Transform nodeInfoPanel;
	Transform[] locks = new Transform[scrPlayer.LOCKED_ENEMIES_MAX];

	// Use this for initialization
	void Start ()
	{
		Instance = this;

		nodeInfoPanel = transform.Find ("NodeInfo");

		locks[0] = transform.Find ("Locks").Find("Lock");
		for (int i = 1; i < locks.Length; ++i)
		{
			locks[i] = ((GameObject)Instantiate(locks[0].gameObject, locks[0].position, locks[0].rotation)).transform;
			locks[i].parent = locks[0].parent;
			locks[i].name = "Lock";
		}

		HideNodeInfo();
	}
	
	// Update is called once per frame
	void Update ()
	{
		for (int i = 0; i < scrPlayer.LOCKED_ENEMIES_MAX; ++i)
		{
			if (i < scrPlayer.Instance.EnemiesLocked.Count)
			{
				Vector3 screenPosition = Camera.main.WorldToScreenPoint(scrPlayer.Instance.EnemiesLocked[i].GetComponent<scrEnemy>().transform.position);
				locks[i].transform.position = screenPosition;
				locks[i].transform.localScale = Vector3.one * (1 - screenPosition.z / scrPlayer.ENEMY_SEARCH_DISTANCE);
				locks[i].GetComponent<Image>().enabled = true;
			}
			else
			{
				locks[i].GetComponent<Image>().enabled = false;
			}
		}
	}

	public void SetNodeInfo(Message data)
	{
		nodeInfoPanel.Find ("Title").GetComponent<Text>().text = data.time + System.Environment.NewLine + data.page_title;

		string location = "Location:";
		if (data.geo == null)
		{
			location += " Unknown";
		}
		else
		{
			if (!string.IsNullOrEmpty(data.geo.city))
				location += " " + data.geo.city;

			if (!string.IsNullOrEmpty(data.geo.region_name))
				location += " " + data.geo.region_name;

			if (!string.IsNullOrEmpty(data.geo.country_name))
				location += " " + data.geo.country_name;

			location += " Lat: " + data.geo.latitude;
			location += " Long: " + data.geo.longitude;
		}

		nodeInfoPanel.Find ("Info").GetComponent<Text>().text = location + System.Environment.NewLine + "User: " + data.user + "    " + "Committed: " + data.change_size + " Bytes";
	}

	public void SetNodeInfection(float infection)
	{
		if (infection > 0)
			nodeInfoPanel.Find ("Status").GetComponent<Text>().text = (int)(infection * 100) + "% INFECTED";
		else
			nodeInfoPanel.Find ("Status").GetComponent<Text>().text = "";
	}

	public void SetNodeInfoPosition(Vector3 worldPosition, float maxDistance)
	{
		Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
		if (screenPoint.z < maxDistance)
		{
			if (screenPoint.z <= 0)
			{
				HideNodeInfo();
			}
			else
			{
				nodeInfoPanel.position = screenPoint;
				nodeInfoPanel.localScale = Vector3.one * (1 - screenPoint.z / maxDistance);	// Scale the size arbitrarily by the z.
			}
		}
	}

	public void HideNodeInfo()
	{
		nodeInfoPanel.localScale = Vector3.zero;
	}
}
