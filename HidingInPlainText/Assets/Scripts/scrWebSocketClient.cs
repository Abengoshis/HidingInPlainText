﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using WebSocketSharp;


public class scrWebSocketClient : MonoBehaviour
{
	private WebSocket client;
	

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

				HandleMessage(e.Data);

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


	}


	void HandleMessage(string message)
	{
		Dictionary<string, object> keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);


	}

}
