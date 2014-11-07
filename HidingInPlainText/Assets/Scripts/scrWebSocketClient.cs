using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using WebSocketSharp;


public class scrWebSocketClient : MonoBehaviour
{
	private WebSocket client;

	private scrFeed feed;

	// Record for useful parts of a message. Maybe homogenise this with scrFeed.Entry.
	private struct Message
	{
		public string page_title;
		public string url;
		public int change_size;
	}

	// Stack of messages read by the client.
	Queue<Message> messagesAccumulated = new Queue<Message>();

	void Start ()
	{
		feed = GetComponent<scrFeed>();

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
		if (messagesAccumulated.Count != 0)
		{
			Message message = messagesAccumulated.Dequeue ();
			feed.AddEntry(message.page_title, message.url, message.change_size);
		}

	}
	
	void ReadMessage(string data)
	{
		Dictionary<string, object> messageData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

		// Filter to only "Main" value of "ns", which are normal page edits. 
		if ((string)messageData["action"] == "edit" && (string)messageData["ns"] == "Main" && Mathf.Abs (System.Convert.ToInt32(messageData["change_size"])) > 100)
		{
			// Create a message from the message data.
			Message message = new Message();
			message.page_title = (string)messageData["page_title"];
			message.url = (string)messageData["url"];
			message.change_size = System.Convert.ToInt32(messageData["change_size"]);

			messagesAccumulated.Enqueue (message);
		}
	}
}
