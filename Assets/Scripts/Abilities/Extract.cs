using UnityEngine;
using System.Collections;

public class Extract : Ability
{
	private GameObject ExProj;
	public AudioSource extractAud;

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

	public bool Automated = true;
	public float MaxRadius = 1.5f;
	public float AccretionSpeed = 0.5f;
	public float DamageMult = 1f;
	public float KnockbackMult = 1f;
	public float ProjSpeedMult = 1f;
	public float ProjSpreadMult = 1f;

	private enum ExtractState
	{
		EmptyHanded, Pulling, FullHands, JustThrew
	}

	private ExtractState currState;
	private bool lastHeld = false;
	private bool showPull = false;
	private LineRenderer extractBeam;

	private ExtractProj projectile;

	public override KeyActivateCond activationCond
	{
		get
		{
			return (Automated || currState == ExtractState.FullHands) ? KeyActivateCond.KeyDown : KeyActivateCond.KeyHold;
		}
	}

	public override void Init(Player newOwner, string newKeyBinding, string displayKeyBinding)
	{
		extractAud = AudioManager.Instance.MakeSource("Extract");

		extractAud.volume = 0;
		extractAud.loop = true;
		extractAud.Play();

		base.Init(newOwner, newKeyBinding, displayKeyBinding);
	}

	public override void UpdateAbility(float deltaTime)
	{
		base.UpdateAbility(deltaTime);
		CurrentCooldown = 0;
		MaxCooldown = 1000;
		
		if (ExProj == null) ExProj = Resources.Load<GameObject>("ExProj");
		if (currState == ExtractState.Pulling)
		{
			extractAud.volume = .35f;
			projectile.transform.localScale = Vector3.one * Mathf.Min(projectile.transform.localScale.x + (AccretionSpeed * Time.deltaTime), MaxRadius * 2);
			if (Automated)
			{
				if ((projectile.Radius) >= MaxRadius)
				{
					showPull = false;
					currState = ExtractState.FullHands;
				}
			}
			else
			{
				if (!lastHeld || (projectile.Radius) >= MaxRadius)
				{
					showPull = false;
					currState = ExtractState.FullHands;
				}
				else
				{
					showPull = true;
					lastHeld = false;
				}
			}
		}
		else if (currState == ExtractState.FullHands)
			projectile.transform.localEulerAngles = Owner.myCamera.transform.localEulerAngles;
		else if (currState == ExtractState.JustThrew)
			if (!lastHeld)
			{
				currState = ExtractState.EmptyHanded;
			}
			else
			{
				lastHeld = false;
			}

		if (currState != ExtractState.Pulling)
		{
			extractAud.volume = 0f;
		}

		if (extractBeam && showPull)
		{
			extractBeam.SetPosition(1, projectile.transform.position);
		}
		else
		{
			if (projectile && projectile.GetComponent<RenderBallisticPath>().enabled ^ currState == ExtractState.FullHands)
				projectile.GetComponent<RenderBallisticPath>().enabled = currState == ExtractState.FullHands;
		}
	}

	public override void ExecuteAbility(Vector3 inputVector = default(Vector3))
	{
		CurrentCooldown = 0;

		if (currState == ExtractState.EmptyHanded)
		{
			var projType = ExtractProj.ProjType.Air;
			if (Owner.hitscanTarget && Owner.hitscanTarget.CompareTag("Stream"))
			{
				if (Owner.hitscanTarget.name.StartsWith("Lava"))
				{
					projType = ExtractProj.ProjType.Lava;
				}
				else if (Owner.hitscanTarget.name.StartsWith("Water"))
				{
					projType = ExtractProj.ProjType.Water;
				}
			}

			projectile = GameObject.Instantiate<GameObject>(ExProj).GetComponent<ExtractProj>();

			projectile.Init(this, projType);

			if (projType != ExtractProj.ProjType.Air)
			{
				extractBeam = projectile.GetComponent<LineRenderer>();
				extractBeam.SetVertexCount(2);
				extractBeam.SetPosition(0, Owner.hitscanContact);
				extractBeam.SetPosition(1, projectile.transform.position);
				extractBeam.SetWidth(AccretionSpeed, AccretionSpeed * 0.25f);
				showPull = true;
			}

			currState = ExtractState.Pulling;
			lastHeld = !Automated;
		}
		else if (currState == ExtractState.Pulling)
		{
			if (Automated)
			{
				projectile.transform.localEulerAngles = Owner.myCamera.transform.localEulerAngles;
				projectile.Throw();
				projectile = null;
				extractBeam = null;
				lastHeld = true;
				showPull = false;
				currState = ExtractState.JustThrew;
			}
			else
			{
				lastHeld = true;
			}
		}
		else if (currState == ExtractState.FullHands)
		{
			projectile.Throw();
			projectile = null;
			extractBeam = null;
			lastHeld = true;
			showPull = false;
			currState = ExtractState.JustThrew;
		}
		else if (currState == ExtractState.JustThrew)
		{
			lastHeld = true;
		}
	}
}
