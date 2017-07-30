using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LocalPlayer : Player
{
	public GameObject localView;
	private CharacterController characterController;

	private Text velocityText;
	private Text deltaTimeText;
	private Text onGroundText;

	public InputSystem inputSystem = new InputSystem ();
	protected Quaternion viewAngles = new Quaternion ();

	void Start ()
	{
		// we don't need a reference to gameMovement on non local client players
		if (this.isServer || this.isLocalPlayer)
			this.gameMovement = GameObject.Find ("GameMovement").GetComponent<GameMovement> ();
		
		// initialize the charactercontroller
		this.characterController = GetComponent<CharacterController> ();
		this.movementState.characterController = this.characterController;

		// TODO: needs to be somewhere else
		//Cursor.visible = false;

		// TODO: needs to be somewheere else!
		this.velocityText = GameObject.Find ("VelocityText").GetComponent<Text> ();
		this.deltaTimeText = GameObject.Find ("DeltaTimeText").GetComponent<Text> ();
		this.onGroundText = GameObject.Find ("OnGroundText").GetComponent<Text> ();
	}

	void Update ()
	{
		if (!this.isLocalPlayer)
			return;
		
		// we update the rotation each frame but sample it in the usercmd each tick
		Quaternion viewAngles = this.inputSystem.GetViewAngleDelta (this.transform, localView.transform);
		this.UpdateViewAngles (viewAngles);
	}

	void FixedUpdate ()
	{
		if (!this.isLocalPlayer)
			return;

		UserCommand command = new UserCommand ();

		// setup UserCommand
		command.viewAngles = this.GetViewAngles ();
		command.movement = this.inputSystem.GetMovement ();
		command.buttons = this.inputSystem.GetButtons ();

		// feed UserCommand and state in GameMovement and process the move
		this.gameMovement.SetupMovement (this.movementState, command);
		this.gameMovement.ProcessMovement ();

		this.velocityText.text = "velocity: " + this.movementState.velocity.magnitude;
		this.deltaTimeText.text = "deltaTime: " + Time.fixedDeltaTime * 1000 + "ms";
		this.onGroundText.text = "onGround: " + this.gameMovement.IsOnGround ();

		this.SetDirtyBit (1);
	}

	void OnControllerColliderHit (ControllerColliderHit hit)
	{
		this.gameMovement.OnCollision (hit);
	}

	Quaternion GetViewAngles ()
	{
		return localView.transform.rotation;
	}

	void UpdateViewAngles (Quaternion viewAngles)
	{
		this.transform.localRotation *= Quaternion.Euler (0.0f, viewAngles.y, 0.0f);
		localView.transform.localRotation *= Quaternion.Euler (viewAngles.x, 0.0f, 0.0f);

		localView.transform.localRotation = this.ClampRotation (localView.transform.localRotation);
	}

	protected Quaternion ClampRotation (Quaternion q)
	{
		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1.0f;

		float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);
		angleX = Mathf.Clamp (angleX, -89.0f, 89.0f);
		q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);
		return q;
	}
}
