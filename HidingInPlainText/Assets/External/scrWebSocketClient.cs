using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using WebSocketSharp;

public class GeoData
{
	public string areacode;
	public string city;
	public string country_code;
	public string country_name;
	public string ip;
	public double latitude;
	public double longitude;
	public string metro_code;
	public string region_code;
	public string region_name;
	public string zipcode;
}

// Record for useful parts of a message. Not all messages contain the same stuff, so converting directly to an object like I do with the GeoData is probably a bad idea.
public struct Message
{
	public string page_title;
	public string summary;
	public string url;
	public int change_size;
	public bool is_anon;
	public bool is_bot;

	// The geographical data, if it exists.
	public GeoData geo;
}



public class scrWebSocketClient : MonoBehaviour
{
	private WebSocket client;

	// Stack of messages read by the client.
	Queue<Message> messagesAccumulated = new Queue<Message>();

	void Start ()
	{
		/* Create the WebSocket client, set up its events, then connect it. */

		client = new WebSocket("ws://wikimon.hatnote.com/en/");
		
		client.OnOpen += (sender, e) => 
		{
			Debug.Log ("Connection established.");
		};

		client.OnError += (sender, e) => 
		{
			Debug.Log ("WebSocket Error: " + e.Message);
		};

		client.OnClose += (sender, e) => 
		{
			Debug.Log ("Connection terminated.");
		};

		client.OnMessage += (sender, e) =>
		{
			if (e.Type == Opcode.Text)
			{
				Debug.Log ("Text message received.");

				ReadMessage(e.Data);

				return;
			}

			if (e.Type == Opcode.Binary)
			{
				Debug.Log ("Binary message received.");
				return;
			}
		};

		client.Connect();
	}

	void OnDestroy()
	{
		client.Close();
	}

	void Update()
	{
		// Dump a message into the feed every frame.
		while (messagesAccumulated.Count != 0)
		{
			messagesAccumulated.Dequeue();
		}

	}
	
	void ReadMessage(string data)
	{
		Dictionary<string, object> messageData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

		// Filter to only "Main" value of "ns", which are normal page edits. 
		if ((string)messageData["action"] == "edit" && (string)messageData["ns"] == "Main" && System.Convert.ToInt32(messageData["change_size"]) != 0 && (bool)messageData["is_minor"] == false)
		{
			// Create a message from the message data.
			Message message = new Message();
			message.page_title = (string)messageData["page_title"];
			message.summary = (string)messageData["summary"];
			message.url = (string)messageData["url"];
			message.change_size = System.Convert.ToInt32(messageData["change_size"]);
			message.is_anon = (bool)messageData["is_anon"];
			message.is_bot = (bool)messageData["is_bot"];

			if (messageData.ContainsKey("geo_ip"))
			{
				message.geo = JsonConvert.DeserializeObject<GeoData>(messageData["geo_ip"].ToString());
			}
			else
			{
				message.geo = null;
			}

			messagesAccumulated.Enqueue (message);
		}
	}
}
