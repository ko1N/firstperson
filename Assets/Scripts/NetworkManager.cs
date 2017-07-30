using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
	public ServerManager server;
	public ClientManager client;

	public bool isAtStartup = true;

	NetworkClient myClient;

	void Update ()
	{
		if (isAtStartup) {
			if (Input.GetKeyDown (KeyCode.S)) {
				SetupServer ();
			}

			if (Input.GetKeyDown (KeyCode.C)) {
				SetupClient ();
			}

			if (Input.GetKeyDown (KeyCode.B)) {
				SetupServer ();
				SetupLocalClient ();
			}
		}
	}

	void OnGUI ()
	{
		if (isAtStartup) {
			GUI.Label (new Rect (200, 10, 150, 100), "Press S for server");     
			GUI.Label (new Rect (200, 30, 150, 100), "Press B for both");       
			GUI.Label (new Rect (200, 50, 150, 100), "Press C for client");
		}
	}

	// Create a server and listen on a port
	public void SetupServer ()
	{
		this.server.StartServer (13337);
		isAtStartup = false;
	}

	// Create a client and connect to the server port
	public void SetupClient ()
	{
		this.client.Connect ("127.0.0.1", 13337);
		isAtStartup = false;
	}

	// Create a local client and connect to the local server
	public void SetupLocalClient ()
	{
		this.server.StartLoopback ();
		this.client.ConnectLoopback ();
		isAtStartup = false;
	}

	// client callbacks
	private void OnDespawnEntity (GameObject spawned)
	{
		Destroy (spawned);
	}

	private void OnClientConnected (NetworkMessage netMsg)
	{
		ClientScene.Ready (netMsg.conn);
		ClientScene.AddPlayer (0);
	}
	/*
	private GameObject OnSpawnEntity (Vector3 position, NetworkHash128 assetId)
	{
		var networkEntity = Instantiate<NetworkIdentity> (_networkStateEntityProtoType);

		networkEntity.transform.SetParent (this.transform);
		return networkEntity.gameObject;
	}

	// server callbacks
	private void OnPlayerDisconnect (NetworkMessage netMsg)
	{
		var playerGamePiece = netMsg.conn.playerControllers [0].gameObject;

		NetworkServer.UnSpawn (playerGamePiece);

		Destroy (playerGamePiece);
	}

	private void PopulateServerEntities ()
	{
		var globals = FindObjectOfType<GlobalAssets> ();

		//initialize all server-controlled entities here
		var npc = Instantiate<GameObject> (globals.NetworkEntityStatePrototype);

		npc.GetComponent<NetworkEntityState> ().PrefabType = PrefabType.Npc;

		NetworkServer.Spawn (npc);

	}

	private void OnAddPlayer (NetworkMessage netMsg)
	{
		var globals = FindObjectOfType<GlobalAssets> ();
		var playerStateGo = Instantiate<GameObject> (globals.NetworkEntityStatePrototype);
		var playerState = playerStateGo.GetComponent<NetworkEntityState> ();

		playerState.PrefabType = PrefabType.Player;
		playerState.transform.SetParent (this.transform);

		NetworkServer.AddPlayerForConnection (netMsg.conn, playerStateGo, 0);
	}
	*/
}
