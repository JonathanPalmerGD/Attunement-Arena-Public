using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class RigidbodyFirstPersonController : MonoBehaviour
{
	public Player Owner;
	public float airDrag = 2;
	public float inputAmt = 0;
	public float forceAmt = 0;
	//The decay time for sleeping constant forces (like air currents)
	private float sleepCounterStart = .2f;
	private float sleepCounter = 0;
	private bool SleepConstantForce = false;
	public bool InCurrent;

	#region Movement & Advanced Settings
	[Serializable]
	public class MovementSettings
	{
		public float ForwardSpeed = 8.0f;   // Speed when walking forward
		public float BackwardSpeed = 4.0f;  // Speed when walking backwards
		public float StrafeSpeed = 4.0f;    // Speed when walking sideways
		public float RunMultiplier = 2.0f;   // Speed when sprinting
		public KeyCode RunKey = KeyCode.LeftShift;
		public float JumpForce = 30f;
		public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
		//[HideInInspector]
		public float CurrentTargetSpeed = 0f;

#if !MOBILE_INPUT
		private bool m_Running;
#endif

		public void UpdateDesiredTargetSpeed(Vector2 input)
		{
			if (input.x < .05f && input.x > -.05f && input.y < .05f && input.y > -.05f)
			{
				CurrentTargetSpeed = 0;

				//Debug.Log("\n" + CurrentTargetSpeed);
				return;
			}
			if (input.x > 0.05f || input.x < -.05f)
			{
				//strafe
				CurrentTargetSpeed = StrafeSpeed;
			}
			if (input.y < -.05f)
			{
				//backwards
				CurrentTargetSpeed = BackwardSpeed;
			}
			if (input.y > 0.05f)
			{
				//forwards
				//handled last as if strafing and moving forward at the same time forwards speed should take precedence
				CurrentTargetSpeed = ForwardSpeed;
			}
		}

#if !MOBILE_INPUT
		public bool Running
		{
			get { return m_Running; }
		}
#endif
	}

	[Serializable]
	public class AdvancedSettings
	{
		public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
		public float stickToGroundHelperDistance = 0.5f; // stops the character
		public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
		public bool airControl; // can the user control the direction that is being moved in the air
	}
	#endregion

	public Camera cam;
	public MovementSettings movementSettings = new MovementSettings();
	public MouseLook mouseLook = new MouseLook();
	public AdvancedSettings advancedSettings = new AdvancedSettings();

	public Rigidbody mRigidBody;
	private Vector3 accRBForces;
	private CapsuleCollider mCapsule;
	private float mYRotation;
	private Vector3 mGroundContactNormal;
	private bool mJump, mPreviouslyGrounded, mJumping, mIsGrounded;

	public Vector3 Velocity
	{
		get { return mRigidBody.velocity; }
	}

	public bool Grounded
	{
		get { return mIsGrounded; }
	}

	public bool Jumping
	{
		get { return mJumping; }
		set { mJumping = value; }
	}

	public bool Running
	{
		get
		{
#if !MOBILE_INPUT
			return movementSettings.Running;
#else
	            return false;
#endif
		}
	}

	private void Start()
	{
		mRigidBody = GetComponent<Rigidbody>();
		mCapsule = GetComponent<CapsuleCollider>();
		mouseLook.Init(transform, cam.transform);
		mouseLook.controller = this;
	}

	private void Update()
	{
		if (!Owner.playerDead)
			RotateView();
	}

	void ApplyExternalForce(Vector3 force)
	{
		accRBForces += force;
	}

	/// <summary>
	/// This function cares if the controller is currently sleeping external forces.
	/// Basically if the player is in a air current but gets hit with an explosive force, we can sleep the application of that effect.
	/// </summary>
	/// <param name="force"></param>
	/// <param name="removeGrounded"></param>
	/// <param name="friendly"></param>
	public void ApplyConstantForce(Vector3 force, bool removeGrounded = true, bool friendly = false)
	{
		if (!SleepConstantForce)
		{
			ApplyExternalForce(force, removeGrounded, friendly, false);
		}
	}

	public void ApplyExternalForce(Vector3 force, bool removeGrounded = true, bool friendly = false, bool tracked = true)
	{
		//Debug.Log("Hit\n" + Owner.name + "\t" + force);
		if (!friendly && Owner.KnockbackMultiplier != 1)
		{
			float magnitude = force.magnitude;
			WaterShield shield = Owner.GetAbility<WaterShield>();

			if (shield && shield.ShieldActive)
			{
				shield.ProcessKnockback(magnitude);
			}

			//Debug.Log("Knockback Multiplier is adjusting\n" + (int)magnitude + "  to  " + (int)magnitude * Owner.KnockbackMultiplier);
			force = force.normalized * magnitude * Owner.KnockbackMultiplier;
		}

		if (removeGrounded)
		{
			Jumping = true;
		}

		if (tracked)
		{
			//Debug.Log("Adding new tracked force: " + force.magnitude +"\n");
			forceAmt += force.magnitude;
		}

		//Debug.Log("" + Owner.name + "\nOld: " + accRBForces + "  New Force:" + force);
		accRBForces += force;
	}

	private void DecayExternalForceTracking()
	{
		if (SleepConstantForce)
		{
			//Debug.Log("Decay:" + SleepConstantForce + "\nForce Amount: " + forceAmt + "\nTimer: " + sleepCounter);
			sleepCounter -= Time.deltaTime;
			if (sleepCounter < 0)
			{
				sleepCounter = 0;
				SleepConstantForce = false;

				forceAmt = 0;
				//Debug.Log("Sleep Reset: " + forceAmt + "\n");
			}
		}

		if (forceAmt > 50)
		{
			//Debug.Log("Sleep Start: " + forceAmt + "\n");
			SleepConstantForce = true;
			sleepCounter = sleepCounterStart;
		}

		if (forceAmt > 1)
		{
			//Debug.Log("Decaying: " + forceAmt + "\n");
			forceAmt *= .8f;
		}
		else
		{
			forceAmt = 0;
		}
	}

	private void FixedUpdate()
	{
		InCurrent = false;
		//Debug.Log(mRigidBody.velocity + "\n" + mRigidBody.velocity.magnitude);
		GroundCheck();
		Vector2 input = Owner.playerDead ? Vector2.zero : GetInput();

		if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || mIsGrounded))
		{
			//Debug.Log("Moving\n");
			// always move along the camera forward as it is the direction that it being aimed at
			Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
			desiredMove = Vector3.ProjectOnPlane(desiredMove, mGroundContactNormal).normalized;

			float speedMult = Owner._speedMult;

			desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed * speedMult;
			desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed * speedMult;
			desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed * speedMult;

			//Debug.Log(speedMult + "\n" + desiredMove.magnitude);
			if (mRigidBody.velocity.sqrMagnitude <
				(movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed * speedMult))
			{
				mRigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
			}
		}

		if (mIsGrounded)
		{
			//mRigidBody.drag = 5f;

			if (mJump)
			{
				mRigidBody.drag = 0f;
				mRigidBody.velocity = new Vector3(mRigidBody.velocity.x, 0f, mRigidBody.velocity.z);
				mRigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
				mJumping = true;
			}

			if (!mJumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && mRigidBody.velocity.magnitude < 1f)
			{
				mRigidBody.Sleep();
			}
		}
		else
		{
			mRigidBody.drag = airDrag;
			if (mPreviouslyGrounded && !mJumping)
			{
				//StickToGroundHelper();
			}
		}
		mJump = false;

		if (accRBForces.sqrMagnitude > 0.1f)
		{
			mRigidBody.AddForce(accRBForces, ForceMode.Impulse);
			accRBForces = Vector3.zero;
		}

		DecayExternalForceTracking();
	}

	private float SlopeMultiplier()
	{
		float angle = Vector3.Angle(mGroundContactNormal, Vector3.up);
		return movementSettings.SlopeCurveModifier.Evaluate(angle);
	}

	private void StickToGroundHelper()
	{
		RaycastHit hitInfo;
		if (Physics.SphereCast(transform.position, mCapsule.radius, Vector3.down, out hitInfo,
							   ((mCapsule.height / 2f) - mCapsule.radius) +
							   advancedSettings.stickToGroundHelperDistance))
		{
			if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
			{
				mRigidBody.velocity = Vector3.ProjectOnPlane(mRigidBody.velocity, hitInfo.normal);
			}
		}
	}

	private Vector2 GetInput()
	{
		Vector2 input = new Vector2
			{
				x = Input.GetAxis(Owner.PlayerInput + "Horizontal"),
				y = Input.GetAxis(Owner.PlayerInput + "Vertical")
			};
		movementSettings.UpdateDesiredTargetSpeed(input);

		inputAmt = input.magnitude;
		return input;
	}

	private void RotateView()
	{
		//avoids the mouse looking if the game is effectively paused
		if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

		// get the rotation before it's changed
		float oldYRotation = transform.eulerAngles.y;

		mouseLook.LookRotation(transform, cam.transform);

		if (mIsGrounded || advancedSettings.airControl)
		{
			// Rotate the rigidbody velocity to match the new direction that the character is looking
			Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
			mRigidBody.velocity = velRotation * mRigidBody.velocity;
		}
	}

	/// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
	private void GroundCheck()
	{
		mPreviouslyGrounded = mIsGrounded;
		RaycastHit hitInfo;
		if (Physics.SphereCast(transform.position, mCapsule.radius, Vector3.down, out hitInfo,
							   (transform.localScale.y * (mCapsule.height / 2f) - mCapsule.radius) + advancedSettings.groundCheckDistance))
		{
			mIsGrounded = true;
			mGroundContactNormal = hitInfo.normal;
		}
		else
		{
			mIsGrounded = false;
			mGroundContactNormal = Vector3.up;
		}
		if (!mPreviouslyGrounded && mIsGrounded && mJumping)
		{
			mJumping = false;
		}
	}
}
