﻿using UnityEngine;
using System.Collections;

public class Gust : Ability
{
	public GameObject gustPrefab;

	public override bool UseCharges
	{
		get
		{
			return true;
		}
	}

	public override int IconID
	{
		get { return 4; }
	}
	public override int AlternateIconID
	{
		get { return 14; }
	}
	public override float GeneralDamage
	{
		get
		{
			return 5f;
		}
	}
	public float RecoilForce = 140;
	public float JumpForce = 140;
	public float Force = 175;
	public float Range = 10;
	public float MaxAngle = 15;
	public bool wasGrounded = false;

	public override void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
	{
		gustPrefab = Resources.Load<GameObject>("gustPrefab");

		base.Init(newOwner, newKeyBinding, displayKeyBinding);
	}

	public override void UpdateAbility(float deltaTime)
	{
		if (Charges == MaxCharges)
		{
			CurrentCooldown = 0;
		}
		else
		{
			if (Owner.controller.Grounded && !wasGrounded)
			{
				//CurrentCooldown = 0.15f;
				wasGrounded = true;
			}
			else if (Owner.controller.Grounded && wasGrounded)
			{
				//Debug.Log("Updating Gust - Grounded\n" + deltaTime);
				base.UpdateAbility(deltaTime);
			}
			else
			{
				//Debug.Log("Not Updating Gust - Not Grounded\n" + deltaTime);
			}
		}
	}

	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		var castDir = inputVector.normalized;

		CurrentCooldown = MaxCooldown;

		GameObject newGust = GameObject.Instantiate<GameObject>(gustPrefab);
		newGust.name = "[P" + Owner.playerID + "] Gust";
		newGust.transform.position = Owner.transform.position;
		newGust.transform.rotation = Quaternion.LookRotation(castDir);
		newGust.transform.SetParent(Owner.transform);
		GameObject.Destroy(newGust, .5f);

		//Debug.DrawLine(Owner.transform.position, Owner.transform.position + castDir * Range, Color.green, 5.0f);

		AudioManager.Instance.MakeSource("Gust").Play();

		foreach (Player p in GameManager.Instance.players)
		{
			if (p == Owner) continue; // Don't influence self just yet

			// Get vector from owner to other player
			var tetherVector = p.transform.position - Owner.transform.position;

			// If out of range, ignore
			if (tetherVector.sqrMagnitude > Range * Range) continue;

			// If the other player is not in the cone of influence, ignore
			if (Vector3.Angle(castDir, tetherVector) > MaxAngle) continue;

			//Debug.DrawRay(Owner.transform.position, p.transform.position - Owner.transform.position, Color.black, 5.0f);
			RaycastHit hit;
			if (Physics.Raycast(Owner.transform.position, p.transform.position - Owner.transform.position, out hit, Range))
			{
				//Debug.DrawRay(Owner.transform.position, p.transform.position - Owner.transform.position, Color.black, 5.0f);
				//Debug.Log(hit.collider.gameObject.name + "\n" + Owner.name + "   " + hit.collider.gameObject.tag);
				if (hit.collider.gameObject.tag == "Player" && hit.collider.name != Owner.name)
				{
					if (p.Grounded)
					{
						Vector3 knockbackVector = new Vector3(castDir.x, 0, castDir.z) + Vector3.up;
						p.controller.ApplyExternalForce(knockbackVector.normalized * Force);
					}
					else
					{
						p.controller.ApplyExternalForce(castDir * Force);
					}

					p.AdjustHealth(-GeneralDamage);
				}
			}
			//Debug.DrawLine(Owner.transform.position, Owner.transform.position + tetherVector, Color.red, 5.0f);
		}

		

		Vector3 oldVel = Owner.controller.mRigidBody.velocity;

		if (Vector3.Angle(castDir, Vector3.down) < 15)
		{
			Owner.controller.mRigidBody.velocity = new Vector3(oldVel.x, 0, oldVel.z);
			Owner.controller.ApplyExternalForce(castDir * JumpForce * -1, true, true);
			if (Owner.Grounded)
			{
				wasGrounded = false;
				Owner.controller.Jumping = true;
			}
		}
		else
		{
			if (!Owner.Grounded)
			{
				Owner.controller.mRigidBody.velocity = new Vector3(0, 0, 0);
				Owner.controller.ApplyExternalForce(castDir * RecoilForce * -1, false, true);
			}
		}
		
	}
}
