using UnityEngine;
using System.Collections;

public class WaterShield : Ability
{
	public GameObject shieldPrefab;
	public GameObject shieldParticle;

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

	public float Range
	{
		get
		{
			return 135f;
		}
	}

	public override void UpdateAbility(float deltaTime)
	{
		if (Owner.buffState == Player.PlayerBuff.Shielded)
		{

		}
		else
		{
			//Debug.Log("Updating Gust - Grounded\n" + deltaTime);
			base.UpdateAbility(deltaTime);
		}
	}

	public override void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
	{
		shieldPrefab = Resources.Load<GameObject>("shieldPrefab");
		shieldParticle = GameObject.Instantiate(shieldPrefab, newOwner.transform.position, newOwner.transform.rotation) as GameObject;
		shieldParticle.name = "Bolt Renderer [P" + newOwner.playerID + "]";
		
		shieldParticle.transform.SetParent(newOwner.transform);
		base.Init(newOwner, newKeyBinding, displayKeyBinding);
	}

	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		var castDir = inputVector.normalized;

		//Debug.DrawLine(Owner.transform.position, Owner.transform.position + castDir * Range, Color.green, 5.0f);

		if (Owner.buffState != Player.PlayerBuff.Shielded)
		{
			Owner.buffState = Player.PlayerBuff.Shielded;
		}
	}
}
