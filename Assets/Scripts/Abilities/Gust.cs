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

	public override int IconID
	{
		get { return 0; }
	}

	public override float GeneralDamage
	{
		get
		{
			return 2f;
		}
	}
	public float RecoilForce = 290;
	public float JumpForce = 140;
	public float Force = 5;
	public float Range = 10;
	public float MaxAngle = 15;
	public bool wasGrounded = false;

	public override void UpdateAbility(float deltaTime)
	{
		if (Owner.controller.Grounded && !wasGrounded)
		{
			CurrentCooldown = 0.15f;
			wasGrounded = true;
		}
		else if(Owner.controller.Grounded && wasGrounded)
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

			Owner.controller.ApplyExternalForce(castDir * Force, false);
		}

		

		Vector3 oldVel = Owner.controller.mRigidBody.velocity;

		if (Vector3.Angle(castDir, Vector3.down) < 15)
		{
			Owner.controller.mRigidBody.velocity = new Vector3(oldVel.x, 0, oldVel.z);
			Owner.controller.ApplyExternalForce(castDir * JumpForce * -1, true);
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
				Owner.controller.AddExternalForce(castDir * RecoilForce * -1, ForceMode.Acceleration, false);
			}
		}
		
	}
}
