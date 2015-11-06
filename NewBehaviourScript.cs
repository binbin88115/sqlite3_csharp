using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using TinyServer;
using Mono.Data.Sqlite;
using System.Data;

using TinyServer.Protocols;
using TinyServer.Schemas;

public class NewBehaviourScript : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		Debug.Log(ServerService.GetInstance().Request("init?account_name=xbb"));
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
}
