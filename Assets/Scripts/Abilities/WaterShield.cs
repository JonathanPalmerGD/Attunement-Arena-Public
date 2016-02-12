using UnityEngine;
using System.Collections;

public class WaterShield : Ability
{
	public GameObject burstPrefab;

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

	public float damageReduction = .5f;
	public float KnockbackReduction = .5f;
	public float Range = 10;
	public float Force = 200;

	public override void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
	{
		burstPrefab = Resources.Load<GameObject>("Effects/burstPrefab");
		base.Init(newOwner, newKeyBinding, displayKeyBinding);

		MaxCooldown = 8f;
		Cost = 15;
		GeneralDamage = 26f;
		Duration = 4f;

	}

	public override bool CanUseAbility()
	{
		return base.CanUseAbility();
	}

	public override void UpdateAbility(float deltaTime)
	{
		//If the owner is shielded
		if (Owner.curStatus == Player.PlayerStatus.Shielded)
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
		//If owner is shielded
		if (Owner.curStatus == Player.PlayerStatus.Shielded)
		{

			//Clear the shield
			Owner.SetPlayerStatus(Player.PlayerStatus.None);
			
			//Deal AOE damage and knockback
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

			GameObject newBurst = GameObject.Instantiate<GameObject>(burstPrefab);
			newBurst.name = "[P" + Owner.playerID + "] Hydro-Burst Prefab";
			newBurst.transform.position = Owner.transform.position;
			newBurst.transform.SetParent(Owner.transform);
			GameObject.Destroy(newBurst, 1.5f);
		}
		else
		{
			//Add Shield
			Owner.SetPlayerStatus(Player.PlayerStatus.Shielded, Duration, true);
		}
	}
}
