using UnityEngine;
using System.Collections;

public class WaterShield : Ability
{
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

	public float damageReduction = .5f;
	public float KnockbackReduction = .5f;

	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		//If owner is shielded
		if (Owner.curStatus == Player.PlayerStatus.Shielded)
		{
			//Clear the shield
			Owner.SetPlayerStatus(Player.PlayerStatus.None);
			
			//Deal AOE damage and knockback
			//TODO
			Debug.Log("TODO: Make the Water Shield go KABOOOOM!\n");
		}
		else
		{
			//Add Shield
			Owner.SetPlayerStatus(Player.PlayerStatus.Shielded, Duration, true);
		}
	}
}
