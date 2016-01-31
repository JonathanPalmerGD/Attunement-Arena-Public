using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class RigidbodyFirstPersonController : MonoBehaviour
{
	public Player Owner;
	public float airDrag = 2;

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
		[HideInInspector]
		public float CurrentTargetSpeed = 8f;

#if !MOBILE_INPUT
		private bool m_Running;
#endif

		public void UpdateDesiredTargetSpeed(Vector2 input)
		{
			if (input == Vector2.zero)
			{
				CurrentTargetSpeed = 0;
				//if (CurrentTargetSpeed != 0)
				//{
				//	Debug.Log("Decaying\n");
				//}
				//float decayAmt = Mathf.Clamp(CurrentTargetSpeed * .1f, .5f, 1);
				//if (CurrentTargetSpeed > 0)
				//{
				//	CurrentTargetSpeed -= decayAmt;
				//	if (CurrentTargetSpeed < 0)
				//	{
				//		CurrentTargetSpeed = 0;
				//	}
				//}
				//else if(CurrentTargetSpeed < 0)
				//{
				//	CurrentTargetSpeed += decayAmt;
				//	if (CurrentTargetSpeed > 0)
				//	{
				//		CurrentTargetSpeed = 0;
				//	}
				//}

				//Debug.Log("\n" + CurrentTargetSpeed);
				return;
			}
			if (input.x > 0 || input.x < 0)
			{
				//strafe
				CurrentTargetSpeed = StrafeSpeed;
			}
			if (input.y < 0)
			{
				//backwards
				CurrentTargetSpeed = BackwardSpeed;
			}
			if (input.y > 0)
			{
				//forwards
				//handled last as if strafing and moving forward at the same time forwards speed should take precedence
				CurrentTargetSpeed = ForwardSpeed;
			}
#if !MOBILE_INPUT
			if (Input.GetKey(RunKey))
			{
				CurrentTargetSpeed *= RunMultiplier;
				m_Running = true;
			}
			else
			{
				m_Running = false;
			}
#endif
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
		//if (Input.GetKeyDown(KeyCode.LeftControl))
		//{
		//	mRigidBody.useGravity = !mRigidBody.useGravity;
		//	mRigidBody.velocity = Vector3.zero;
		//}

		RotateView();

		//if (Input.GetButtonDown(Owner.PlayerInput + "Jump") && !mJump)
		//{
		//	mJump = true;
		//}
	}

	void ApplyExternalForce(Vector3 force)
	{
		accRBForces += force;
	}

	public void ApplyExternalForce(Vector3 force, bool friendly = false)
	{
		//Debug.Log("Hit\n" + Owner.name + "\t" + force);
		if (!friendly && Owner.buffState == Player.PlayerBuff.Shielded)
		{
			//Debug.Log("Hit\n" + Owner.name + "\t" + force);

			//Negate the player's shielded state?
			return;
		}

		Debug.Log("Hit\n" + Owner.name + "\n" + accRBForces + " " + force);
		accRBForces += force;
	}

	public void AddExternalForce(Vector3 force, ForceMode fMode = ForceMode.Force, bool friendly = false)
	{
		if (!friendly && Owner.buffState == Player.PlayerBuff.Shielded)
		{
			//Negate the player's shielded state?
			return;
		}

		mRigidBody.AddForce(force, fMode);
	}

	private void FixedUpdate()
	{
		//Debug.Log(mRigidBody.velocity + "\n" + mRigidBody.velocity.magnitude);
		GroundCheck();
		Vector2 input = GetInput();

		if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || mIsGrounded))
		{
			//Debug.Log("Moving\n");
			// always move along the camera forward as it is the direction that it being aimed at
			Vector3 desiredMove =	cam.transform.forward * input.y + cam.transform.right * input.x;
			desiredMove = Vector3.ProjectOnPlane(desiredMove, mGroundContactNormal).normalized;

			//mRigidBody.drag = 50f;

			desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
			desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
			desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;
			//Debug.DrawLine(transform.position, transform.position + desiredMove * 5, Color.blue, .1f);
			if (mRigidBody.velocity.sqrMagnitude <
				(movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed))
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
							   ((mCapsule.height / 2f) - mCapsule.radius) + advancedSettings.groundCheckDistance))
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
