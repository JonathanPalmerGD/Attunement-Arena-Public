using UnityEngine;
using System.Collections;

public class Gust : Ability
{
	public GameObject gustPrefab;
	public GameObject groundPoundPrefab;

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
	public float GroundPoundForce = 280;
	public float Force = 175;
	public float Range = 10;
	public float MaxAngle = 15;
	public bool wasGrounded = false;
	public bool earthAligned = false;
	public bool groundPoundReady = false;

	public override void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
	{
		gustPrefab = Resources.Load<GameObject>("gustPrefab");
		groundPoundPrefab = Resources.Load<GameObject>("groundPoundPrefab");

		base.Init(newOwner, newKeyBinding, displayKeyBinding);
	}

	public override void UpdateAbility(float deltaTime)
	{
		if (earthAligned && groundPoundReady && Owner.controller.Grounded)
		{
			OnGroundImpact();
		}

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
						if (earthAligned)
						{
							GroundPound();
						}
						else
						{
							p.controller.ApplyExternalForce(castDir * Force);
						}
					}

					p.AdjustHealth(-GeneralDamage);
				}
			}
			//Debug.DrawLine(Owner.transform.position, Owner.transform.position + tetherVector, Color.red, 5.0f);
		}

		GustSelfResult(inputVector);
	}

	public void GustSelfResult(Vector3 inputVector = default(Vector3))
	{
		var castDir = inputVector.normalized;

		//If the ability is directed downward
		if (Vector3.Angle(castDir, Vector3.down) < 15)
		{
			if (Owner.Grounded)
			{
				wasGrounded = false;
				Owner.controller.Jumping = true;

				//Nerf our vertical velocity
				if (earthAligned)
				{
					AdjustVelocity(true);
				}
				else
				{
					AdjustVelocity();
				}
				//Jump
				Owner.controller.ApplyExternalForce(castDir * JumpForce * -1, true, true);
			}
			else
			{
				if (earthAligned)
				{
					GroundPound();
				}
				else
				{
					//Nerf our vertical velocity
					if (earthAligned)
					{
						AdjustVelocity(true);
					}
					else
					{
						AdjustVelocity();
					}
					//Jump
					Owner.controller.ApplyExternalForce(castDir * JumpForce * -1, true, true);
				}
			}
		}
		else
		{
			if (!Owner.Grounded)
			{
				if (earthAligned)
				{
					GroundPound();
				}
				else
				{
					Owner.controller.mRigidBody.velocity = Vector3.zero;
					Owner.controller.ApplyExternalForce(castDir * RecoilForce * -1, false, true);
				}
			}
		}
	}

	public void AdjustVelocity(bool amplifyVelocity = false)
	{
		Vector3 oldVel = Owner.controller.mRigidBody.velocity * (amplifyVelocity ? 2.5f : 1.0f);
		Owner.controller.mRigidBody.velocity = new Vector3(oldVel.x, 0, oldVel.z);
	}

	public void GroundPound()
	{
		//Debug.Log("Ground Pound\n");
		var castDir = Vector3.down;

		groundPoundReady = true;

		AdjustVelocity();
		Owner.controller.ApplyExternalForce(castDir * GroundPoundForce, true, true);
	}


	public void OnGroundImpact()
	{
		groundPoundReady = false;

		//Create particle effect.
		GameObject poundPrefab = GameObject.Instantiate<GameObject>(groundPoundPrefab);
		poundPrefab.transform.position = Owner.transform.position;
		GameObject.Destroy(poundPrefab, 2.0f);

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
					player.AdjustHealth(-1 * blastIntensityBasedOnDist * GeneralDamage);
				}
			}
		}
	}
}
