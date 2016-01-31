using UnityEngine;
using System.Collections;

public class Bolt : Ability
{
	public GameObject boltPrefab;
	public GameObject bolt;
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
		get { return 1; }
	}

	public override float GeneralDamage
	{
		get
		{
			return 8f;
		}
	}
	public float MaxAngle
	{
		get
		{
			return 5f;
		}
	}
	
	public float Range
	{
		get
		{
			return 135f;
		}
	}

	public override void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
	{
		boltPrefab = Resources.Load<GameObject>("boltPrefab");
		bolt = GameObject.Instantiate(boltPrefab, newOwner.transform.position, newOwner.transform.rotation) as GameObject;
		boltPrefab.name = "Bolt Renderer [P" + newOwner.playerID + "]";

		boltPrefab.transform.SetParent(newOwner.transform);

		base.Init(newOwner, newKeyBinding, displayKeyBinding);
	}

	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		var castDir = inputVector.normalized;

		Debug.DrawLine(Owner.transform.position, Owner.transform.position + castDir * Range, Color.green, 5.0f);

		foreach (Player p in GameManager.Instance.players)
		{
			if (p == Owner) continue; // Don't influence self

			// Get vector from owner to other player
			var tetherVector = Owner.transform.position - p.transform.position;

			// If out of range, ignore
			if (tetherVector.sqrMagnitude > Range * Range) continue;

			// If there's something in the way, ignore player
			if (Physics.Linecast(Owner.transform.position, p.transform.position)) continue;

			Debug.DrawLine(Owner.transform.position, Owner.transform.position + tetherVector, Color.red, 5.0f);

			// If the other player is not in the cone of influence, ignore
			if (Vector3.Angle(castDir, tetherVector) > MaxAngle) continue;

			Debug.DrawLine(Owner.transform.position, Owner.transform.position + tetherVector, Color.red, 5.0f);

			//Owner.controller.ApplyExternalForce(castDir * Force, false);
		}
	}
}
