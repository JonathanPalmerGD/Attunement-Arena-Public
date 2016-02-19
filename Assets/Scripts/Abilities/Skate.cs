using UnityEngine;
using System.Collections;

public class Skate : Ability
{
	public GameObject icePrefab;
	public GameObject iceParent;

	public Status slowStatus;
	public Status chillDmgStatus;
	public override KeyActivateCond activationCond
	{
		get
		{
			if (Owner.ControlType == Player.PlayerControls.GamePad)
			{
				return KeyActivateCond.KeyHold;
				//return KeyActivateCond.GetAxis;
			}
			else
			{
				return KeyActivateCond.KeyHold;
			}
		}
	}
	public override bool UseCharges
	{
		get
		{
			return false;
		}
	}
	public override int IconID
	{
		get { return 9; }
	}
	public float slowMultAdj = .5f;

	public float Force = 15;
	public float AmplifiedForce = 15;

	public bool waterAligned;

	public override void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
	{
		icePrefab = Resources.Load<GameObject>("Effects/icePrefab");
		iceParent = new GameObject();
		iceParent.name = "[P" + newOwner.playerID + "] Ice Parent";
		base.Init(newOwner, newKeyBinding, displayKeyBinding);

		Cost = 2f;
		Force = 26;
		MaxCooldown = .05f;
		Duration = 3.5f;
		GeneralDamage = 0f;
	}

	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		AudioSource source = AudioManager.Instance.MakeSource("Whoosh");
		source.volume = .3f;
		source.Play();

		#region Ice Cloud Setup
		GameObject iceCloud = GameObject.Instantiate<GameObject>(icePrefab);
		iceCloud.name = "[P" + Owner.playerID + "] Ice Cloud";
		
		if (GeneralDamage != 0 || slowMultAdj != 0)
		{
			IceAreaEffect iceEff = iceCloud.AddComponent<IceAreaEffect>();

			iceEff.Owner = Owner;
			iceEff.Creator = this;
		}

		iceCloud.transform.SetParent(iceParent.transform);
		float yDiff = inputVector.normalized.y;
		iceCloud.transform.position = Owner.transform.position - (inputVector * .4f) - (Vector3.up * yDiff);

		GameObject.Destroy(iceCloud, Duration);
		#endregion

		Vector3 lookAtPos = new Vector3((Owner.transform.position + inputVector).x, iceCloud.transform.position.y, (Owner.transform.position + inputVector).z);
		iceCloud.transform.LookAt(lookAtPos);

		Vector3 oldVel = Owner.controller.mRigidBody.velocity;
		
		if (Owner.Grounded)
		{
			Owner.controller.Jumping = true;
		}

		Vector3 forwardForce;
		if (Owner.transform.position.y < Owner.transform.position.y + inputVector.y)
		{
			Owner.controller.mRigidBody.velocity = new Vector3(oldVel.x, Mathf.Clamp(oldVel.y, 0, 100), oldVel.z);
			//Debug.Log("Gain height\n");
			forwardForce = new Vector3(inputVector.x, inputVector.y * 8, inputVector.z).normalized;
			
			//Owner.SendMessage("ApplyExternalForce", );
		}
		else
		{
			Owner.controller.mRigidBody.velocity = new Vector3(oldVel.x, Mathf.Clamp(oldVel.y, -12, 100), oldVel.z);
			//Debug.Log("Forward\n");
			forwardForce = new Vector3(inputVector.x, inputVector.y * 6, inputVector.z).normalized;
			//Owner.SendMessage("ApplyExternalForce", inputVector * Force);
		}

		float appliedForce = Force;

		if (waterAligned)
		{
			//Get the WaterShield from the player.
			WaterShield shield = Owner.GetAbility<WaterShield>();

			if (shield)
			{
				//Up the force
				appliedForce += AmplifiedForce;

				//If the shield is active
				if (shield.ShieldActive)
				{
					//Extend the duration
					shield.AddWaterShield(CooldownReduction);
				}
			}
		}

		Owner.controller.ApplyExternalForce(forwardForce * Force, true, true);
	}
}
