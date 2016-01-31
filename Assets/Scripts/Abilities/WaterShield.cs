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
			//Debug.Log("Updating Gust - Grounded\n" + deltaTime);
			base.UpdateAbility(deltaTime);
		}
	}

	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		var castDir = inputVector.normalized;
		
		//Debug.DrawLine(Owner.transform.position, Owner.transform.position + castDir * Range, Color.green, 5.0f);

		Debug.Log("Shield Player\n");
		durCounter = Duration;
		Owner.buffState = Player.PlayerBuff.Shielded;
	}
}
