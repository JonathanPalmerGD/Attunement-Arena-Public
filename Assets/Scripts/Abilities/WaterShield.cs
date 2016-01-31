using UnityEngine;
using System.Collections;

public class WaterShield : Ability
{
	public GameObject waterShieldPrefab;
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

	private float durCounter = 0;

	public override void UpdateAbility(float deltaTime)
	{
		if (Owner.buffState == Player.PlayerBuff.Shielded)
		{
			//Debug.Log(durCounter + "\n");
			durCounter -= deltaTime;
			if(durCounter <= 0)
			{
				Owner.buffState = Player.PlayerBuff.None;
			}
		}
		else
		{
			if (shieldParticle.activeSelf)
			{
				shieldParticle.SetActive(false);
			}
			//Debug.Log("Updating Gust - Grounded\n" + deltaTime);
			base.UpdateAbility(deltaTime);
		}
	}

	public override void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
	{
		waterShieldPrefab = Resources.Load<GameObject>("waterShieldPrefab");
		shieldParticle = GameObject.Instantiate(waterShieldPrefab, newOwner.transform.position, newOwner.transform.rotation) as GameObject;
		shieldParticle.name = "Bolt Renderer [P" + newOwner.playerID + "]";
		
		shieldParticle.transform.SetParent(newOwner.transform);
		base.Init(newOwner, newKeyBinding, displayKeyBinding);
	}

	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		var castDir = inputVector.normalized;

		shieldParticle.SetActive(true);

		//Debug.DrawLine(Owner.transform.position, Owner.transform.position + castDir * Range, Color.green, 5.0f);

		if (Owner.buffState != Player.PlayerBuff.Shielded)
		{
			durCounter = Duration;
			Owner.buffState = Player.PlayerBuff.Shielded;
		}
	}
}
