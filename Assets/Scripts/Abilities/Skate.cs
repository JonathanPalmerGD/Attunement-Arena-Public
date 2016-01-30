using UnityEngine;
using System.Collections;

public class Skate : Ability
{
	public GameObject icePrefab;
	public virtual KeyActivateCond activationCond
	{
		get { return KeyActivateCond.KeyHold; }
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
		get { return 5; }
	}
	public override float GeneralDamage
	{
		get
		{
			return 2f;
		}
	}
	public float RecoilForce = 140;
	public float Force = 5;
	public float Range = 10;
	public float MaxAngle = 15;

	public override void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
	{
		icePrefab = Resources.Load<GameObject>("icePrefab");
		
		base.Init(newOwner, newKeyBinding, displayKeyBinding);
	}

	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		//Make an icePrefab below the player.


	}
}
