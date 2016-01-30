using UnityEngine;
using System.Collections;

public class Gust : Ability
{
	public override bool UseCharges
	{
		get
		{
			return true;
		}
	}

	public override float GeneralDamage
	{
		get
		{
			return 2f;
		}
	}
	public float RecoilForce = 100;
	public float Force = 5;
	public float Range = 10;
	public float MaxAngle = 15;

	public override void UpdateAbility(float deltaTime)
	{
		if (Owner.controller.Grounded)
		{
			//Debug.Log("Updating Gust - Grounded\n" + deltaTime);
			base.UpdateAbility(deltaTime);
		}
		else
		{
			//Debug.Log("Not Updating Gust - Not Grounded\n" + deltaTime);
		}
	}

	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		var castDir = inputVector.normalized;

		Debug.DrawLine(Owner.transform.position, Owner.transform.position + castDir * Range, Color.green, 5.0f);

		foreach (Player p in GameManager.Instance.players)
		{
			if (p == Owner) continue; // Don't influence self just yet

			// Get vector from owner to other player
			var tetherVector = Owner.transform.position - p.transform.position;

			// If out of range, ignore
			if (tetherVector.sqrMagnitude > Range * Range) continue;

			// If there's something in the way, ignore player
			if (Physics.Linecast(Owner.transform.position, p.transform.position)) continue;

			// If the other player is not in the cone of influence, ignore
			if (Vector3.Angle(castDir, tetherVector) > MaxAngle) continue;

			Debug.DrawLine(Owner.transform.position, Owner.transform.position + tetherVector, Color.red, 5.0f);

			p.SendMessage("ApplyExternalForce", castDir * Force);
		}

		if (Owner.Grounded)
		{
			Owner.controller.Jumping = true;
		}

		if (Vector3.Angle(castDir, Vector3.down) < 15 || !Owner.Grounded)
		{
			Vector3 oldVel = Owner.controller.mRigidBody.velocity;
			Owner.controller.mRigidBody.velocity = new Vector3(oldVel.x, 0, oldVel.z);
			Owner.SendMessage("ApplyExternalForce", castDir * RecoilForce * -1);
		}
	}
}
