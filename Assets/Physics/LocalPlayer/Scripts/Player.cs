using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class Player : NetworkBehaviour
{
	public GameMovement gameMovement;
	protected GameMovementState movementState = new GameMovementState ();

	protected NetworkedPropContext networkContext = new NetworkedPropContext ();
	protected NetworkedProp<Vector3> origin = new NetworkedProp<Vector3> (NetworkedSerializer.Vector3);
	protected NetworkedProp<Vector3> velocity = new NetworkedProp<Vector3> (NetworkedSerializer.Vector3);
	protected NetworkedProp<Int32> team = new NetworkedProp<Int32> (NetworkedSerializer.Int32);

	// server send
	public override bool OnSerialize (NetworkWriter writer, bool forceAll)
	{
		if (!this.isServer)
			return false;
	
		//Debug.Log ("OnSerialize");
		return this.networkContext.OnSerialize (writer, forceAll);
	}

	// client receive
	public override void OnDeserialize (NetworkReader reader, bool initialState)
	{
		//if (this.isServer)
		//	return;

		Debug.Log ("OnDeserialize");
		this.networkContext.OnDeserialize (reader, initialState);
	}
}
