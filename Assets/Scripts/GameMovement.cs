using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GameMovementSettings
{
	public float movementScale = 0.03f;
	public bool quakeBunnyHop = true;
	public float gravity = 800.0f;
	public float jumpHeight = 48.0f;
	// 24.0f
	public float friction = 5.0f;
	public float stopSpeed = 80.0f;
	public float maxSpeed = 250.0f;
	public float acceleration = 6.0f;
	public float airAcceleration = 48.0f;
	// 12.0f
	public float stepSize = 16.0f;
	public float bounce = 0.0f;
}

public class GameMovementState
{
	// movement flags
	public static int IN_JUMP_HELD = (1 << 1);

	public CharacterController characterController;
	public Vector3 velocity;

	public int moveType = 2;

	public int oldButtons = 0;
	public int movementFlags = 0;

	public float duckTime = 0.0f;
	public float duckJumpTime = 0.0f;
	public float jumpTime = 0.0f;

	public bool onGround = false;
	public float surfaceFriction = 1.0f;

	public Vector3 wishVelocity = Vector3.zero;
	public Vector3 jumpVelocity = Vector3.zero;

	public static float stepSize = 0.16f;
	public float stepHeight = 0.0f;
}

public class GameMovement : MonoBehaviour
{
	public GameMovementSettings settings;
	private GameMovementState state;
	private UserCommand command;
	private Vector3 forward, right, up;

	static float NON_JUMP_VELOCITY = 140.0f;
	static float DIST_EPSILON = 0.03125f;

	static int COORD_FRACTIONAL_BITS = 5;
	static int COORD_DENOMINATOR = (1 << (COORD_FRACTIONAL_BITS));
	static float COORD_RESOLUTION = (1.0f / (float)(COORD_DENOMINATOR));

	public void SetupMovement (GameMovementState state, UserCommand command)
	{
		this.state = state;
		this.command = command;
	}

	public void ProcessMovement ()
	{
		this.PlayerMove ();
		this.FinishPlayerMove ();
	}

	public void OnCollision (ControllerColliderHit hit)
	{
		if (!this.state.onGround) {
			if (hit.normal.y > 0.7f)
				this.ClipVelocity (this.state.velocity, hit.normal, out this.state.velocity, 1.0f);
			else
				this.ClipVelocity (this.state.velocity, hit.normal, out this.state.velocity, 1.0f + this.settings.bounce * (1.0f - this.state.surfaceFriction));
		} else {
			// ignore slopes if on ground
			if (hit.normal.y > 0.7f)
				return;

			if (hit.normal.y < 0.0f) {
				/*
                Debug.Log("normal < 0");
                Vector3 dir = Vector3.Cross(this.state.velocity, hit.normal);
                dir.Normalize();
                this.state.velocity = dir * Vector3.Dot(dir, this.state.velocity);
                return;
                */
			}

			this.ClipVelocity (this.state.velocity, hit.normal, out this.state.velocity, 1.0f);
		}

		/*
        bool ignoreSlopes = (this.onGround && this.oldOnGround);

        Vector3 originalVelocity = this.state.velocity;
        Vector3 newVelocity = this.state.velocity;
        Vector3 primalVelocity = this.state.velocity;

        if (collision.contacts.Length == 1 && !this.onGround)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y > 0.7f)
                {
                    if (!ignoreSlopes)
                    {
                        this.ClipVelocity(originalVelocity, contact.normal, out newVelocity, 1.0f);
                        originalVelocity = newVelocity;
                    }
                }
                else
                {
                    this.ClipVelocity(originalVelocity, contact.normal, out newVelocity, 1.0f + globalBounce * (1.0f - this.surfaceFriction));
                }
            }

            this.state.velocity = newVelocity;
            originalVelocity = newVelocity;
        }
        else
        {
            Debug.Log(ignoreSlopes);

            int i, j;
            for (i = 0; i < collision.contacts.Length; i++)
            {
                if (collision.contacts[i].normal.y <= 0.7f && ignoreSlopes)
                {
                    this.ClipVelocity(originalVelocity, collision.contacts[i].normal, out newVelocity, 1.0f);
                    originalVelocity = newVelocity;
                }

                for (j = 0; j < collision.contacts.Length; j++)
                {
                    if (i == j)
                        continue;

                    if (Vector3.Dot(this.state.velocity, collision.contacts[j].normal) < 0.0f)
                        break;
                }

                if (j == collision.contacts.Length)
                    break;
            }

            this.state.velocity = newVelocity;
            originalVelocity = newVelocity;

            // this can cause issues in unity when going around edges? :(
            if (i == collision.contacts.Length)
            {
                if (collision.contacts.Length != 2)
                {
                    this.state.velocity = Vector3.zero;
                    return;
                }

                Vector3 dir = Vector3.Cross(collision.contacts[0].normal, collision.contacts[1].normal);
                dir.Normalize();
                this.state.velocity = dir * Vector3.Dot(dir, this.state.velocity);
            }

            if (Vector3.Dot(this.state.velocity, primalVelocity) <= 0.0f)
            {
                this.state.velocity = Vector3.zero;
                return;
            }
        }
        */
	}

