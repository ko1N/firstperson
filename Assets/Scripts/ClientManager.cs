using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ClientManager : MonoBehaviour
{

	private NetworkClient client;

	public void Connect (string ip, int port)
	{
		this.client = new NetworkClient ();
		this.RegisterHandlers ();
		this.client.Connect (ip, port);
	}

	public void ConnectLoopback ()
	{
		this.client = ClientScene.ConnectLocalServer ();
		this.RegisterHandlers ();
	}

	private void RegisterHandlers ()
	{
		this.client.RegisterHandler (MsgType.Connect, OnConnect);
		this.client.RegisterHandler (MsgType.Disconnect, OnDisconnect);
		this.client.RegisterHandler (MsgType.NotReady, OnNotReady);
		this.client.RegisterHandler (MsgType.Error, OnError);
	}

	private void OnConnect (NetworkMessage msg)
	{
		Debug.Log ("client: connected");
		ClientScene.Ready (msg.conn);
		ClientScene.AddPlayer (0);
	}

	private void OnDisconnect (NetworkMessage msg)
	{
		Debug.Log ("client: disconnect");
	}

	private void OnNotReady (NetworkMessage msg)
	{
		Debug.Log ("client: not ready");
	}

	private void OnError (NetworkMessage msg)
	{
		Debug.Log ("client: error " + msg.ToString ());
	}
}
