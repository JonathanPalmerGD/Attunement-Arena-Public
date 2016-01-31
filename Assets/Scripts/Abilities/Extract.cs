using UnityEngine;
using System.Collections;

public class Extract : Ability
{
	public override int IconID
	{
		get
		{
			return 2;
		}
	}

	public override float Cost
	{
		get
		{
			return 0;
		}

		set
		{
			return;
		}
	}

	public float MaxRadius = 1.5f;
	public float AccretionSpeed = 0.5f;
	public float DamageMult = 1f;
	public float KnockbackMult = 1f;
	public float ProjSpeedMult = 1f;

	public enum ProjType
	{
		Air, Water, Lava
	}

	private Vector3 extractingFrom;
	private ProjType projType;

	private enum ExtractState
	{
		EmptyHanded, Pulling, FullHands
	}

	private ExtractState currState;
	private bool keepPulling = false;
	private LineRenderer extractBeam;

	private ExtractProj projectile;

	public override KeyActivateCond activationCond
	{
		get
		{
			return currState == ExtractState.FullHands ? KeyActivateCond.KeyDown : KeyActivateCond.KeyHold;
		}
	}

	public override void UpdateAbility(float deltaTime)
	{
		base.UpdateAbility(deltaTime);
		if (currState == ExtractState.Pulling)
			if (!keepPulling || projectile.transform.localScale.x >= MaxRadius)
			{
				Debug.Log("No longer Extracting!");
				if (extractBeam) Destroy(extractBeam);
				currState = ExtractState.FullHands;
			}
			else
			{
				if (extractBeam) extractBeam.SetPosition(1, projectile.transform.position);
				keepPulling = false;
			}
	}

	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		if (currState == ExtractState.EmptyHanded)
		{
			if (Owner.hitscanTarget && Owner.hitscanTarget.CompareTag("Stream"))
			{
				if (Owner.hitscanTarget.name.StartsWith("Lava"))
				{
					projType = ProjType.Lava;
				}
				else if (Owner.hitscanTarget.name.StartsWith("Water"))
				{
					projType = ProjType.Water;
				}
				else
				{
					projType = ProjType.Air;
				}
			}
			else
			{
				projType = ProjType.Air;
			}

			projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<ExtractProj>();

			projectile.Init(this, projType);

			if (projType != ProjType.Air)
			{
				extractBeam = projectile.gameObject.AddComponent<LineRenderer>();
				extractBeam.material = projectile.GetComponent<MeshRenderer>().material;
				extractBeam.SetVertexCount(2);
				extractBeam.SetPosition(0, Owner.hitscanContact);
				extractBeam.SetPosition(1, projectile.transform.position);
				Color clr = projectile.GetComponent<MeshRenderer>().material.color;
				extractBeam.SetColors(clr, clr);
				extractBeam.SetWidth(AccretionSpeed * 0.25f, AccretionSpeed * 0.33f);
			}

			currState = ExtractState.Pulling;
			keepPulling = true;
		}
		else if (currState == ExtractState.Pulling)
		{
			projectile.transform.localScale = Vector3.one * Mathf.Min(projectile.transform.localScale.x + (AccretionSpeed * Time.deltaTime), MaxRadius);
			keepPulling = true;
		}
		else if (currState == ExtractState.FullHands)
		{
			projectile.Throw(inputVector);
			projectile = null;
			currState = ExtractState.EmptyHanded;
		}
	}
}
