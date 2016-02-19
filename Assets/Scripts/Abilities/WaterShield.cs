using UnityEngine;
using System.Collections;

public class WaterShield : Ability
{
	public GameObject burstPrefab;
	public ParticleSystem shieldVisual;

	public Status dmgReductStatus;
	public Status knockbackReductStatus;

	public override KeyActivateCond activationCond
	{
		get { return KeyActivateCond.KeyDown; }
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
		get { return 7; }
	}

	public override int AlternateIconID
	{
		get { return 14; }
	}

	public float dmgMultAdj = .5f;
	public float kckBackMultAdj = .5f;
	public float Range = 10;
	public float Force = 200;
	public bool ShieldActive = false;

	public override void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
	{
		burstPrefab = Resources.Load<GameObject>("Effects/burstPrefab");
		base.Init(newOwner, newKeyBinding, displayKeyBinding);

		shieldVisual = newOwner.transform.FindChild("waterShield").GetComponent<ParticleSystem>();

		dmgReductStatus = Owner.AddStatus(this, Status.StatusTypes.Shielded, 0, dmgMultAdj, false);
		knockbackReductStatus = Owner.AddStatus(this, Status.StatusTypes.Sturdy, 0, kckBackMultAdj, false);

		MaxCooldown = 8f;
		Cost = 15;
		GeneralDamage = 26f;
		Duration = 4f;
	}

	public override bool CanUseAbility()
	{
		return base.CanUseAbility();
	}

	public void CheckShieldHolding()
	{
		//If we have the components for a shield
		if (dmgReductStatus != null && knockbackReductStatus != null)
		{
			float remainDurShield = dmgReductStatus.DurationLeft;
			float remainDurKnockBack = knockbackReductStatus.DurationLeft;

			if (remainDurKnockBack <= 0 || remainDurShield <= 0)
			{
				shieldVisual.enableEmission = false;
				ShieldActive = false;
			}
		}
		else
		{
			shieldVisual.enableEmission = false;
			ShieldActive = false;
		}
	}

	public override void UpdateAbility(float deltaTime)
	{
		CheckShieldHolding();

		//If the owner is shielded
		if (ShieldActive)
		{
			//Set it to have a detonate icon
			SetDisplayIcon = AlternateIconID;
			CurrentCooldown = 0;
		}
		else
		{
			//Set it to have a normal icon
			SetDisplayIcon = IconID;
			if (CurrentCooldown - deltaTime > 0)
			{
				CurrentCooldown -= deltaTime;
			}
			else
			{
				CurrentCooldown = 0;
			}
		}
	}

	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		if (ShieldActive)
		{
			//Remove the current shield
			dmgReductStatus.DurationLeft = 0;
			knockbackReductStatus.DurationLeft = 0;

			//Disable the visual
			shieldVisual.enableEmission = false;

			//Detonate it
			DetonateShield();
		}
		else
		{
			AddWaterShield(Duration);
		}
	}

	public void ProcessDamageTaken(float damageAmt)
	{
		float damagePrevented = damageAmt * dmgMultAdj;

		//Reduce shield duration.
		// The percentage lost is the amount of base duration lost.
		knockbackReductStatus.DurationLeft -= (damagePrevented / Owner.MaxHealth) * Duration;
		dmgReductStatus.DurationLeft -= (damagePrevented / Owner.MaxHealth) * Duration;
	}

	public void ProcessKnockback(float knockbackAmt)
	{
		//How much damage was prevented
		float knockbackPrevented = knockbackAmt * kckBackMultAdj;

		//Debug.Log("Applying reduced external force."
		//+ "\nBase Mag: " + magnitude
		//+ "\tKnock prevented: " + (magnitude * shield.kckBackMultAdj)
		//+ "\nShield Dur Red: " + (knockbackPrevented / 750) * shield.Duration
		//+ "\tNew Knockback force: " + force.magnitude);

		//Reduce shield duration.
		knockbackReductStatus.DurationLeft -= (knockbackPrevented / 750) * Duration;
		dmgReductStatus.DurationLeft -= (knockbackPrevented / 750) * Duration;
	}

	public void AddWaterShield(float specificDuration = 0)
	{
		dmgReductStatus.DurationLeft += specificDuration;
		dmgReductStatus.EffectAmt = dmgMultAdj;
		
		knockbackReductStatus.EffectAmt = kckBackMultAdj;
		knockbackReductStatus.DurationLeft += specificDuration;
		
		shieldVisual.enableEmission = true;

		ShieldActive = true;
	}

	public void DetonateShield()
	{
		#region Deal AOE damage and knockback
		foreach (Player player in GameManager.Instance.players)
		{
			if (player == Owner)
			{
			}
			else
			{

				float dist = Vector3.Distance(Owner.transform.position, player.transform.position);

				//Debug.DrawRay(Owner.transform.position, p.transform.position - Owner.transform.position, Color.black, 5.0f);
				RaycastHit hit;
				if (Physics.Raycast(Owner.transform.position, player.transform.position - Owner.transform.position, out hit, Range))
				{
					//Find what percentage away the player is
					float percentOfdist = (Range - dist) / Range;

					//The players take a minimum of 25% effect from the blast.
					float blastIntensityBasedOnDist = Mathf.Clamp(percentOfdist, .25f, 1);

					//Find the direction they're knocked away
					Vector3 knockbackDir = player.transform.position - Owner.transform.position;

					//Debug.Log(percentOfdist + "  " + blastIntensityBasedOnDist + " \n" + knockbackDir + "\n");

					//Debug.DrawLine(Owner.transform.position, Owner.transform.position + Vector3.up * 100, Color.blue, 15f);

					//Knock them away based on the direction, force of the ability and how much of the blast they're affected by
					player.controller.ApplyExternalForce((knockbackDir.normalized + Vector3.up) * Force * blastIntensityBasedOnDist);

					//Adjust their health based on the blasts damage and how much of the blast.
					player.AdjustHealth(-1 * blastIntensityBasedOnDist * GeneralDamage * Owner._dmgDealtMult);
				}
			}
		}
		#endregion

		#region Create the visual explosion
		GameObject newBurst = GameObject.Instantiate<GameObject>(burstPrefab);
		newBurst.name = "[P" + Owner.playerID + "] Hydro-Burst Prefab";
		newBurst.transform.position = Owner.transform.position;
		newBurst.transform.SetParent(Owner.transform);
		GameObject.Destroy(newBurst, 1.5f);
		#endregion
	}
}