	public bool IsOnGround ()
	{
		return this.state.onGround;
	}

	protected void SetOnGround (bool onGround)
	{
		bool oldOnGround = this.state.onGround;

		// base velocity tests

		this.state.onGround = onGround;

		if (onGround)
			this.state.surfaceFriction = 1.0f;
	}

	protected void SetOrigin (Vector3 origin)
	{
		//if (this.IsOnGround())
		//    origin.y -= 0.01f;

		this.state.characterController.Move (this.ScaleOrigin (origin - this.GetOrigin ()));

		if (this.IsOnGround ()) {
			float gravity = 1.0f;

			Vector3 newOrigin = Vector3.zero;
			newOrigin.y -= ((gravity * this.settings.gravity * 10.0f * Time.fixedDeltaTime) * Time.fixedDeltaTime * this.settings.movementScale);
			//Debug.Log (newOrigin.y);
			this.state.characterController.Move (newOrigin);
		}
	}

	protected Vector3 GetOrigin ()
	{
		return this.ScaleOriginReverse (this.state.characterController.center);
	}

	// source engine to unity conversion
	protected Vector3 ScaleOrigin (Vector3 origin)
	{
		return origin * this.settings.movementScale;
	}

	// unity to source engine conversion
	protected Vector3 ScaleOriginReverse (Vector3 origin)
	{
		return origin / this.settings.movementScale;
	}

	public void PlayerMove ()
	{
		this.state.wishVelocity = Vector3.zero;
		this.state.jumpVelocity = Vector3.zero;

		this.ReduceTimers ();

		this.forward = this.command.viewAngles * Vector3.forward;
		this.right = this.command.viewAngles * Vector3.right;
		this.up = this.command.viewAngles * Vector3.up;

		if (this.state.moveType != 2) // MOVETYPE_WALK
            this.CategorizePosition ();

		//this.UpdateDuckJumpEyeOffset();
		this.Duck ();

		if (!this.LadderMove () && this.state.moveType == 9)
			this.state.moveType = 2;

		switch (this.state.moveType) {
		case 0:
			break;

		case 9:
			this.FullLadderMove ();
			break;

		case 2:
			this.FullWalkMove ();
			break;

		default:
			break;
		}
	}

	protected void ReduceTimers ()
	{
	}

	public void FinishPlayerMove ()
	{
		this.state.oldButtons = this.command.buttons;
	}

	protected void CategorizePosition ()
	{
		this.SetOnGround (this.state.characterController.isGrounded);
	}

	protected void StartGravity ()
	{
		float gravity = 1.0f;

		this.state.velocity.y -= (gravity * this.settings.gravity * 0.5f * Time.fixedDeltaTime);
		//this.state.velocity.z += this.baseVelocity * Time.fixedDeltaTime; // other objects don't push us yet

		CheckVelocity ();
	}

	protected void FinishGravity ()
	{
		float gravity = 1.0f;

		this.state.velocity.y -= (gravity * this.settings.gravity * 0.5f * Time.fixedDeltaTime);

		this.CheckVelocity ();
	}

	protected void Duck ()
	{
	}

