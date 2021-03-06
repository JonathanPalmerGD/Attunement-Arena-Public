﻿using UnityEngine;
using System.Collections;

public class Bolt : Ability
{
	public GameObject boltPrefab;
	public GameObject bolt;
	private BoltEffect boltEff;
	public Status DamageAmp;

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
		get { return 3; }
	}

	public override float GeneralDamage
	{
		get
		{
			return generalDamage;
		}
	}
	private float maxAngle = 8;
	public float MaxAngle
	{
		get
		{
			return maxAngle;
		}
		set
		{
			maxAngle = value;
		}
	}

	public float VelocityDampenThreshold = 9;
	public float empowerMax = .25f;
	public float empowerChargeTime = 2;
	public bool lightningAligned = false;

	public float Range
	{
		get
		{
			return 135f;
		}
	}

	public override void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
	{
		boltPrefab = Resources.Load<GameObject>("Effects/boltPrefab");
		bolt = GameObject.Instantiate(boltPrefab, newOwner.transform.position, newOwner.transform.rotation) as GameObject;
		bolt.name = "Bolt Renderer [P" + newOwner.playerID + "]";
		boltEff = bolt.GetComponent<BoltEffect>();

		base.Init(newOwner, newKeyBinding, displayKeyBinding);

		MaxCooldown = .07f;
		MaxAngle = 8;
		GeneralDamage = 2.0f;
		Cost = 3;
		Duration = 4f;

		bolt.transform.SetParent(newOwner.transform);

	}

	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		var castDir = inputVector.normalized;

		//Debug.DrawLine(Owner.transform.position, Owner.transform.position + castDir * Range, Color.green, 5.0f);

		foreach (Player p in GameManager.Instance.players)
		{
			if (p == Owner && GameManager.Instance.NumPlayers > 1) continue; // Don't influence self

			// Get vector from owner to other player
			var tetherVector = p.transform.position - Owner.transform.position;

			// If out of range, ignore
			if (tetherVector.sqrMagnitude > Range * Range)
			{
				boltEff.ZapPoint(Owner.transform.position + castDir * Range);
				continue;
			}

			// If the other player is not in the cone of influence, ignore
			if (Vector3.Angle(castDir, tetherVector) > MaxAngle)
			{
				boltEff.ZapPoint(Owner.transform.position + castDir * Range);
				continue;
			}


			// If there's something in the way, ignore player
			//if (Physics.Linecast(Owner.transform.position, p.transform.position))
			RaycastHit hit;
			if (Physics.Raycast(Owner.transform.position, p.transform.position - Owner.transform.position, out hit, Range))
			{
				if (hit.collider.gameObject.tag != "Player")
				{
					boltEff.ZapPoint(hit.point);

					//Debug.DrawLine(Owner.transform.position, hit.point, Color.blue, 5.0f);
					//Debug.Log(hit.collider.name + "\n\n\n");
					continue;
				}
				else
				{
					#region Debug Lines for Lightning
					//Debug.DrawLine(Owner.transform.position, Owner.transform.position + tetherVector, Color.red, 5.0f);
					//Debug.Log(Vector3.Angle(castDir, tetherVector) + "\n\n\n");
					//Debug.DrawLine(Owner.transform.position, Owner.transform.position + tetherVector, Color.white, 5.0f);

					//Debug.DrawLine(Owner.transform.position, Owner.transform.position + tetherVector, Color.black, 25.0f);
					#endregion

					if (lightningAligned)
					{
						if (DamageAmp == null)
						{
							DamageAmp = Owner.AddStatus(this, Status.StatusTypes.Empowered, Duration, 0.0f, false);
						}
						else
						{
							//Reset the status duration if we deal any damage with lightning.
							DamageAmp.DurationLeft = Duration;
							
							float ampGain = empowerMax * MaxCooldown / empowerChargeTime;

							//If we are close to the threshold, only add the little bit we need left, else add whatever is to be gained.
							ampGain = DamageAmp.EffectAmt + ampGain < empowerMax ? ampGain : empowerMax - DamageAmp.EffectAmt;

							//Ramp up the damage amp.
							DamageAmp.ModifyStatus(0, ampGain);
						}
					}

					boltEff.ZapTarget(p.gameObject);

					//Debug.Log(p.name + "\n");
					p.AdjustHealth(-GeneralDamage * Owner._dmgDealtMult);

					if (p.controller.mRigidBody.velocity.sqrMagnitude >= VelocityDampenThreshold * VelocityDampenThreshold)
					{
						p.controller.mRigidBody.velocity -= p.controller.mRigidBody.velocity.normalized * VelocityDampenThreshold;
					}
				}
			}
		}
	}
}
