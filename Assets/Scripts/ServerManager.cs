using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerManager : MonoBehaviour
{
	public GameObject playerPrefab;
	public Transform spawnPosition;

	public void StartServer (int port)
	{
		NetworkServer.Listen (port);
		this.RegisterHandlers ();
	}

	public void StartLoopback ()
	{
		this.RegisterHandlers ();
	}

	private void RegisterHandlers ()
	{
		NetworkServer.RegisterHandler (MsgType.Connect, OnConnect);
		NetworkServer.RegisterHandler (MsgType.Disconnect, OnDisconnect);
		NetworkServer.RegisterHandler (MsgType.AddPlayer, AddPlayer);
		NetworkServer.RegisterHandler (MsgType.RemovePlayer, RemovePlayer);
	}

	private void OnConnect (NetworkMessage msg)
	{
		Debug.Log ("server: new client connected");
	}

	private void OnDisconnect (NetworkMessage msg)
	{
		Debug.Log ("server: client disconnected");
		NetworkServer.DestroyPlayersForConnection (msg.conn);
	}

	private void OnServerReady (NetworkMessage msg)
	{
	}

	private void AddPlayer (NetworkMessage msg)
	{
		Debug.Log ("server: adding player");
		GameObject player = (GameObject)Instantiate (this.playerPrefab, this.spawnPosition.position, Quaternion.identity);
		NetworkServer.AddPlayerForConnection (msg.conn, player, 0);
	}

	private void RemovePlayer (NetworkMessage msg)
	{
		/*
		PlayerController player;
		if (conn.GetPlayer (playerControllerId, out player)) {
			if (player.NetworkIdentity != null && player.NetworkIdentity.gameObject != null)
				NetworkServer.Destroy (player.NetworkIdentity.gameObject);
		}
		*/
	}

	private void OnServerError (NetworkMessage msg)
	{
		Debug.Log ("server: error " + msg.ToString ());
	}
}