	protected void FullWalkMove ()
	{
		this.StartGravity ();

		this.CheckJumpButton ();

		if (this.state.onGround) {
			this.state.velocity.y = 0.0f;
			this.Friction ();
		}

		this.CheckVelocity ();

		if (this.state.onGround)
			this.WalkMove ();
		else
			this.PlayerAirMove ();

		this.CategorizePosition ();

		this.CheckVelocity ();

		this.FinishGravity ();

		if (this.state.onGround)
			this.state.velocity.y = 0.0f;

		//CheckFalling();
	}

	protected bool LadderMove ()
	{
		return false;
	}

	protected void FullLadderMove ()
	{
	}

	protected bool CheckJumpButton ()
	{
		if (!this.settings.quakeBunnyHop) {
			if ((command.buttons & UserCommand.IN_JUMP) == 0) {
				this.state.oldButtons &= ~UserCommand.IN_JUMP;
				return false;
			}

			if (!this.state.onGround) {
				this.state.oldButtons |= UserCommand.IN_JUMP;
				return false;
			}

			if ((this.state.oldButtons & UserCommand.IN_JUMP) != 0)
				return false;
		} else {
			if ((command.buttons & UserCommand.IN_JUMP) == 0) {
				this.state.movementFlags &= ~GameMovementState.IN_JUMP_HELD;
				return false;
			}

			if ((this.state.movementFlags & GameMovementState.IN_JUMP_HELD) != 0)
				return false;


			if (this.state.onGround)
				this.state.movementFlags |= GameMovementState.IN_JUMP_HELD;
			else
				return false;
		}

		/*
         // Cannot jump will in the unduck transition.
         if ( player->m_Local.m_bDucking && (  player->GetFlags() & FL_DUCKING ) )
         return false;
         */

		if (this.state.duckJumpTime > 0.0f)
			return false;

		this.SetOnGround (false);

		float groundFactor = 1.0f;
		// flGroundFactor = player->m_pSurfaceData->game.jumpFactor; 

		float multiplier = Mathf.Sqrt (2.0f * this.settings.gravity * this.settings.jumpHeight);

		float startY = this.state.velocity.y;
		/*
         * if ( (  player->m_Local.m_bDucking ) || (  player->GetFlags() & FL_DUCKING ) )
         * mv->m_vecVelocity[2] = flGroundFactor * flMul;  // 2 * gravity * height
         * else
         */
		this.state.velocity.y += groundFactor * multiplier;

		this.FinishGravity ();

		this.state.jumpVelocity.y += this.state.velocity.y - startY;
		this.state.stepHeight += 0.15f;

		this.state.oldButtons |= UserCommand.IN_JUMP;
		return true;
	}

	protected void WalkMove ()
	{
		Vector3 forward = this.command.viewAngles * Vector3.forward;
		Vector3 right = this.command.viewAngles * Vector3.right;
		Vector3 up = this.command.viewAngles * Vector3.up;

		bool oldOnGround = this.state.onGround;

		float forwardMove = this.command.movement.x;
		float rightMove = this.command.movement.y;

		forward.y = right.y = 0.0f;
		forward.Normalize ();
		right.Normalize ();

		Vector3 wishVelocity = Vector3.zero;
		wishVelocity.x = forward.x * forwardMove + right.x * rightMove;
		wishVelocity.z = forward.z * forwardMove + right.z * rightMove;

		Vector3 wishDirection = wishVelocity;
		float wishSpeed = wishDirection.magnitude;
		wishDirection.Normalize ();

		// clamp
		if (wishSpeed != 0.0f && wishSpeed > this.settings.maxSpeed) {
			for (int i = 0; i < 3; i++)
				wishVelocity [i] *= this.settings.maxSpeed / wishSpeed;
			wishSpeed = this.settings.maxSpeed;
		}

		// set pmove velocity
		this.state.velocity.y = 0.0f;
		this.Accelerate (wishDirection, wishSpeed, this.settings.acceleration);
		this.state.velocity.y = 0.0f;

		// add in base velocity

		float speed = this.state.velocity.magnitude;
		if (speed < 1.0f) {
			this.state.velocity = Vector3.zero;
			// subtract base velocity
			return;
		}

		Vector3 origin = this.GetOrigin ();
		origin.x += this.state.velocity.x * Time.fixedDeltaTime;
		origin.z += this.state.velocity.z * Time.fixedDeltaTime;
		this.SetOrigin (origin);

		this.state.wishVelocity += wishDirection * wishSpeed;
	}

