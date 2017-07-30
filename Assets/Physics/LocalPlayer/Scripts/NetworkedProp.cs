using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

// networked prop interface
public interface INetworkedProp
{
	void SetTick (int tick);

	bool WriteToNetwork (int fromTick, NetworkWriter writer);

	bool ReadFromNetwork (NetworkReader reader);
}

// network serialization helper
public class NetworkedSerializer<T>
{
	public NetworkedSerializer (Action<NetworkWriter, T> serializer, Func<NetworkReader, T> deserializer)
	{
		this.serializer = serializer;
		this.deserializer = deserializer;
		this.interpolator = null;
	}

	public NetworkedSerializer (Action<NetworkWriter, T> serializer, Func<NetworkReader, T> deserializer, Func<T, T, float, T> interpolator)
	{
		this.serializer = serializer;
		this.deserializer = deserializer;
		this.interpolator = interpolator;
	}

	public Action<NetworkWriter, T> serializer { get; }

	public Func<NetworkReader, T> deserializer { get; }

	public Func<T, T, float, T> interpolator { get; }
}

// this is kinda super ugly
public class NetworkedSerializer
{
	static public NetworkedSerializer<Vector3> Vector3 = new NetworkedSerializer<Vector3> ((NetworkWriter w, Vector3 v) => {
		w.Write (v);
	}, (NetworkReader r) => {
		return r.ReadVector3 ();
	}, (Vector3 v1, Vector3 v2, float fraction) => {
		return v1 + (v2 - v1) * fraction;
	});

	static public NetworkedSerializer<Int32> Int32 = new NetworkedSerializer<Int32> ((NetworkWriter w, Int32 v) => {
		w.Write (v);
	}, (NetworkReader r) => {
		return r.ReadInt32 ();
	});
}

// networked property
public class NetworkedProp<T> : INetworkedProp
{
	private NetworkedSerializer<T> serializer;
	private int index = 0;
	private int currentTick = 0;
	private T[] oldValue = new T[100];
	private T currentValue;

	public NetworkedProp (NetworkedSerializer<T> serializer)
	{
		Debug.Log ("NetworkedProp #" + NetworkedPropContext.currentContext.GetCount ());

		this.index = NetworkedPropContext.currentContext.GetCount ();
		NetworkedPropContext.currentContext.Add (this);

		this.serializer = serializer;
	}

	// TODO: create global tickcount in GameManager
	public void SetTick (int tick)
	{
		this.currentTick = tick;
	}

	public void SetValue (T value)
	{
		this.currentValue = value;
		this.oldValue [this.currentTick % 100] = value;
	}

	public T GetValue ()
	{
		return this.currentValue;
	}

	public T GetInterpolatedValue (int fromTick, float fraction)
	{
		T v1 = this.oldValue [fromTick % 100];
		if (this.serializer.interpolator == null)
			return v1;

		T v2 = this.oldValue [(fromTick + 1) % 100];
		return this.serializer.interpolator (v1, v2, fraction);
	}

	public bool WriteToNetwork (int fromTick, NetworkWriter writer)
	{
		// we dont have a newer update for the client yet
		if (fromTick == this.currentTick)
			return false;

		// the client is already up to date and not to far behind
		if (fromTick != -1 && this.currentTick - fromTick < 100 &&
		    this.oldValue [fromTick % 100].Equals (this.currentValue))
			return false;

		writer.Write ((short)this.index);
		this.serializer.serializer (writer, this.currentValue);
		return true;
	}

	public bool ReadFromNetwork (NetworkReader reader)
	{
		T value = this.serializer.deserializer (reader);
		// TODO: in case we skipped some values for whatever reason fill them up?
		this.currentValue = value;
		this.oldValue [this.currentTick % 100] = value;
		return true;
	}
}

public class NetworkedPropContext
{
	static public NetworkedPropContext currentContext { get; set; }

	protected List<INetworkedProp> props = new List<INetworkedProp> ();

	// on instantiation reset global index counter
	public NetworkedPropContext ()
	{
		Debug.Log ("NetworkedPropContext");

		NetworkedPropContext.currentContext = this;
	}

	public void Add (INetworkedProp prop)
	{
		this.props.Add (prop);
	}

	public int GetCount ()
	{
		return this.props.Count;
	}

	// send from server
	public bool OnSerialize (NetworkWriter writer, bool forceAll)
	{
		foreach (INetworkedProp prop in this.props) {
			if (forceAll) {
				prop.WriteToNetwork (-1, writer);
			} else {
				prop.WriteToNetwork (-1, writer); // TODO: add global tickcount :)
			}
		}

		return true;
	}

	// received from client
	public void OnDeserialize (NetworkReader reader, bool initialState)
	{
		while (true) {
			try {
				short index = reader.ReadInt16 ();
				if (index > 0 && index < this.props.Count) {
					this.props [index].ReadFromNetwork (reader);
				}
			} catch (Exception e) {
				break;
			}
		}
	}
}