	protected void PlayerAirMove ()
	{
		float forwardMove = this.command.movement.x;
		float rightMove = this.command.movement.y;

		Vector3 forward = this.command.viewAngles * Vector3.forward;
		Vector3 right = this.command.viewAngles * Vector3.right;
		Vector3 up = this.command.viewAngles * Vector3.up;

		forward.y = right.y = 0.0f;
		forward.Normalize ();
		right.Normalize ();

		Vector3 wishVelocity = Vector3.zero;
		wishVelocity.x = forward.x * forwardMove + right.x * rightMove;
		wishVelocity.z = forward.z * forwardMove + right.z * rightMove;

		Vector3 wishDirection = wishVelocity;
		float wishSpeed = wishDirection.magnitude;
		wishDirection.Normalize ();

		// clamp
		if (wishSpeed != 0.0f && wishSpeed > this.settings.maxSpeed) {
			for (int i = 0; i < 3; i++)
				wishVelocity [i] *= this.settings.maxSpeed / wishSpeed;
			wishSpeed = this.settings.maxSpeed;
		}

		this.AirAccelerate (wishDirection, wishSpeed, this.settings.airAcceleration);

		this.SetOrigin (this.GetOrigin () + this.state.velocity * Time.fixedDeltaTime);
	}

	protected void Accelerate (Vector3 wishDir, float wishSpeed, float acceleration)
	{
		// canAccelerate?

		float currentSpeed = Vector3.Dot (this.state.velocity, wishDir);
		float addSpeed = wishSpeed - currentSpeed;
		if (addSpeed <= 0.0f)
			return;

		float accelSpeed = acceleration * Time.fixedDeltaTime * wishSpeed * this.state.surfaceFriction;
		if (accelSpeed > addSpeed)
			accelSpeed = addSpeed;

		for (int i = 0; i < 3; i++)
			this.state.velocity [i] += accelSpeed * wishDir [i];
	}

	protected void AirAccelerate (Vector3 wishDir, float wishSpeed, float acceleration)
	{
		if (wishSpeed > 30)
			wishSpeed = 30;

		float currentSpeed = Vector3.Dot (this.state.velocity, wishDir);
		float addSpeed = wishSpeed - currentSpeed;
		if (addSpeed <= 0.0f)
			return;

		float accelSpeed = acceleration * wishSpeed * Time.fixedDeltaTime * this.state.surfaceFriction;
		if (accelSpeed > addSpeed)
			accelSpeed = addSpeed;

		for (int i = 0; i < 3; i++) {
			this.state.velocity [i] += accelSpeed * wishDir [i];
			this.state.wishVelocity [i] += accelSpeed * wishDir [i];
		}
	}

	protected void Friction ()
	{
		float speed = this.state.velocity.magnitude;
		if (speed < 0.1f)
			return;

		float drop = 0.0f;
		if (this.state.onGround) {
			float friction = this.settings.friction * this.state.surfaceFriction;
			float control = (speed < this.settings.stopSpeed) ? this.settings.stopSpeed : speed;
			drop += control * friction * Time.fixedDeltaTime;
		}

		float newSpeed = speed - drop;
		if (newSpeed < 0.0f)
			newSpeed = 0.0f;

		if (newSpeed != speed) {
			newSpeed /= speed;

			for (int i = 0; i < 3; i++)
				this.state.velocity [i] *= newSpeed;
		}

		this.state.wishVelocity -= (1.0f - newSpeed) * this.state.velocity;
	}

	protected void CheckVelocity ()
	{
	}

	protected int ClipVelocity (Vector3 inVelocity, Vector3 normal, out Vector3 outVelocity, float overbounce)
	{
		float angle = normal.y;

		int blocked = 0;
		if (angle > 0.0f)
			blocked |= 0x01;
		if (angle == 0.0f)
			blocked |= 0x02;

		float backoff = Vector3.Dot (inVelocity, normal) * overbounce;

		outVelocity = Vector3.zero;
		for (int i = 0; i < 3; i++) {
			float change = normal [i] * backoff;
			outVelocity [i] = inVelocity [i] - change;
		}

		float adjust = Vector3.Dot (outVelocity, normal);
		if (adjust < 0.0f)
			outVelocity -= (normal * adjust);

		return blocked;
	}
}
